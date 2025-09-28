"""
Persian:
    SessionManager نسخهٔ نهایی (async). این فایل مدیریت چرخهٔ حیات سشن‌ها را انجام می‌دهد:
    - ایجاد/خواندن/بروزرسانی/حذف سشن در DB (SessionDB)
    - نگهداری runtime clients در حافظه (BaseInstaClientWrapper instances)
    - dump/load سشن‌ها به صورت base64 در session_blob (بدون استفاده از helperهای encrypt/decrypt)
    - هماهنگی با proxy و probe مدیاها
    - همه متدهای IO/شبکه‌ای async هستند یا از حالت sync پشتیبانی می‌کنند
    - متد export_session برای خروجی امن و سازگار جهت ارسال به کلاینت اضافه شده است.

English:
    Final async SessionManager. Responsibilities:
    - CRUD session metadata in DB (SessionDB)
    - keep runtime client instances in memory (BaseInstaClientWrapper)
    - persist session bytes as base64 in session_blob (no encrypt/decrypt helpers used here)
    - apply proxy and run media probes
    - methods are async and handle underlying sync client methods safely
    - export_session added to provide a safe, client-friendly session snapshot.
"""

from typing import Optional, List, Dict, Any
from datetime import datetime
import asyncio
import base64
import traceback
import uuid
import logging

from sqlmodel import Session, select
from app.db import engine
from app.models.session_db_model import SessionDB
from app.i18n import translate
from app.utils.retry import retry_with_backoff
from app.config import PROBE_RETRY_ATTEMPTS, PROBE_RETRY_BACKOFF_BASE
from app.services.insta_client_factory import BaseInstaClientWrapper, InstaClientFactory
from app.services.telemetry_service import incr

logger = logging.getLogger(__name__)


class SessionManager:
    """
    Persian:
        SessionManager async: مدیریت ایجاد/خواندن/به‌روز‌رسانی/حذف سشن و عملیات مرتبط با client runtime.

    English:
        Async SessionManager: manage create/read/update/delete sessions and runtime clients.
    """

    def __init__(self, client_factory=InstaClientFactory):
        # runtime clients: session_id -> BaseInstaClientWrapper instance
        self.clients: Dict[str, BaseInstaClientWrapper] = {}
        # client_factory should be a factory class exposing create_new()
        self.client_factory = client_factory
        # per-session asyncio locks to avoid races
        self._locks: Dict[str, asyncio.Lock] = {}

    # ----------------------------
    # DB helpers (sync interactions)
    # ----------------------------
    def _get_session_db(self, session_id: str) -> Optional[SessionDB]:
        with Session(engine) as db:
            return db.get(SessionDB, session_id)

    def _upsert_session_db(self, sess: SessionDB) -> SessionDB:
        with Session(engine) as db:
            db.add(sess)
            db.commit()
            db.refresh(sess)
            return sess

    # ----------------------------
    # client lifecycle (async-safe)
    # ----------------------------
    async def _ensure_client(self, session_id: str) -> BaseInstaClientWrapper:
        """
        Persian:
            اطمینان حاصل می‌کند که یک client runtime برای session_id وجود دارد و آن را برمی‌گرداند.
            این متد با lock داخلی از ایجاد clientهای تکراری جلوگیری می‌کند.

        English:
            Ensure a runtime client exists for session_id and return it. Uses per-session lock
            to avoid concurrent creation races.
        """
        lock = self._locks.setdefault(session_id, asyncio.Lock())
        async with lock:
            if session_id in self.clients:
                return self.clients[session_id]

            sess = self._get_session_db(session_id)
            if not sess:
                raise RuntimeError("session not found")

            # Instantiate a new client via factory. Expect factory to expose create_new().
            client_wrapper: Optional[BaseInstaClientWrapper] = None
            try:
                create_new = getattr(self.client_factory, "create_new", None)
                if create_new is None:
                    # fallback: if client_factory is a wrapper instance/class without create_new, try to instantiate
                    client_wrapper = self.client_factory()
                else:
                    # call create_new (may be sync)
                    client_wrapper = create_new()
                    # if returns coroutine, await it
                    if asyncio.iscoroutine(client_wrapper):
                        client_wrapper = await client_wrapper
            except Exception as e:
                logger.exception("Failed to create client wrapper: %s", e)
                # fallback: try BaseInstaClientWrapper.create_new if available
                try:
                    client_wrapper = InstaClientFactory.create_new()
                except Exception as e2:
                    logger.exception("Fallback client creation failed: %s", e2)
                    raise RuntimeError(f"failed to create insta client: {e2}")

            # If DB has a session_blob, attempt to load it into client
            if sess.session_blob and client_wrapper is not None:
                try:
                    # decrypt and load session bytes
                    from app.services.insta_client_factory import decrypt_session_token
                    raw = decrypt_session_token(sess.session_blob)
                    load_fn = getattr(client_wrapper, "load_session_bytes", None)
                    if asyncio.iscoroutinefunction(load_fn):
                        await load_fn(raw)
                    elif callable(load_fn):
                        load_fn(raw)
                except Exception as e:
                    logger.exception("Failed to load session blob into client: %s", e)
                    # ignore load failures; client will require login
                    pass

            # store in runtime map
            self.clients[session_id] = client_wrapper
            # update telemetry
            incr("active_clients")
            return client_wrapper

    # ----------------------------
    # CRUD on session (async-friendly interface)
    # ----------------------------
    async def create_session(
        self,
        account_id: str,
        proxy: Optional[str] = None,
        proxy_enabled: bool = False,
        locale: str = "en",
    ) -> SessionDB:
        """
        Persian:
            یک رکورد سشن جدید ایجاد می‌کند و یک client runtime آماده می‌سازد (در حافظه).

        English:
            Create a new session DB record and prepare a runtime client (kept in memory).
        """
        sid = str(uuid.uuid4())
        now = datetime.utcnow()
        session_db = SessionDB(
            id=sid,
            account_id=account_id,
            proxy=proxy,
            proxy_enabled=proxy_enabled,
            last_media_check=None,
            locale_preference=locale,
            session_blob=None,
            created_at=now,
            updated_at=now,
        )
        self._upsert_session_db(session_db)

        # create runtime client using factory
        try:
            client = self.client_factory.create_new() if hasattr(self.client_factory, "create_new") else self.client_factory()
            if asyncio.iscoroutine(client):
                client = await client
        except Exception as e:
            logger.exception("Failed initial client creation for new session: %s", e)
            # fallback: try factory static method
            client = InstaClientFactory.create_new()

        self.clients[sid] = client
        return session_db

    async def get_session(self, session_id: str) -> Optional[SessionDB]:
        return self._get_session_db(session_id)

    async def update_session(self, session_id: str, **fields) -> SessionDB:
        with Session(engine) as db:
            sess = db.get(SessionDB, session_id)
            if not sess:
                raise RuntimeError("session not found")
            for k, v in fields.items():
                setattr(sess, k, v)
            sess.updated_at = datetime.utcnow()
            db.add(sess)
            db.commit()
            db.refresh(sess)
            return sess

    async def delete_session(self, session_id: str):
        with Session(engine) as db:
            sess = db.get(SessionDB, session_id)
            if sess:
                db.delete(sess)
                db.commit()
        # remove runtime client (best-effort)
        self.clients.pop(session_id, None)

    # ----------------------------
    # proxy apply & persist session blob
    # ----------------------------
    async def apply_proxy_if_enabled(self, session_id: str):
        """
        Persian:
            اگر proxy_enabled باشد، proxy مربوطه را روی client runtime اعمال می‌کند.

        English:
            If proxy_enabled for session, apply the proxy to the runtime client.
        """
        sess = self._get_session_db(session_id)
        if not sess:
            raise RuntimeError("session not found")
        # Only attempt to set proxy if proxy_enabled True and proxy value present
        if not sess.proxy_enabled or not sess.proxy:
            logger.debug("Proxy not enabled or not set for %s; skipping set_proxy", session_id)
            return

        client = await self._ensure_client(session_id)
        if client is None:
            logger.debug("No client available for %s when applying proxy", session_id)
            return

        setp = getattr(client, "set_proxy", None)
        if not callable(setp):
            logger.debug("Client for %s does not support set_proxy; skipping", session_id)
            return

        try:
            # call sync or async set_proxy
            if asyncio.iscoroutinefunction(setp):
                await setp(sess.proxy)
            else:
                setp(sess.proxy)
        except NotImplementedError:
            # client indicates set_proxy not supported; log and continue
            logger.warning("set_proxy not implemented by client for session %s", session_id)
        except Exception as e:
            logger.exception("Failed to apply proxy for %s: %s", session_id, e)
            # ignore proxy apply errors to avoid internal failures
            return

    async def persist_client_session(self, session_id: str):
        """
        Persian:
            اگر client runtime قابلیت dump session_bytes دارد، آن را گرفته و به صورت base64
            در session_blob سیو می‌کند. این نسخه رمزنگاری انجام نمی‌دهد (session_blob ذخیره‌شده base64 است).

        English:
            If runtime client supports dump_session_bytes, capture it and store as base64 in session_blob.
            This does not encrypt the blob (it stores base64 of raw bytes).
        """
        client = self.clients.get(session_id)
        if not client:
            return
        dump_fn = getattr(client, "dump_session_bytes", None)
        if not callable(dump_fn):
            logger.debug("Client for %s has no dump_session_bytes", session_id)
            return
        try:
            raw = dump_fn() if not asyncio.iscoroutinefunction(dump_fn) else await dump_fn()
            if raw is None:
                return
            # ensure bytes
            b = raw if isinstance(raw, (bytes, bytearray)) else (str(raw).encode())
            # encrypt and store
            from app.services.insta_client_factory import encrypt_session_bytes
            token = encrypt_session_bytes(b)
            with Session(engine) as db:
                sess = db.get(SessionDB, session_id)
                if not sess:
                    return
                sess.session_blob = token
                sess.updated_at = datetime.utcnow()
                db.add(sess)
                db.commit()
                db.refresh(sess)
        except Exception as e:
            logger.exception("Failed to persist session for %s: %s", session_id, e)
            # swallow to avoid breaking caller flows; optionally log
            pass

    # ----------------------------
    # export session snapshot (for API responses)
    # ----------------------------
    def export_session(self, session_id: str) -> Dict[str, Any]:
        """
        Persian:
            یک خروجی امن و سبک از وضعیت سشن برای فرستادن به کلاینت.
            هیچ دادهٔ حساس (مثل رمز عبور) برنمی‌گرداند و session_blob هم برنمی‌گردد.

        English:
            Return a safe, lightweight snapshot of the session for client responses.
            Does not include sensitive data; session_blob is omitted.
        """
        sess = self._get_session_db(session_id)
        if not sess:
            raise RuntimeError("session not found")

        client = self.clients.get(session_id)
        client_obj = getattr(client, "client", None) if client else None

        # Try to extract a stable user id if available
        uid = None
        if client_obj is not None:
            uid = getattr(client_obj, "user_id", None) or getattr(client_obj, "user_pk", None)

        return {
            "id": sess.id,
            "account_id": sess.account_id,
            "proxy_enabled": bool(sess.proxy_enabled),
            "proxy": sess.proxy if sess.proxy_enabled else None,
            "locale": sess.locale_preference,
            "last_media_check": sess.last_media_check.isoformat() if sess.last_media_check else None,
            "created_at": sess.created_at.isoformat() if sess.created_at else None,
            "updated_at": sess.updated_at.isoformat() if sess.updated_at else None,
            "runtime": {
                "has_client": client is not None,
                "user_id": uid,
            },
        }

    # ----------------------------
    # media probe (with retry) and login flow
    # ----------------------------
    @retry_with_backoff(PROBE_RETRY_ATTEMPTS, PROBE_RETRY_BACKOFF_BASE)
    async def probe_medias(self, session_id: str, uid: str, amount: int = 5) -> List[dict]:
        """
        Persian:
            پروب مدیاها (مثلاً user_medias) با تلاش مجدد. در هر موفقیت متریک probe_calls را افزایش می‌دهد.

        English:
            Probe user medias with retry. Increments probe_calls metric on successful attempt.
        """
        # ensure proxy applied if enabled; apply_proxy_if_enabled now safely returns when proxy disabled
        await self.apply_proxy_if_enabled(session_id)
        client = await self._ensure_client(session_id)

        user_medias_fn = getattr(client, "user_medias", None)
        try:
            if asyncio.iscoroutinefunction(user_medias_fn):
                medias = await user_medias_fn(uid, amount)
            else:
                medias = user_medias_fn(uid, amount)
        except Exception as e:
            logger.exception("user_medias failed for %s: %s", session_id, e)
            # bubble up to retry wrapper
            raise
        # update last_media_check timestamp
        with Session(engine) as db:
            s = db.get(SessionDB, session_id)
            if s:
                s.last_media_check = datetime.utcnow()
                s.updated_at = datetime.utcnow()
                db.add(s)
                db.commit()
        incr("probe_calls")
        return medias

    async def login_and_probe(self, session_id: str, username: str, password: str) -> Dict[str, Any]:
        """
        Persian:
            عملیات لاگین و سپس پروب مدیا. در صورت نیاز به چالش یا 2FA، نتیجه شامل challenge_token خواهد بود.
            در صورت موفقیت، session dump گرفته و ذخیره می‌شود.

        English:
            Perform login and then probe medias. On challenge/2FA, returns challenge_token.
            On success, persists session dump (base64) to DB.
        """
        # apply proxy only if enabled
        await self.apply_proxy_if_enabled(session_id)
        client = await self._ensure_client(session_id)
        login_fn = getattr(client, "login", None)

        if not callable(login_fn):
            logger.warning("Client for %s does not support login", session_id)
            return {
                "ok": False,
                "message": translate("login.no_client_login", "fa"),
                "message_en": translate("login.no_client_login", "en"),
                "error_detail": "client has no login method"
            }

        try:
            if asyncio.iscoroutinefunction(login_fn):
                login_res = await login_fn(username, password)
            else:
                login_res = login_fn(username, password)
        except NotImplementedError as e:
            tb = traceback.format_exc()
            logger.exception("Login not implemented for session %s: %s", session_id, e)
            return {
                "ok": False,
                "message": translate("login.internal_error", "fa"),
                "message_en": translate("login.internal_error", "en"),
                "error_detail": "login not implemented",
                "exception": e.__class__.__name__,
                "traceback": tb
            }
        except Exception as e:
            tb = traceback.format_exc()
            logger.exception("Exception while performing client.login for session %s: %s", session_id, e)
            return {
                "ok": False,
                "error": {"code": "login_exception", "detail": str(e)},
                "message": translate("login.internal_error", "fa"),
                "message_en": translate("login.internal_error", "en"),
                "error_detail": str(e),
                "exception": e.__class__.__name__,
                "traceback": tb
            }

        # handle common shapes returned by clients
        if isinstance(login_res, dict):
            if login_res.get("two_factor_required") or login_res.get("two_factor"):
                from app.services import challenge_store_redis as challenge_store
                info = login_res.get("two_factor_info") or login_res
                token = await challenge_store.create_challenge(session_id, "two_factor_required", {"info": info})
                return {"ok": False, "challenge_required": True, "challenge_token": token, "info": info}
            if login_res.get("challenge_required") or login_res.get("challenge_options"):
                from app.services import challenge_store_redis as challenge_store
                info = login_res.get("challenge_options") or login_res
                token = await challenge_store.create_challenge(session_id, "challenge_required", {"info": info})
                return {"ok": False, "challenge_required": True, "challenge_token": token, "info": info}

        try:
            await self.persist_client_session(session_id)
        except Exception as e:
            logger.exception("Failed to persist session for %s: %s", session_id, e)
            # continue even if persist fails

        uid = None
        if isinstance(login_res, dict):
            uid = login_res.get("user_id") or login_res.get("pk") or login_res.get("user_id")
        if not uid:
            client_obj = getattr(client, "client", None)
            uid = getattr(client_obj, "user_id", None) or getattr(client_obj, "user_pk", None)
        if not uid:
            uid = f"user_{username}"

        try:
            medias = await self.probe_medias(session_id, uid, 5)
        except Exception as e:
            tb = traceback.format_exc()
            logger.exception("Probe medias failed for %s: %s", session_id, e)
            return {
                "ok": False,
                "message": translate("network.error", "fa"),
                "message_en": translate("network.error", "en"),
                "error_detail": str(e),
                "exception": e.__class__.__name__,
                "traceback": tb
            }
        needs_investigation = len(medias) == 0
        sess = self._get_session_db(session_id)
        message_key = "probe.empty_medias" if needs_investigation else "health.ok"
        localized = translate(message_key, sess.locale_preference if sess else "fa", uid=uid)
        localized_en = translate(message_key, "en", uid=uid)

        if not (isinstance(login_res, dict) and login_res.get("authenticated", False)):
            fa_msg = translate("login.bad_password", "fa")
            en_msg = translate("login.bad_password", "en")
            if isinstance(login_res, dict):
                if login_res.get("message"):
                    fa_msg = login_res["message"]
                if login_res.get("message_en"):
                    en_msg = login_res["message_en"]
                if login_res.get("error_type") == "checkpoint_challenge_required":
                    fa_msg = translate("login.blocked", "fa")
                    en_msg = translate("login.blocked", "en")
            return {
                "ok": False,
                "medias_count": len(medias),
                "message": fa_msg,
                "message_en": en_msg,
                "login_result": login_res,
                "error_detail": login_res.get("error_detail") if isinstance(login_res, dict) else str(login_res)
            }

        session_snapshot = self.export_session(session_id)

        return {
            "ok": True,
            "login": login_res,
            "medias_count": len(medias),
            "needs_investigation": needs_investigation,
            "message": localized,
            "message_en": localized_en,
            "session": session_snapshot,
        }
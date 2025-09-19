"""
Persian:
    SessionManager نسخهٔ async: مدیریت سشن‌ها با persistence در DB، نگهداری client runtime،
    پشتیبانی از dump/load سشن رمزنگاری‌شده و تعامل async با Redis-backed challenge store.

English:
    Async SessionManager: manage persisted sessions, keep runtime clients, support encrypted session dump/load,
    and interact asynchronously with Redis-backed challenge store.
"""

from typing import Optional, List, Dict, Any
from app.models.session_db_model import SessionDB
from app.i18n import translate
from app.utils.retry import retry_with_backoff
from app.config import PROBE_RETRY_ATTEMPTS, PROBE_RETRY_BACKOFF_BASE
from sqlmodel import Session, select
from app.db import engine
import uuid
from datetime import datetime
import json
import asyncio

from app.services.insta_client_factory import InstaClientWrapper, encrypt_session_bytes, decrypt_session_token
from app.services import challenge_store_redis as challenge_store  # async store
from app.services.telemetry_service import incr

# Note: InstaClientWrapper.create_new may raise if instagrapi not installed.
class SessionManager:
    """
    Persian:
        SessionManager async: عملیات ایجاد/خواندن/به‌روزرسانی/حذف سشن و انجام login+probe به‌صورت ناهمگام.

    English:
        Async SessionManager: async create/read/update/delete sessions and perform login+probe flows.
    """
    def __init__(self, client_factory=InstaClientWrapper):
        self.clients: Dict[str, InstaClientWrapper] = {}
        self.client_factory = client_factory
        # lock to guard client creation per session id
        self._locks: Dict[str, asyncio.Lock] = {}

    # ----------------------------
    # DB helpers (sync)
    # ----------------------------
    def _get_session_db(self, session_id: str) -> Optional[SessionDB]:
        with Session(engine) as db:
            return db.get(SessionDB, session_id)

    def _upsert_session_db(self, sess: SessionDB):
        with Session(engine) as db:
            db.add(sess)
            db.commit()
            db.refresh(sess)
            return sess

    # ----------------------------
    # client lifecycle (async-safe)
    # ----------------------------
    async def _ensure_client(self, session_id: str) -> InstaClientWrapper:
        # per-session lock to prevent concurrent create/load races
        lock = self._locks.setdefault(session_id, asyncio.Lock())
        async with lock:
            if session_id in self.clients:
                return self.clients[session_id]
            sess = self._get_session_db(session_id)
            if not sess:
                raise RuntimeError("session not found")
            # create client wrapper instance (may be sync internal)
            client_wrapper = None
            try:
                # create_new may be sync or async; call accordingly
                create_new = getattr(self.client_factory, "create_new", None)
                if asyncio.iscoroutinefunction(create_new):
                    client_wrapper = await create_new()
                elif callable(create_new):
                    client_wrapper = create_new()
                else:
                    client_wrapper = self.client_factory()
            except Exception:
                # fallback to factory() call
                client_wrapper = self.client_factory()

            # load session blob if exists
            if sess.session_blob:
                try:
                    raw = decrypt_session_token(sess.session_blob)
                    # load_session_bytes may be sync; support both
                    load_fn = getattr(client_wrapper, "load_session_bytes", None)
                    if asyncio.iscoroutinefunction(load_fn):
                        await load_fn(raw)
                    elif callable(load_fn):
                        load_fn(raw)
                except Exception:
                    # If load fails, proceed with fresh client (login required)
                    pass

            self.clients[session_id] = client_wrapper
            return client_wrapper

    # ----------------------------
    # CRUD on session (async-friendly interface)
    # ----------------------------
    async def create_session(self, account_id: str, proxy: Optional[str]=None, proxy_enabled: bool=False, locale: str="en") -> SessionDB:
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
            updated_at=now
        )
        self._upsert_session_db(session_db)
        # create runtime client
        client = None
        create_new = getattr(self.client_factory, "create_new", None)
        try:
            if asyncio.iscoroutinefunction(create_new):
                client = await create_new()
            elif callable(create_new):
                client = create_new()
            else:
                client = self.client_factory()
        except Exception:
            client = self.client_factory()
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
        # remove runtime client
        self.clients.pop(session_id, None)

    # ----------------------------
    # proxy apply & persist session blob
    # ----------------------------
    async def apply_proxy_if_enabled(self, session_id: str):
        sess = self._get_session_db(session_id)
        if not sess:
            raise RuntimeError("session not found")
        client = await self._ensure_client(session_id)
        # set_proxy may be sync
        setp = getattr(client, "set_proxy", None)
        if asyncio.iscoroutinefunction(setp):
            await setp(sess.proxy if sess.proxy_enabled else None)
        elif callable(setp):
            setp(sess.proxy if sess.proxy_enabled else None)

    async def persist_client_session(self, session_id: str):
        """
        Dump session bytes from client, encrypt and store into DB.session_blob
        """
        client = self.clients.get(session_id)
        if not client:
            return
        dump_fn = getattr(client, "dump_session_bytes", None)
        if not dump_fn:
            return
        try:
            raw = dump_fn() if not asyncio.iscoroutinefunction(dump_fn) else await dump_fn()
            token = encrypt_session_bytes(raw)
            # store in DB
            with Session(engine) as db:
                sess = db.get(SessionDB, session_id)
                if not sess:
                    return
                sess.session_blob = token
                sess.updated_at = datetime.utcnow()
                db.add(sess)
                db.commit()
                db.refresh(sess)
        except Exception:
            # do not raise to avoid breaking flows
            pass

    # ----------------------------
    # media probe (with retry) and login flow
    # ----------------------------
    @retry_with_backoff(PROBE_RETRY_ATTEMPTS, PROBE_RETRY_BACKOFF_BASE)
    async def probe_medias(self, session_id: str, uid: str, amount: int = 5) -> List[dict]:
        await self.apply_proxy_if_enabled(session_id)
        client = await self._ensure_client(session_id)
        # user_medias may be sync or async
        user_medias_fn = getattr(client, "user_medias", None)
        if asyncio.iscoroutinefunction(user_medias_fn):
            medias = await user_medias_fn(uid, amount)
        else:
            medias = user_medias_fn(uid, amount)
        # update last_media_check
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
        # ensure client
        await self.apply_proxy_if_enabled(session_id)
        client = await self._ensure_client(session_id)
        login_fn = getattr(client, "login", None)
        try:
            login_res = await login_fn(username, password) if asyncio.iscoroutinefunction(login_fn) else login_fn(username, password)
        except Exception as e:
            # map exceptions using insta_error_map if needed; here simple fallback
            return {"ok": False, "error": "login_exception", "detail": str(e)}

        # check for challenge/two-factor markers in response
        if isinstance(login_res, dict) and (login_res.get("two_factor_required") or login_res.get("challenge_required")):
            # create challenge in async store
            info = login_res.get("two_factor_info") or login_res.get("challenge_options") or {}
            token = await challenge_store.create_challenge(session_id, "challenge", {"info": info})
            return {"ok": False, "challenge_required": True, "challenge_token": token, "info": info}

        # persist session bytes if available
        try:
            await self.persist_client_session(session_id)
        except Exception:
            pass

        # determine uid
        uid = login_res.get("user_id") if isinstance(login_res, dict) else None
        if not uid:
            # try inspect client
            uid = getattr(client.client, "user_id", None) or getattr(client.client, "user_pk", None) or f"user_{username}"

        medias = await self.probe_medias(session_id, uid, 5)
        needs_investigation = len(medias) == 0
        sess = self._get_session_db(session_id)
        message_key = "probe.empty_medias" if needs_investigation else "health.ok"
        localized = translate(message_key, sess.locale_preference if sess else "en", uid=uid)
        return {"ok": True, "login": login_res, "medias_count": len(medias), "needs_investigation": needs_investigation, "message": localized}

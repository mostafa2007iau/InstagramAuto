"""
Persian:
    هندلر لاگین واقعی با instagrapi:
    - تلاش برای لاگین با instagrapi.Client از طریق InstaClientFactory
    - نگاشت استثناها به کدهای API با استفاده از insta_error_map
    - در صورت نیاز به challenge یا 2FA، ایجاد چالش در Redis store و بازگشت توکن چالش
    - در صورت موفقیت، dump سشن و ذخیره encrypted session_blob از طریق SessionManager

English:
    Real login handler using instagrapi:
    - attempt login via InstaClientFactory
    - map exceptions to API codes using insta_error_map
    - create a Redis-backed challenge token on challenge/2FA
    - on success, persist encrypted session blob via SessionManager
"""

from app.services.insta_client_factory import InstaClientFactory, encrypt_session_bytes, decrypt_session_token
from app.services import challenge_store_redis as challenge_store
from app.utils.insta_error_map import map_instagram_exception
from app.i18n import translate
import asyncio
import traceback

# session_manager will be injected by caller (should be async SessionManager)
async def handle_login_flow(session_manager, session_id: str, username: str, password: str, locale: str = "en"):
    """
    Persian:
        سعی می‌کند لاگین را انجام دهد و پاسخ ساخت‌یافته برمی‌گرداند.
        خروجی:
          - on success: {"ok": True, "login": login_res}
          - on challenge required: {"ok": False, "challenge_required": True, "challenge_token": token}
          - on error: {"ok": False, "error": {...mapped...}}
    English:
        Attempts login and returns structured result as described above.
    """
    # Ensure client exists and proxy applied
    try:
        # ensure client instance available
        client = await session_manager._ensure_client(session_id)
    except Exception as e:
        return {"ok": False, "error": {"code": "session_missing", "message": translate("login.error", locale=locale, reason=str(e))}}

    # perform login
    login_fn = getattr(client, "login", None)
    if login_fn is None:
        return {"ok": False, "error": {"code": "no_client_login", "message": translate("login.error", locale=locale, reason="client has no login method")}}
    try:
        # login_fn may be sync or async
        if asyncio.iscoroutinefunction(login_fn):
            login_res = await login_fn(username, password)
        else:
            login_res = login_fn(username, password)
    except Exception as exc:
        mapped = map_instagram_exception(exc, locale=locale)
        # If exception indicates challenge or 2fa, create challenge token
        if mapped["code"] in ("challenge_required", "two_factor_required"):
            token = await challenge_store.create_challenge(session_id, mapped["code"], {"info": mapped.get("details")})
            return {"ok": False, "challenge_required": True, "challenge_token": token, "code": mapped["code"], "message": mapped["message"]}
        return {"ok": False, "error": mapped}

    # Interpret login_res shapes (instagrapi may return dict markers)
    if isinstance(login_res, dict):
        if login_res.get("two_factor_required") or login_res.get("two_factor"):
            token = await challenge_store.create_challenge(session_id, "two_factor_required", {"info": login_res})
            return {"ok": False, "challenge_required": True, "challenge_token": token, "code": "two_factor_required"}
        if login_res.get("challenge_required") or login_res.get("challenge_options"):
            token = await challenge_store.create_challenge(session_id, "challenge_required", {"info": login_res})
            return {"ok": False, "challenge_required": True, "challenge_token": token, "code": "challenge_required"}
        if login_res.get("status") in ("fail", "error"):
            # map common fail reasons if present
            err = login_res.get("error") or login_res.get("message") or "login_failed"
            return {"ok": False, "error": {"code": "login_failed", "message": translate("login.error", locale=locale, reason=str(err)), "details": login_res}}

    # If reached here, consider login successful. Persist session blob if possible.
    try:
        await session_manager.persist_client_session(session_id)
    except Exception:
        # do not fail login if persist fails, but include a warning in result
        return {"ok": True, "login": login_res, "warning": "persist_failed"}

    return {"ok": True, "login": login_res}

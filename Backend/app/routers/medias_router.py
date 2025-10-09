"""
Persian:
    Router async برای دریافت مدیاها با پشتیبانی cursor-based واقعی.
    endpoint فهرست مدیاها اکنون توسط rate-limit محافظت شده است.

English:
    Async router to fetch medias with real cursor-based pagination.
    The list endpoint is protected with a default rate-limit.
"""

from fastapi import APIRouter, Depends, HTTPException, Query, Request
from typing import Optional, List, Dict, Any
from app.deps import get_session_manager, get_locale, get_client_log_verbosity
from app.services.session_manager import SessionManager
from app.schemas.media_schema import MediaItem
from app.schemas.pagination_schema import PaginatedResponse, PaginationMeta
from app.utils.secure_cursor import sign, verify
from app.i18n import translate
from app.middleware.verbosity_middleware import default_rate_limit
import asyncio
import logging

router = APIRouter(prefix="/api/medias", tags=["medias"])

def _build_cursor_from_provider(provider_cursor: Optional[Any], account_id: str) -> str:
    payload = {"provider_cursor": provider_cursor, "account_id": account_id}
    return sign(payload)

def _decode_cursor_secure(token: Optional[str]) -> Dict[str, Any]:
    if not token:
        return {"provider_cursor": None, "account_id": None}
    try:
        return verify(token)
    except Exception:
        # invalid token -> treat as fresh cursor
        return {"provider_cursor": None, "account_id": None}

@router.get("", response_model=PaginatedResponse[MediaItem], summary="List medias / فهرست مدیاها", description="Return paged medias for a session/account / بازگرداندن مدیاها با صفحه‌بندی cursor-based")
async def list_medias(request: Request, session_id: str = Query(...), limit: int = Query(20, le=100), cursor: Optional[str] = Query(None), session_manager: SessionManager = Depends(get_session_manager), locale: str = Depends(get_locale), verbosity = Depends(get_client_log_verbosity), _rl=Depends(default_rate_limit)):
    sess = await session_manager.get_session(session_id)
    if not sess:
        raise HTTPException(status_code=404, detail="session not found")

    decoded = _decode_cursor_secure(cursor)
    provider_cursor = decoded.get("provider_cursor")
    cursor_account = decoded.get("account_id")
    if cursor_account and cursor_account != sess.account_id:
        # cursor belongs to different account: ignore it
        provider_cursor = None

    # ensure client
    client = await session_manager._ensure_client(session_id)

    # determine uid to pass to client methods
    # prefer runtime client internal id (user_id / user_pk) if available, otherwise use account_id as username
    uid = None
    try:
        client_obj = getattr(client, "client", None)
        if client_obj is not None:
            uid = getattr(client_obj, "user_id", None) or getattr(client_obj, "user_pk", None) or getattr(client_obj, "username", None)
    except Exception:
        uid = None
    if not uid:
        # do not prefix with 'user_'; client libraries usually accept username or numeric id
        uid = sess.account_id

    medias_items = []
    next_provider_cursor = None

    try:
        user_medias_fn = getattr(client, "get_user_medias_page", None)
        if user_medias_fn:
            # support sync or async
            logging.getLogger(__name__).debug("Calling get_user_medias_page with uid=%s provider_cursor=%s limit=%s", uid, provider_cursor, limit)
            if asyncio.iscoroutinefunction(user_medias_fn):
                medias_raw, next_provider_cursor = await user_medias_fn(uid, limit, provider_cursor)
            else:
                medias_raw, next_provider_cursor = user_medias_fn(uid, limit, provider_cursor)
        else:
            # fallback to user_medias
            um = getattr(client, "user_medias", None)
            if not um:
                raise HTTPException(status_code=500, detail="client does not support media retrieval")
            logging.getLogger(__name__).debug("Calling user_medias with uid=%s limit=%s", uid, limit)
            if asyncio.iscoroutinefunction(um):
                medias_raw = await um(uid, limit)
            else:
                medias_raw = um(uid, limit)

        logging.getLogger(__name__).debug("medias_raw returned type=%s count=%s", type(medias_raw), len(medias_raw) if medias_raw else 0)

        # If provider returned no medias, attempt a probe fallback which applies proxy/retries
        if not medias_raw:
            logging.getLogger(__name__).warning("medias endpoint returned empty from client; attempting probe_medias fallback for session %s", session_id)
            try:
                # probe_medias expects a uid; reuse same uid variable
                probe_result = await session_manager.probe_medias(session_id, uid, limit)
                if probe_result:
                    medias_raw = probe_result
            except Exception as e:
                logging.getLogger(__name__).exception("probe_medias fallback failed for %s: %s", session_id, e)

    except HTTPException:
        raise
    except Exception as e:
        logging.getLogger(__name__).exception("Unexpected exception in list_medias: %s", e)
        raise HTTPException(status_code=500, detail="failed to fetch medias")

    # Normalize medias_raw into MediaItem list
    items = []
    for m in (medias_raw or []):
        try:
            mid = m.get("id") if isinstance(m, dict) else str(m)
            caption = m.get("caption") if isinstance(m, dict) else None
            thumb = m.get("thumbnail") or m.get("thumbnail_url") or m.get("thumb") if isinstance(m, dict) else None
            items.append(MediaItem(id=mid, caption=caption, thumbnail_url=thumb))
        except Exception:
            continue

    logging.getLogger(__name__).debug("Returning %s media items for session %s", len(items), session_id)

    next_cursor = _build_cursor_from_provider(next_provider_cursor, sess.account_id) if next_provider_cursor else None
    meta = PaginationMeta(next_cursor=next_cursor, count_returned=len(items))
    return {"items": items, "meta": meta}

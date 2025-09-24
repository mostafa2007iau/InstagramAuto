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

    medias_items = []
    next_provider_cursor = None

    try:
        user_medias_fn = getattr(client, "get_user_medias_page", None)
        if user_medias_fn:
            # support sync or async
            if asyncio.iscoroutinefunction(user_medias_fn):
                medias_raw, next_provider_cursor = await user_medias_fn(f"user_{sess.account_id}", limit, provider_cursor)
            else:
                medias_raw, next_provider_cursor = user_medias_fn(f"user_{sess.account_id}", limit, provider_cursor)
        else:
            # fallback to user_medias
            um = getattr(client, "user_medias", None)
            if not um:
                raise HTTPException(status_code=500, detail="client does not support media retrieval")
            if asyncio.iscoroutinefunction(um):
                medias_raw = await um(f"user_{sess.account_id}", limit)
            else:
                medias_raw = um(f"user_{sess.account_id}", limit)
    except Exception:
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

    next_cursor = _build_cursor_from_provider(next_provider_cursor, sess.account_id) if next_provider_cursor else None
    meta = PaginationMeta(next_cursor=next_cursor, count_returned=len(items))
    return {"items": items, "meta": meta}

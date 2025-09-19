"""
Persian:
    Router async برای دریافت مدیاها با پشتیبانی cursor-based واقعی.
    - تلاش می‌کند از pagination provider (مثلاً instagrapi next_max_id) استفاده کند.
    - cursor توکنی است امن که payload آن شامل provider_cursor و account_id است و با CURSOR_SECRET امضا می‌شود.
    - اگر client پیاده‌سازی provider cursor نداشته باشد، fallback به رفتار قبلی انجام می‌شود.

English:
    Async router to fetch medias with real cursor-based pagination.
    - Tries to use provider pagination (e.g., instagrapi next_max_id).
    - Cursor is a secure token containing provider_cursor and account_id signed with CURSOR_SECRET.
    - Falls back to previous behavior if client doesn't provide provider cursor.
"""

from fastapi import APIRouter, Depends, HTTPException, Query, Request
from typing import Optional, List, Dict, Any
from app.deps import get_session_manager, get_locale, get_client_log_verbosity
from app.services.session_manager import SessionManager
from app.schemas.media_schema import MediaItem
from app.schemas.pagination_schema import PaginatedResponse, PaginationMeta
from app.utils.secure_cursor import sign, verify
from app.i18n import translate
import json

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
async def list_medias(request: Request, session_id: str = Query(...), limit: int = Query(20, le=100), cursor: Optional[str] = Query(None), session_manager: SessionManager = Depends(get_session_manager), locale: str = Depends(get_locale), verbosity = Depends(get_client_log_verbosity)):
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

    # Preferred: call a client method that supports pagination and returns (items, next_provider_cursor)
    # Many clients (instagrapi) offer user_medias with max_id/next_max_id or an iterator.
    medias_items = []
    next_provider_cursor = None

    # Try patterns for instagrapi-like clients
    try:
        # pattern A: client.user_medias returns (items, next_max_id) or provides last_response with next_max_id
        user_medias_fn = getattr(client, "user_medias", None)
        if user_medias_fn:
            # call with provider cursor if available (many libs accept max_id or cursor param)
            # We'll try common parameter names 'max_id' or 'cursor' by kwargs if function signature supports it.
            try:
                # try keyword param 'max_id'
                if provider_cursor is not None:
                    raw = user_medias_fn(f"user_{sess.account_id}", limit, provider_cursor)
                else:
                    raw = user_medias_fn(f"user_{sess.account_id}", limit)
            except TypeError:
                # fallback: call with only (uid, amount)
                raw = user_medias_fn(f"user_{sess.account_id}", limit)
            # raw may be list of medias or a structure including 'items' and 'next_max_id'
            if isinstance(raw, dict):
                # expected shape: {"items":[...], "next_max_id": "..."}
                medias_raw = raw.get("items", [])
                next_provider_cursor = raw.get("next_max_id")
            elif isinstance(raw, list):
                medias_raw = raw
                # try to read last_response or attribute for next cursor
                next_provider_cursor = getattr(client.client, "last_response", {}).get("next_max_id") if hasattr(client, "client") else None
            else:
                medias_raw = list(raw)
        else:
            raise AttributeError("client has no user_medias")
    except Exception:
        # Fallback: try a generic call and treat as simple list
        try:
            raw = client.user_medias(f"user_{sess.account_id}", limit)
            medias_raw = raw if isinstance(raw, list) else []
        except Exception:
            raise HTTPException(status_code=500, detail="failed to fetch medias")

    # Normalize medias_raw into MediaItem list
    items = []
    for m in medias_raw:
        try:
            mid = m.get("id") if isinstance(m, dict) else str(m)
            caption = m.get("caption") if isinstance(m, dict) else None
            thumb = m.get("thumbnail") or m.get("thumbnail_url") or m.get("thumb") if isinstance(m, dict) else None
            items.append(MediaItem(id=mid, caption=caption, thumbnail_url=thumb))
        except Exception:
            continue

    # Build next_cursor token if provider supplied one
    next_cursor = _build_cursor_from_provider(next_provider_cursor, sess.account_id) if next_provider_cursor else None
    meta = PaginationMeta(next_cursor=next_cursor, count_returned=len(items))
    return {"items": items, "meta": meta}

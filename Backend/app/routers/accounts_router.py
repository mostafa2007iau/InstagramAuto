"""
Persian:
    روتری برای مدیریت حساب‌ها و سشن‌ها (ایجاد سشن، لاگین+پروب، export/import، toggle پروکسی، حذف).
    این نسخه async است و با SessionManager async کار می‌کند. تمامی endpointها توضیحات دو‌زبانه در summary/description دارند.

English:
    Router for accounts and sessions management (create session, login+probe, export/import, proxy toggle, delete).
    This async version targets the async SessionManager. Endpoints include bilingual summaries/descriptions.
"""

from fastapi import APIRouter, Depends, HTTPException, Request
from typing import Dict
from app.schemas.session_schema import SessionCreateIn, SessionOut
from app.deps import get_session_manager, get_locale, get_client_log_verbosity
from app.services.session_manager import SessionManager
from app.services.proxy_service import test_proxy
from app.i18n import translate
from app.logging_utils import format_log

router = APIRouter(prefix="/api/accounts", tags=["accounts"])

# Create session
@router.post("/sessions", response_model=SessionOut, summary="Create session / ایجاد سشن", description="Create a session metadata record / ساخت رکورد متادیتای سشن")
async def create_session(payload: SessionCreateIn, session_manager: SessionManager = Depends(get_session_manager)):
    meta = await session_manager.create_session(
        account_id=payload.account_id,
        proxy=payload.proxy,
        proxy_enabled=payload.proxy_enabled,
        locale=payload.locale
    )
    return meta

# Login and mandatory media probe
@router.post("/sessions/{session_id}/login", summary="Login and probe / ورود و پروب", description="Perform login and mandatory media probe / انجام لاگین و پروب اجباری مدیا")
async def login_and_probe(session_id: str, request: Request, username: str, password: str, session_manager: SessionManager = Depends(get_session_manager), locale: str = Depends(get_locale), verbosity = Depends(get_client_log_verbosity)):
    meta = await session_manager.get_session(session_id)
    if not meta:
        raise HTTPException(status_code=404, detail="session not found")
    result = await session_manager.login_and_probe(session_id, username, password)
    log_obj = format_log(str(result), verbosity, locale)
    return {"result": result, "log": log_obj}

# Test proxy (does not persist to session)
@router.post("/sessions/{session_id}/proxy/test", summary="Test proxy / تست پروکسی", description="Test provided proxy string / تست رشته پروکسی ارائه‌شده")
async def proxy_test(session_id: str, request: Request, proxy: str, session_manager: SessionManager = Depends(get_session_manager), locale: str = Depends(get_locale)):
    ok, message = test_proxy(proxy, locale=locale)
    if not ok:
        raise HTTPException(status_code=400, detail=message)
    return {"ok": True, "message": message}

# Toggle proxy_enabled and apply immediately
@router.post("/sessions/{session_id}/proxy/toggle", summary="Toggle proxy / تغییر وضعیت پروکسی", description="Enable or disable proxy for session / فعال یا غیرفعال‌سازی پروکسی برای سشن")
async def proxy_toggle(session_id: str, enabled: bool, session_manager: SessionManager = Depends(get_session_manager)):
    meta = await session_manager.get_session(session_id)
    if not meta:
        raise HTTPException(status_code=404, detail="session not found")
    await session_manager.update_session(session_id, proxy_enabled=enabled)
    # apply immediately
    await session_manager.apply_proxy_if_enabled(session_id)
    return {"ok": True, "proxy_enabled": enabled}

# Export session metadata (including encrypted session_blob)
@router.get("/sessions/{session_id}/export", summary="Export session / خروجی سشن", description="Export session metadata as JSON / خروجی متادیتای سشن به صورت JSON")
async def export_session(session_id: str, session_manager: SessionManager = Depends(get_session_manager)):
    meta = await session_manager.get_session(session_id)
    if not meta:
        raise HTTPException(status_code=404, detail="session not found")
    try:
        payload = await session_manager.export_session(session_id)
        return {"ok": True, "payload": payload}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

# Import session metadata JSON into DB
@router.post("/sessions/import", summary="Import session / وارد کردن سشن", description="Import session metadata JSON into DB / وارد کردن متادیتای سشن از JSON")
async def import_session(payload: Dict, session_manager: SessionManager = Depends(get_session_manager)):
    try:
        s = await session_manager.import_session(payload)
        return {"ok": True, "session_id": s.id}
    except Exception as e:
        raise HTTPException(status_code=400, detail=str(e))

# Delete session
@router.delete("/sessions/{session_id}", summary="Delete session / حذف سشن", description="Delete session and runtime client / حذف سشن و کلاینت زمان اجرا")
async def delete_session(session_id: str, session_manager: SessionManager = Depends(get_session_manager)):
    meta = await session_manager.get_session(session_id)
    if not meta:
        raise HTTPException(status_code=404, detail="session not found")
    await session_manager.delete_session(session_id)
    return {"ok": True}

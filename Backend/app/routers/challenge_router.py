"""
Persian:
    Router async برای مدیریت چالش/2FA با استفاده از Redis-backed challenge store.
    حالا endpoint حل چالش و ایجاد چالش با rate-limit محافظت شده‌اند.

English:
    Async router to manage challenge/2FA using Redis-backed challenge store.
    Challenge creation and resolve endpoints are protected with rate-limits.
"""

from fastapi import APIRouter, HTTPException, Body, Depends
from typing import Dict
from app.services import challenge_store_redis as store
from app.middleware.verbosity_middleware import default_rate_limit, strict_rate_limit
from app.deps import get_locale

router = APIRouter(prefix="/api/challenge", tags=["challenge"])

@router.get("/{token}", summary="Get challenge / دریافت چالش", description="Get challenge state by token / دریافت وضعیت چالش بر اساس توکن")
async def get_challenge_state(token: str):
    st = await store.get_challenge(token)
    if not st:
        raise HTTPException(status_code=404, detail="challenge not found or expired")
    return {"ok": True, "state": st}

@router.post("/{token}/resolve", summary="Resolve challenge / حل چالش", description="Resolve challenge by submitting response (e.g., code) / حل چالش با ارسال پاسخ (مثلا کد)")
async def post_resolve_challenge(token: str, payload: Dict = Body(...), _rl=Depends(strict_rate_limit), locale: str = Depends(get_locale)):
    try:
        res = await store.resolve_challenge(token, payload)
    except Exception as e:
        raise HTTPException(status_code=400, detail=str(e))
    return {"ok": True, "result": res}

# Convenience endpoint to create a challenge (protected; mainly for testing)
@router.post("/create", summary="Create a challenge / ایجاد چالش", description="Create a transient challenge token (mainly for testing) / ایجاد توکن چالش موقتی", )
async def create_challenge_endpoint(payload: Dict = Body(...), _rl=Depends(default_rate_limit)):
    session_id = payload.get("session_id")
    ch_type = payload.get("type", "challenge")
    data = payload.get("payload", {})
    token = await store.create_challenge(session_id, ch_type, data)
    return {"ok": True, "token": token}

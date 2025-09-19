"""
Persian:
    Router async برای مدیریت چالش/2FA با استفاده از Redis-backed challenge store.
    این فایل به صورت async نوشته شده و از توابع async store مستقیماً await می‌کند.

English:
    Async router to manage challenge/2FA using Redis-backed challenge store.
    This file is written async and directly awaits async store functions.
"""

from fastapi import APIRouter, HTTPException, Body, Depends
from typing import Dict
from app.services import challenge_store_redis as store

router = APIRouter(prefix="/api/challenge", tags=["challenge"])

@router.get("/{token}", summary="Get challenge / دریافت چالش", description="Get challenge state by token / دریافت وضعیت چالش بر اساس توکن")
async def get_challenge_state(token: str):
    st = await store.get_challenge(token)
    if not st:
        raise HTTPException(status_code=404, detail="challenge not found or expired")
    return {"ok": True, "state": st}

@router.post("/{token}/resolve", summary="Resolve challenge / حل چالش", description="Resolve challenge by submitting response (e.g., code) / حل چالش با ارسال پاسخ (مثلا کد)")
async def post_resolve_challenge(token: str, payload: Dict = Body(...)):
    try:
        res = await store.resolve_challenge(token, payload)
    except Exception as e:
        raise HTTPException(status_code=400, detail=str(e))
    return {"ok": True, "result": res}

# Convenience endpoint to create a challenge (mainly for testing / کمک در تست)
@router.post("/create", summary="Create a challenge / ایجاد چالش", description="Create a transient challenge token (mainly for testing) / ایجاد توکن چالش موقتی")
async def create_challenge_endpoint(payload: Dict = Body(...)):
    session_id = payload.get("session_id")
    ch_type = payload.get("type", "challenge")
    data = payload.get("payload", {})
    token = await store.create_challenge(session_id, ch_type, data)
    return {"ok": True, "token": token}

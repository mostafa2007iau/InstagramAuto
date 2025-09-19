"""
Persian:
    مسیر برای دریافت eventهای inbound (مثلاً webhook یا polling result) که قوانین را اجرا می‌کند.

English:
    Router to accept inbound events (e.g., webhook or polling) and run rules engine.
"""

from fastapi import APIRouter, Body
from app.services.rules_engine import run_rules_for_account

router = APIRouter(prefix="/api/inbound", tags=["inbound"])

@router.post("/event", summary="Inbound event", description="Receive an event (new media, comment) and evaluate rules")
async def inbound_event(payload: dict = Body(...)):
    # payload must contain account_id and session_id to allow rule evaluation and enqueue
    account_id = payload.get("account_id")
    if not account_id:
        return {"ok": False, "error": "missing_account_id"}
    run_rules_for_account(account_id, payload)
    return {"ok": True}

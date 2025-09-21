"""
Persian:
    Router برای دریافت رویدادهای ورودی (webhook/poll) و اجرای rules engine.
    این endpoint به دلیل احتمال فراخوانی زیاد با rate-limit محافظت شده است.

English:
    Router to receive inbound events (webhook/poll) and run rules engine.
    This endpoint is rate-limited to reduce abuse.
"""

from fastapi import APIRouter, Body, Depends, HTTPException
from app.services.rules_engine import evaluate_rules_for_event
from app.middleware.rate_limit_dependency import default_rate_limit
from typing import Dict

router = APIRouter(prefix="/api/inbound", tags=["inbound"])

@router.post("/event", summary="Inbound event / رویداد ورودی", description="Receive inbound events and run rules engine / دریافت رویدادهای ورودی و اجرای قواعد")
async def inbound_event(payload: Dict = Body(...), _rl=Depends(default_rate_limit)):
    # validate minimal shape
    account_id = payload.get("account_id")
    session_id = payload.get("session_id")
    if not account_id or not session_id:
        raise HTTPException(status_code=400, detail="account_id and session_id are required")
    # run rules engine (async or sync)
    try:
        res = evaluate_rules_for_event(payload)
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    return {"ok": True, "matched": res}

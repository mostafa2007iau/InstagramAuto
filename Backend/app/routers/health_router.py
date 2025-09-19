"""
Persian:
    router ساده health که پیام محلی‌شده بازمی‌گرداند.

English:
    Simple health router returning localized message.
"""

from fastapi import APIRouter, Request
from app.i18n import translate
from app.services.telemetry_service import get_metrics

router = APIRouter(prefix="/api/health", tags=["health"])

@router.get("", summary="Health check", description="Return service health and basic metrics")
async def health(request: Request):
    locale = getattr(request.state, "locale", "en")
    msg = translate("health.ok", locale=locale)
    metrics = get_metrics()
    return {"status": "ok", "message": msg, "metrics": metrics}


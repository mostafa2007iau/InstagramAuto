from fastapi import APIRouter, Depends
from app.middleware.verbosity_middleware import default_rate_limit
from datetime import datetime

router = APIRouter(prefix="/api/settings", tags=["settings"])

@router.get("/{account_id}")
async def get_settings(account_id: str, _rl=Depends(default_rate_limit)):
    # Return default settings shaped to match client SettingsDto
    now = datetime.utcnow().isoformat() + "Z"
    return {
        "account_id": account_id,
        "enable_delay": False,
        "comment_delay_sec": 5,
        "like_delay_sec": 2,
        "dm_delay_sec": 5,
        "hourly_limit": 100,
        "daily_limit": 1000,
        "random_jitter_enabled": False,
        "jitter_min_sec": 0,
        "jitter_max_sec": 0,
        "created_at": now,
        "updated_at": now
    }

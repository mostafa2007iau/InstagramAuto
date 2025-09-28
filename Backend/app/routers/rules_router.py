"""
Persian:
    Router برای مدیریت و فهرست قاعده‌ها (rules) برای هر اکانت.
English:
    Router to list and manage rules for an account.
"""

from fastapi import APIRouter, Query, Depends
from typing import Optional
from app.middleware.verbosity_middleware import default_rate_limit

router = APIRouter(prefix="/api/rules", tags=["rules"])

@router.get("")
async def list_rules(account_id: Optional[str] = Query(None), limit: int = Query(50), cursor: Optional[str] = Query(None), _rl=Depends(default_rate_limit)):
    """Return an empty paginated rules list (stub)"""
    return {"items": [], "meta": {"next_cursor": None, "count_returned": 0}}

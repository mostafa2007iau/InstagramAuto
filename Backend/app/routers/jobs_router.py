from fastapi import APIRouter, Query, Depends
from typing import Optional
from app.middleware.verbosity_middleware import default_rate_limit

router = APIRouter(prefix="/api/jobs", tags=["jobs"])

@router.get("")
async def list_jobs(cursor: Optional[str] = Query(None), limit: int = Query(20), status: Optional[str] = Query(None), type: Optional[str] = Query(None), _rl=Depends(default_rate_limit)):
    """Return an empty paginated jobs list (stub)"""
    return {"items": [], "meta": {"next_cursor": None, "count_returned": 0}}

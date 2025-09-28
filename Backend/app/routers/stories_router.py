from fastapi import APIRouter, Query, Depends
from typing import Optional
from app.middleware.verbosity_middleware import default_rate_limit

router = APIRouter(prefix="/api/stories", tags=["stories"])

@router.get("")
async def list_stories(session_id: str = Query(...), limit: int = Query(20), cursor: Optional[str] = Query(None), _rl=Depends(default_rate_limit)):
    # Return empty stories page (stub)
    return {"items": [], "meta": {"next_cursor": None, "count_returned": 0}}

@router.post("/{story_id}/reply")
async def reply_to_story(story_id: str, session_id: str, _rl=Depends(default_rate_limit)):
    return {"ok": True}

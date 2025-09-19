"""
Persian:
    Router async برای خواندن لاگ‌ها با فیلتر سطح و صفحه‌بندی. پاسخ‌ها PaginatedResponse[LogOut] هستند.

English:
    Async router to read logs with level/account filters and pagination. Responses use PaginatedResponse[LogOut].
"""

from fastapi import APIRouter, Depends, Query
from sqlmodel import Session, select
from typing import Optional
from app.db import get_session
from app.models.log_model import EventLog
from app.schemas.log_schema import LogOut
from app.schemas.pagination_schema import PaginatedResponse, PaginationMeta
import base64, json

router = APIRouter(prefix="/api/logs", tags=["logs"])

def _encode_cursor(state: dict) -> str:
    return base64.urlsafe_b64encode(json.dumps(state).encode()).decode()

def _decode_cursor(token: Optional[str]) -> dict:
    if not token:
        return {"offset": 0}
    try:
        raw = base64.urlsafe_b64decode(token.encode()).decode()
        return json.loads(raw)
    except Exception:
        return {"offset": 0}

@router.get("", response_model=PaginatedResponse[LogOut], summary="List logs / فهرست لاگ‌ها", description="List logs with filters and pagination / فهرست لاگ‌ها با فیلتر و صفحه‌بندی")
async def list_logs(level: Optional[str] = Query(None), account_id: Optional[str] = Query(None), limit: int = Query(50, le=200), cursor: Optional[str] = Query(None), db: Session = Depends(get_session)):
    decoded = _decode_cursor(cursor)
    offset = decoded.get("offset", 0)
    statement = select(EventLog).order_by(EventLog.created_at.desc())
    if level:
        statement = statement.where(EventLog.level == level)
    if account_id:
        statement = statement.where(EventLog.account_id == account_id)
    rows = db.exec(statement.limit(limit).offset(offset)).all()
    items = [LogOut.from_orm(r) for r in rows]
    next_offset = offset + len(items)
    next_cursor = _encode_cursor({"offset": next_offset}) if len(items) == limit else None
    meta = PaginationMeta(next_cursor=next_cursor, count_returned=len(items))
    return {"items": items, "meta": meta}

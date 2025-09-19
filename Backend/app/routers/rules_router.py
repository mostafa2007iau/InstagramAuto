"""
Persian:
    Router مدیریت قواعد با اعتبارسنجی jsonlogic هنگام ایجاد/به‌روزرسانی.
    هنگام ایجاد، اگر expression اعتبار نداشته باشد خطای محلی‌شده برگردانده می‌شود.

English:
    Rules router with jsonlogic validation on create/update.
    On create/update, expression is validated; localized error returned if invalid.
"""

from fastapi import APIRouter, Depends, HTTPException, Query, Body
from typing import Optional
from app.db import get_session
from sqlmodel import Session, select
from app.models.rule_model import Rule
from app.schemas.rule_schema import RuleIn, RuleOut
from app.schemas.pagination_schema import PaginatedResponse, PaginationMeta
from app.deps import get_locale
from jsonlogic import jsonlogic
import json, base64

router = APIRouter(prefix="/api/rules", tags=["rules"])

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

def validate_jsonlogic(expr_str: str) -> bool:
    try:
        obj = json.loads(expr_str)
        # attempt a dry-run evaluation on empty context; jsonlogic will error if invalid structure
        jsonlogic(obj, data={})
        # optionally check operator whitelist here
        return True
    except Exception:
        return False

@router.post("", response_model=RuleOut, summary="Create rule / ایجاد قاعده", description="Create a new rule with jsonlogic expression / ایجاد قاعده جدید با عبارت jsonlogic")
async def create_rule(payload: RuleIn, db: Session = Depends(get_session), locale: str = Depends(get_locale)):
    if not validate_jsonlogic(payload.expression):
        # localized error
        raise HTTPException(status_code=400, detail="Invalid rule expression / عبارت قاعده نامعتبر است")
    rule = Rule(**payload.dict())
    db.add(rule)
    db.commit()
    db.refresh(rule)
    return rule

@router.get("", response_model=PaginatedResponse[RuleOut], summary="List rules / فهرست قواعد", description="List rules with pagination / فهرست قواعد با صفحه‌بندی")
async def list_rules(account_id: Optional[str] = Query(None), limit: int = Query(20, le=100), cursor: Optional[str] = Query(None), db: Session = Depends(get_session)):
    decoded = _decode_cursor(cursor)
    offset = decoded.get("offset", 0)
    statement = select(Rule).order_by(Rule.created_at.desc())
    if account_id:
        statement = statement.where(Rule.account_id == account_id)
    rows = db.exec(statement.limit(limit).offset(offset)).all()
    items = [RuleOut.from_orm(r) for r in rows]
    next_offset = offset + len(items)
    next_cursor = _encode_cursor({"offset": next_offset}) if len(items) == limit else None
    meta = PaginationMeta(next_cursor=next_cursor, count_returned=len(items))
    return {"items": items, "meta": meta}

@router.put("/{rule_id}", response_model=RuleOut, summary="Update rule / به‌روزرسانی قاعده", description="Update a rule with validation / به‌روزرسانی قاعده با اعتبارسنجی")
async def update_rule(rule_id: int, payload: RuleIn, db: Session = Depends(get_session), locale: str = Depends(get_locale)):
    rule = db.get(Rule, rule_id)
    if not rule:
        raise HTTPException(status_code=404, detail="rule not found")
    if not validate_jsonlogic(payload.expression):
        raise HTTPException(status_code=400, detail="Invalid rule expression / عبارت قاعده نامعتبر است")
    for k, v in payload.dict().items():
        setattr(rule, k, v)
    db.add(rule)
    db.commit()
    db.refresh(rule)
    return rule

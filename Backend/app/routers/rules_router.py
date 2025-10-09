"""
Persian:
    Router قوانین با پشتیبانی از عملیات CRUD.
    اضافه شدن فعال/غیرفعال‌سازی و حذف قوانین.

English:
    Rules router with CRUD operations support.
    Added rule enable/disable and delete operations.
"""

from fastapi import APIRouter, Depends, HTTPException, Query
from typing import Optional, List
from app.deps import get_session_manager, get_locale
from app.schemas.rule_schema import RuleIn, RuleOut, RuleUpdate
from app.services.session_manager import SessionManager
from app.schemas.pagination_schema import PaginatedResponse, PaginationMeta
from app.middleware.verbosity_middleware import default_rate_limit
import logging

router = APIRouter(prefix="/api/rules", tags=["rules"])

@router.post("", response_model=RuleOut)
async def create_rule(
    rule: RuleIn,
    session_manager: SessionManager = Depends(get_session_manager),
    _rl=Depends(default_rate_limit)
):
    """
    Create new rule / ایجاد قانون جدید
    """
    return await session_manager.create_rule(rule)

@router.get("", response_model=PaginatedResponse[RuleOut])
async def list_rules(
    account_id: Optional[str] = None,
    limit: int = Query(50, le=100),
    cursor: Optional[str] = None,
    session_manager: SessionManager = Depends(get_session_manager),
    _rl=Depends(default_rate_limit)
):
    """
    List rules / فهرست قوانین
    """
    rules = await session_manager.get_rules(account_id, limit, cursor)
    return {
        "items": rules,
        "meta": PaginationMeta(
            next_cursor=None,  # TODO: implement pagination
            count_returned=len(rules)
        )
    }

@router.put("/{rule_id}")
async def update_rule(
    rule_id: str,
    update: RuleUpdate,
    session_manager: SessionManager = Depends(get_session_manager),
    _rl=Depends(default_rate_limit)
):
    """
    Update rule / به‌روزرسانی قانون
    """
    return await session_manager.update_rule(rule_id, update)

@router.delete("/{rule_id}")
async def delete_rule(
    rule_id: str,
    session_manager: SessionManager = Depends(get_session_manager),
    _rl=Depends(default_rate_limit)
):
    """
    Delete rule / حذف قانون
    """
    return await session_manager.delete_rule(rule_id)

@router.get("/export")
async def export_rules(
    account_id: str,
    format: str = "json",
    session_manager: SessionManager = Depends(get_session_manager),
    _rl=Depends(default_rate_limit)
):
    """
    Export rules / خروجی گرفتن از قوانین
    """
    return await session_manager.export_rules(account_id, format)

@router.post("/import")
async def import_rules(
    account_id: str,
    data: str,
    format: str = "json",
    session_manager: SessionManager = Depends(get_session_manager),
    _rl=Depends(default_rate_limit)
):
    """
    Import rules / وارد کردن قوانین
    """
    return await session_manager.import_rules(account_id, data, format)

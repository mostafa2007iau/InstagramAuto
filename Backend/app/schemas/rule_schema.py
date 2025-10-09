"""
Persian:
    اسکیمای قوانین با پشتیبانی از فعال/غیرفعال‌سازی.

English:
    Rule schemas with enable/disable support.
"""

from pydantic import BaseModel, Field
from typing import Optional, List
from datetime import datetime

class RuleIn(BaseModel):
    """Create rule input / ورودی ایجاد قانون"""
    account_id: str = Field(..., description="Account ID / شناسه حساب")
    name: str = Field(..., description="Rule name / نام قانون")
    condition: str = Field(..., description="json-logic expression as JSON string")
    media_id: Optional[str] = Field(None, description="Media ID / شناسه پست")
    enabled: bool = Field(True, description="Rule enabled state / وضعیت فعال بودن")
    replies: List[str] = Field([], description="Reply templates / قالب‌های پاسخ")
    send_dm: bool = Field(False, description="Send DM flag / ارسال پیام مستقیم")
    dm_template: Optional[dict] = Field(None, description="DM template / قالب پیام مستقیم")
    attachments: List[dict] = Field([], description="Media attachments / پیوست‌های رسانه‌ای")

class RuleOut(RuleIn):
    """Rule output / خروجی قانون"""
    id: str = Field(..., description="Rule ID / شناسه قانون")
    created_at: datetime
    updated_at: datetime

class RuleUpdate(BaseModel):
    """Update rule input / ورودی به‌روزرسانی قانون"""
    name: Optional[str] = None
    condition: Optional[str] = None
    enabled: Optional[bool] = None
    replies: Optional[List[str]] = None
    send_dm: Optional[bool] = None
    dm_template: Optional[dict] = None
    attachments: Optional[List[dict]] = None

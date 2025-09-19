"""
Persian:
    مدل لاگ ساختاری برای نگهداری رخدادها و پیام‌های محلی‌شده.

English:
    Structured log model for storing events and localized messages.
"""

from sqlmodel import SQLModel, Field
from typing import Optional
from datetime import datetime

class EventLog(SQLModel, table=True):
    id: Optional[int] = Field(default=None, primary_key=True)
    account_id: Optional[str] = Field(None, index=True, description="Related account id / شناسه حساب مرتبط")
    level: str = Field("INFO", description="Log level / سطح لاگ")
    message_en: str = Field(..., description="English message")
    message_fa: Optional[str] = Field(None, description="Persian message")
    metadata: Optional[str] = Field(None, description="JSON metadata as string")
    created_at: datetime = Field(default_factory=datetime.utcnow)

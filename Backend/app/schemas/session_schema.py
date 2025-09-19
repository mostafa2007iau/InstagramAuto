"""
Persian:
    اسکیمای پایدنت برای SessionMetadata ورودی/خروجی.

English:
    Pydantic schemas for session metadata I/O.
"""

from typing import Optional
from pydantic import BaseModel, Field
from datetime import datetime

class SessionCreateIn(BaseModel):
    """
    Persian:
        ورودی ساخت سشن: account_id، proxy، proxy_enabled و locale.

    English:
        Input for creating a session.
    """
    account_id: str = Field(..., description="شناسه حساب / account identifier")
    proxy: Optional[str] = Field(None, description="Proxy string or None")
    proxy_enabled: bool = Field(False, description="Enable proxy for this session")
    locale: str = Field("en", description="Preferred locale for session")

class SessionOut(BaseModel):
    id: str
    account_id: str
    proxy: Optional[str]
    proxy_enabled: bool
    last_media_check: Optional[datetime]
    locale_preference: str

"""
Persian:
    اسکیمای EventLog برای برگرداندن لاگ‌ها.

English:
    EventLog schema returned by API.
"""

from pydantic import BaseModel
from typing import Optional

class LogOut(BaseModel):
    id: int
    account_id: Optional[str]
    level: str
    message_en: str
    message_fa: Optional[str]
    metadata: Optional[str]
    created_at: str

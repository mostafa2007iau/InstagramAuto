"""
Persian:
    مدل دیتابیس برای metadata سشن با فیلد اضافی session_blob برای نگهداری session bytes رمزنگاری‌شده.

English:
    DB model for session metadata including session_blob field to store encrypted session bytes.
"""

from sqlmodel import SQLModel, Field
from typing import Optional
from datetime import datetime

class SessionDB(SQLModel, table=True):
    id: str = Field(primary_key=True, description="Session UUID")
    account_id: str = Field(index=True, description="Linked account id")
    proxy: Optional[str] = Field(None, description="Proxy string or null")
    proxy_enabled: bool = Field(False, description="Is proxy enabled")
    last_media_check: Optional[datetime] = Field(None, description="Last probe time")
    locale_preference: str = Field("en", description="Locale preference")
    session_blob: Optional[str] = Field(None, description="Encrypted session bytes (base64) / بایت‌های نشست رمزنگاری‌شده")
    created_at: datetime = Field(default_factory=datetime.utcnow)
    updated_at: datetime = Field(default_factory=datetime.utcnow)

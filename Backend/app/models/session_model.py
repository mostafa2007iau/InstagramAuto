"""
Persian:
    مدل داده‌ای ساده برای metadata سشن که در persistence نگهداری می‌شود.

English:
    Simple session metadata model stored in persistence layer.
"""

from typing import Optional
from pydantic import BaseModel, Field
from datetime import datetime

class SessionMetadata(BaseModel):
    """
    Persian:
        متادیتای سشن شامل اطلاعات پروکسی، آخرین بررسی مدیا و تنظیمات locale.

    English:
        Session metadata including proxy settings, last media check and locale preference.
    """
    id: str = Field(..., description="Unique session id / شناسه سشن یکتا")
    account_id: str = Field(..., description="Linked account identifier / شناسه حساب متصل")
    proxy: Optional[str] = Field(None, description="Proxy string or None / رشته پروکسی یا None")
    proxy_enabled: bool = Field(False, description="Is proxy enabled for this session / آیا پروکسی فعال است؟")
    last_media_check: Optional[datetime] = Field(None, description="Last time probe ran / زمان آخرین پروب")
    locale_preference: str = Field("en", description="Preferred locale for this session / زبان ترجیحی این سشن")

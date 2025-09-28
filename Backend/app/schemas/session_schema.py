# app/schemas/session_schema.py

"""
Persian:
    اسکیمای پایدنت برای SessionMetadata ورودی/خروجی و پاسخ لاگین (شامل چالش).
English:
    Pydantic schemas for session metadata I/O and login response (with challenge support).
"""

from typing import Optional, Dict, Any
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
    """
    Persian:
        خروجی متادیتای سشن.
    English:
        Output metadata for a session.
    """
    id: str = Field(..., description="Session identifier")
    account_id: str = Field(..., description="Account identifier")
    proxy: Optional[str] = Field(None, description="Proxy string if set")
    proxy_enabled: bool = Field(..., description="Whether proxy is enabled")
    last_media_check: Optional[datetime] = Field(
        None, description="Timestamp of last media probe"
    )
    locale_preference: str = Field(..., description="Locale preference of session")


class LoginResultOut(BaseModel):
    """
    Persian:
        پاسخ لاگین شامل وضعیت، لاگ داخلی، و در صورت نیاز چالش (توکن و payload).
    English:
        Login response including status, internal log, and if required, challenge token & payload.
    """
    ok: bool = Field(..., description="Whether Instagram login succeeded")
    status: str = Field(..., description="Login status: 'ok' or 'challenge_required'")
    session_blob: Optional[str] = Field(
        None, description="Session blob when login succeeds"
    )
    log: Optional[Any] = Field(
        None, description="Optional debug/log object from client"
    )
    challenge_token: Optional[str] = Field(
        None, description="Token to fetch/resolve the challenge"
    )
    challenge_payload: Optional[Dict[str, Any]] = Field(
        None, description="Masked info (e.g. phone/email) for the challenge"
    )

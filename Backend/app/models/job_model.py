"""
Persian:
    مدل Job برای صف ارسال: ذخیره job_id، نوع، وضعیت، payload و timestampها برای پیگیری، idempotency token.

English:
    Job model to track queued jobs: job_id, type, status, payload, timestamps and idempotency token.
"""

from sqlmodel import SQLModel, Field, JSON
from typing import Optional
from datetime import datetime

class Job(SQLModel, table=True):
    id: Optional[int] = Field(default=None, primary_key=True)
    job_id: Optional[str] = Field(None, index=True, description="RQ/Celery job id or uuid")
    idempotency_key: Optional[str] = Field(None, index=True, description="Client-provided idempotency key")
    job_type: str = Field(..., description="Type of job e.g., send_message")
    payload: Optional[str] = Field(None, description="JSON payload as string")
    status: str = Field("queued", description="queued|processing|done|failed")
    attempts: int = Field(0)
    last_error: Optional[str] = Field(None)
    created_at: datetime = Field(default_factory=datetime.utcnow)
    updated_at: datetime = Field(default_factory=datetime.utcnow)

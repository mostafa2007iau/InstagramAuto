"""
Persian:
    ??????? ????? ?? ????? ? ???????.

English:
    Models for jobs and settings.
"""

from datetime import datetime
from enum import Enum
from typing import Optional, Dict, Any
from pydantic import BaseModel

class JobStatus(str, Enum):
    PENDING = "pending"
    IN_PROGRESS = "in_progress"
    COMPLETED = "completed"
    FAILED = "failed"
    CANCELLED = "cancelled"

class JobItem(BaseModel):
    """Job item model"""
    id: Optional[str] = None
    type: str
    status: JobStatus
    rule_id: str
    media_id: str
    target_user_id: str
    comment_id: Optional[str] = None
    payload: Dict[str, Any]
    created_at: datetime = datetime.utcnow()
    updated_at: datetime = datetime.utcnow()
    completed_at: Optional[datetime] = None
    error: Optional[str] = None
    attempts: int = 0
    account_id: Optional[str] = None
    session_blob: Optional[str] = None

class AccountSettings(BaseModel):
    """Account settings model"""
    account_id: str
    enable_delay: bool = True
    reply_delay: int = 30  # seconds
    dm_delay: int = 60  # seconds
    like_delay: int = 15  # seconds
    hourly_limit: int = 20
    daily_limit: int = 100
    random_jitter: bool = True
    jitter_min: int = 1  # seconds
    jitter_max: int = 5  # seconds
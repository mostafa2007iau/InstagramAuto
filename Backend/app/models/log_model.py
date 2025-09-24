from typing import Optional, Dict, Any
from datetime import datetime
from sqlmodel import SQLModel, Field
from sqlalchemy import Column, JSON

class EventLog(SQLModel, table=True):
    id: Optional[int] = Field(default=None, primary_key=True)
    account_id: Optional[str] = Field(
        default=None,
        index=True,
        description="Related account id / شناسه حساب مرتبط"
    )
    level: str = Field(
        default="INFO",
        description="Log level / سطح لاگ"
    )
    message_en: str = Field(
        ...,
        description="English message"
    )
    message_fa: Optional[str] = Field(
        default=None,
        description="Persian message"
    )
    metadata_json: Optional[Dict[str, Any]] = Field(
        default=None,
        sa_column=Column("metadata", JSON),
        description="JSON metadata"
    )
    created_at: datetime = Field(
        default_factory=datetime.utcnow,
        description="Creation timestamp"
    )

"""
Persian:
    مدل Rule که قوانین ارسال/اعمال را ذخیره می‌کند.

English:
    Rule model storing sending/apply rules.
"""

from sqlmodel import SQLModel, Field
from typing import Optional
from datetime import datetime

class Rule(SQLModel, table=True):
    id: Optional[int] = Field(default=None, primary_key=True)
    account_id: str = Field(index=True, description="Linked account id / شناسه حساب مرتبط")
    name: str = Field(..., description="Rule name / نام قاعده")
    expression: str = Field(..., description="Rule expression / عبارت شرطی")
    enabled: bool = Field(default=True, description="Is rule active / آیا فعال است")
    created_at: datetime = Field(default_factory=datetime.utcnow)
    updated_at: datetime = Field(default_factory=datetime.utcnow)

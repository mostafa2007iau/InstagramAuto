"""
Persian:
    اسکیمای Rule برای ورودی/خروجی API.

English:
    Rule schema for API I/O.
"""

from pydantic import BaseModel, Field
from typing import Optional

class RuleIn(BaseModel):
    account_id: str = Field(..., description="Account id")
    name: str = Field(..., description="Rule name")
    expression: str = Field(..., description="Expression")
    enabled: bool = True

class RuleOut(RuleIn):
    id: int
    created_at: str
    updated_at: str

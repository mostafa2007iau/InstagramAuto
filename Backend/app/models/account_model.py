"""
Persian:
    مدل Account برای نگهداری اطلاعات پایه حساب.

English:
    Account model for storing basic account information.
"""

from sqlmodel import SQLModel, Field
from typing import Optional

class Account(SQLModel, table=True):
    id: Optional[int] = Field(default=None, primary_key=True)
    username: str = Field(index=True, description="Account username / نام کاربری")
    display_name: Optional[str] = Field(None, description="Human readable name / نام نمایشی")
    created_at: Optional[str] = Field(None, description="Creation time ISO / زمان ایجاد به فرمت ISO")

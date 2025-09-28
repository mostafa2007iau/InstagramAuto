"""
Persian:
    ???? ????? ???? API.
English:
    Comment schema for API.

Install:
    pip install pydantic
"""
from pydantic import BaseModel
from datetime import datetime

class CommentSchema(BaseModel):
    id: str
    media_id: str
    user_id: str
    text: str
    created_at: datetime
    processed: bool

    class Config:
        orm_mode = True

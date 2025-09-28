"""
Persian:
    ??? ????? ???? ????? ? ?????? ????????.
English:
    Comment model for storing and processing comments.

Install:
    pip install sqlalchemy
"""
from sqlalchemy import Column, Integer, String, Boolean, DateTime
from app.db import Base

class Comment(Base):
    __tablename__ = "comments"
    id = Column(String, primary_key=True)
    media_id = Column(String)
    user_id = Column(String)
    text = Column(String)
    created_at = Column(DateTime)
    processed = Column(Boolean, default=False)

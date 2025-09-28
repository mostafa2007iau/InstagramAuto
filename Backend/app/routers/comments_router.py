"""
Persian:
    ???? ????? ???? ?????? ? ?????? ????????.
English:
    Comment router for managing and viewing comments.

Install:
    pip install fastapi
"""
from fastapi import APIRouter, Depends, Query
from sqlalchemy.orm import Session as DBSession
from app.db import get_db
from app.models.comment_model import Comment
from app.schemas.comment_schema import CommentSchema
from typing import List

router = APIRouter(prefix="/api/comments", tags=["comments"])

@router.get("", response_model=List[CommentSchema], summary="List comments / ????? ????????")
def list_comments(media_id: str = Query(...), db: DBSession = Depends(get_db)):
    return db.query(Comment).filter(Comment.media_id == media_id).all()

"""
Persian:
    ???? ?????? ? ?????? ???? ????? ???? ? ?????? ????.
English:
    Router for manual reply and DM sending.

Install:
    pip install fastapi
"""
from fastapi import APIRouter, Depends, HTTPException
from app.services.reply_service import ReplyService
from app.services.dm_service import DMService

router = APIRouter(prefix="/api/reply", tags=["reply"])

@router.post("/comment", summary="Send reply to comment / ????? ???? ?? ?????")
async def send_reply_to_comment(media_id: str, comment_id: str, reply_text: str):
    # TODO: Implement using ReplyService
    return {"ok": True}

@router.post("/dm", summary="Send DM to user / ????? ?????? ?? ?????")
async def send_dm_to_user(user_id: str, dm_text: str):
    # TODO: Implement using DMService
    return {"ok": True}

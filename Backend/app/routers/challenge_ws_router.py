"""
Persian:
    WebSocket endpoint برای جریان چالش با استفاده از Redis pub/sub.
    پیام‌های لاگ هنگام اتصال/قطع اضافه شده‌اند.

English:
    WebSocket endpoint to stream challenge events using Redis pub/sub.
    Connection/disconnection events are logged.
"""

from fastapi import APIRouter, WebSocket, WebSocketDisconnect
from app.services.challenge_store_redis import subscribe_challenge_events
import asyncio
import logging

logger = logging.getLogger("challenge-ws")

router = APIRouter(prefix="/ws/challenge", tags=["challenge-ws"])

@router.websocket("/{token}")
async def ws_challenge(websocket: WebSocket, token: str):
    """
    Accept websocket connection and subscribe to pubsub channel. Only forwards events matching the token.
    """
    await websocket.accept()
    logger.info("ws connected token=%s", token)
    try:
        async for ev in subscribe_challenge_events():
            try:
                ev_token = ev.get("token")
            except Exception:
                ev_token = None
            if ev_token == token:
                await websocket.send_json(ev)
    except WebSocketDisconnect:
        logger.info("ws disconnected token=%s", token)
        return
    except Exception as e:
        logger.exception("ws error for token=%s error=%s", token, str(e))
        try:
            await websocket.send_json({"error": str(e)})
        except Exception:
            pass
        await websocket.close()

"""
Persian:
    WebSocket endpoint async برای جریان چالش با استفاده از Redis pub/sub.
    اتصال WebSocket پیام‌های مربوط به توکن متصل‌شده را به صورت real-time ارسال می‌کند.

English:
    Async WebSocket endpoint to stream challenge events using Redis pub/sub.
    The WebSocket forwards events related to the connected token in real-time.
"""

from fastapi import APIRouter, WebSocket, WebSocketDisconnect
from app.services.challenge_store_redis import subscribe_challenge_events
import asyncio

router = APIRouter(prefix="/ws/challenge", tags=["challenge-ws"])

@router.websocket("/{token}")
async def ws_challenge(websocket: WebSocket, token: str):
    """
    Persian:
        اتصال websocket را قبول کرده و به کانال pubsub متصل می‌شود. تنها eventهای مرتبط با token ارسال می‌شوند.

    English:
        Accept websocket connection and subscribe to pubsub channel. Only forwards events matching the token.
    """
    await websocket.accept()
    try:
        async for ev in subscribe_challenge_events():
            try:
                ev_token = ev.get("token")
            except Exception:
                ev_token = None
            if ev_token == token:
                await websocket.send_json(ev)
    except WebSocketDisconnect:
        return
    except Exception as e:
        try:
            await websocket.send_json({"error": str(e)})
        except Exception:
            pass
        await websocket.close()

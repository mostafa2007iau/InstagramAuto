"""
Persian:
    Challenge store مبتنی بر Redis. این ماژول چالش‌ها را در Redis ذخیره می‌کند، TTL را اعمال می‌کند
    و هنگام تغییر وضعیت، پیام pubsub منتشر می‌کند تا WebSocket subscribers بتوانند آن را دریافت کنند.

English:
    Redis-backed challenge store. This module persists challenges into Redis with TTL and publishes
    pub/sub messages on state changes so WebSocket subscribers can receive realtime updates.
"""

from typing import Optional, Dict, Any
import os
import json
import time
import uuid
import asyncio

import aioredis

REDIS_URL = os.getenv("REDIS_URL", "redis://localhost:6379/0")
_CHALLENGE_PREFIX = "challenge:"
_PUBSUB_CHANNEL = "challenge_events"
TTL_SECONDS = int(os.getenv("CHALLENGE_TTL", "300"))

# Lazy singleton Redis pool
_redis = None

async def _get_redis():
    global _redis
    if _redis is None:
        _redis = await aioredis.from_url(REDIS_URL, encoding="utf-8", decode_responses=True)
    return _redis

async def create_challenge(session_id: str, ch_type: str, payload: Dict[str, Any]) -> str:
    """
    Persian:
        یک چالش جدید می‌سازد، آن را در Redis ذخیره می‌کند و event انتشار می‌دهد.

    English:
        Create a new challenge, persist it in Redis and publish an event.
    """
    token = str(uuid.uuid4())
    key = _CHALLENGE_PREFIX + token
    st = {
        "session_id": session_id,
        "type": ch_type,
        "payload": payload,
        "created_at": time.time()
    }
    r = await _get_redis()
    await r.set(key, json.dumps(st), ex=TTL_SECONDS)
    # publish create event
    await r.publish(_PUBSUB_CHANNEL, json.dumps({"token": token, "event": "created", "state": st}))
    return token

async def get_challenge(token: str) -> Optional[Dict[str, Any]]:
    """
    Persian:
        وضعیت یک چالش را از Redis می‌خواند. اگر منقضی شده باشد None برمی‌گرداند.

    English:
        Read the challenge state from Redis. Returns None if expired/not found.
    """
    r = await _get_redis()
    key = _CHALLENGE_PREFIX + token
    raw = await r.get(key)
    if not raw:
        return None
    try:
        return json.loads(raw)
    except Exception:
        return None

async def resolve_challenge(token: str, response: Dict[str, Any]) -> Dict[str, Any]:
    """
    Persian:
        چالش را حل می‌کند: state را حذف می‌کند و یک event انتشار می‌دهد.

    English:
        Resolve a challenge: delete state and publish a resolved event.
    """
    r = await _get_redis()
    key = _CHALLENGE_PREFIX + token
    st_raw = await r.get(key)
    if not st_raw:
        raise RuntimeError("challenge not found or expired")
    try:
        st = json.loads(st_raw)
    except Exception:
        st = {"session_id": None}
    await r.delete(key)
    await r.publish(_PUBSUB_CHANNEL, json.dumps({"token": token, "event": "resolved", "response": response, "state": st}))
    return {"ok": True, "applied": True}

# helper: subscribe async generator for pubsub messages (used by WebSocket router)
async def subscribe_challenge_events():
    """
    Persian:
        یک async generator برای مشترک شدن به کانال pubsub ارسال می‌کند.
        استفاده: async for msg in subscribe_challenge_events(): ...
    English:
        An async generator to subscribe to pubsub channel. Usage: async for msg in subscribe_challenge_events(): ...
    """
    r = await _get_redis()
    pubsub = r.pubsub()
    await pubsub.subscribe(_PUBSUB_CHANNEL)
    try:
        while True:
            message = await pubsub.get_message(ignore_subscribe_messages=True, timeout=1.0)
            if message:
                # message: {'type':'message', 'channel': '...', 'data': '...'}
                try:
                    data = json.loads(message["data"])
                except Exception:
                    data = {"raw": message["data"]}
                yield data
            else:
                await asyncio.sleep(0.1)
    finally:
        await pubsub.unsubscribe(_PUBSUB_CHANNEL)
        await pubsub.close()

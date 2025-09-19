"""
Persian:
    تست گردش چالش: ایجاد چالش، اتصال WebSocket و حل چالش.

English:
    Test challenge lifecycle: create, connect WS, resolve.
"""

import pytest
from fastapi import WebSocket
from httpx import AsyncClient
from app.main import app
from app.services.challenge_manager import create_challenge
import asyncio

@pytest.mark.asyncio
async def test_challenge_ws_and_resolve(tmp_path):
    token = create_challenge("sess_ws", "2fa", {"info":"sample"})
    async with AsyncClient(app=app, base_url="http://test") as ac:
        # connect websocket
        url = f"ws://test/ws/challenge/{token}"
        async with ac.websocket_connect(f"/ws/challenge/{token}") as ws:
            msg = await ws.receive_json()  # first pending state
            assert msg["status"] in ("pending",)
            # resolve via HTTP
            resp = await ac.post(f"/api/challenge/{token}/resolve", json={"code":"000000"})
            assert resp.status_code == 200
            # subsequent receive should find expired and close
            msg2 = await ws.receive_json()
            assert msg2["status"] == "expired"

"""
Persian:
    تست WebSocket چالش: ایجاد چالش، اتصال WebSocket و دریافت eventهای resolve (نیازمند redis running).

English:
    Challenge WebSocket test: create challenge, connect WS and receive resolve events (requires redis).
"""

import pytest
import os
import asyncio
from httpx import AsyncClient, AsyncClientWebSocket
from app.main import app
from app.db import init_db

@pytest.mark.asyncio
async def test_ws_challenge_events(tmp_path):
    dbfile = tmp_path / "ws.db"
    os.environ["DATABASE_URL"] = f"sqlite:///{dbfile}"
    os.environ.setdefault("REDIS_URL", "redis://localhost:6379/0")
    init_db()
    async with AsyncClient(app=app, base_url="http://test") as ac:
        # create challenge via endpoint
        resp = await ac.post("/api/challenge/create", json={"session_id":"sess_ws","type":"challenge","payload":{"info":"wstest"}})
        assert resp.status_code == 200
        token = resp.json()["token"]

        # connect to websocket and then resolve challenge to see event
        async with ac.websocket_connect(f"/ws/challenge/{token}") as ws:
            # receive create event (pubsub publishing is async; wait briefly)
            msg = await ws.receive_json(timeout=5.0)
            # first event may be 'created' or pending state, ensure token matches
            assert msg.get("token") == token
            # resolve the challenge via HTTP
            resp2 = await ac.post(f"/api/challenge/{token}/resolve", json={"code":"111111"})
            assert resp2.status_code == 200
            # receive resolved event
            msg2 = await ws.receive_json(timeout=5.0)
            assert msg2.get("event") == "resolved"
            assert msg2.get("token") == token

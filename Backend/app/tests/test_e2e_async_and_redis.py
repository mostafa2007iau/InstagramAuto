"""
Persian:
    تست E2E برای جریان async شامل: ایجاد سشن، لاگین (mock)، دریافت medias با pagination،
    و جریان ایجاد/حل چالش با Redis-backed store (نیازمند redis در حال اجرا).

English:
    E2E test for async flows: create session, login (mock), list medias pagination,
    and challenge create/resolve using Redis-backed store (requires redis running).
"""

import pytest
import os
import asyncio
from httpx import AsyncClient
from app.main import app
from app.db import init_db
from app.services.session_manager import SessionManager
from app.services.challenge_store_redis import create_challenge, get_challenge, resolve_challenge

@pytest.mark.asyncio
async def test_e2e_flow_with_redis(tmp_path, monkeypatch):
    # Prepare fresh DB
    dbfile = tmp_path / "e2e_async.db"
    os.environ["DATABASE_URL"] = f"sqlite:///{dbfile}"
    # Ensure redis env points to real redis in local dev (docker-compose)
    os.environ.setdefault("REDIS_URL", "redis://localhost:6379/0")
    init_db()
    async with AsyncClient(app=app, base_url="http://test") as ac:
        # create session
        resp = await ac.post("/api/accounts/sessions", json={"account_id":"acct_e2e","proxy":None,"proxy_enabled":False,"locale":"en"})
        assert resp.status_code == 200
        session = resp.json()
        sid = session["id"]

        # login (SessionManager uses Mock or real instagrapi depending on env)
        resp2 = await ac.post(f"/api/accounts/sessions/{sid}/login", params={"username":"user1","password":"pwd"})
        assert resp2.status_code == 200
        body2 = resp2.json()
        assert "result" in body2

        # medias
        resp3 = await ac.get("/api/medias", params={"session_id": sid, "limit": 5})
        assert resp3.status_code == 200
        body3 = resp3.json()
        assert "items" in body3

        # Challenge create via endpoint (simulate login returning challenge)
        resp4 = await ac.post("/api/challenge/create", json={"session_id": sid, "type":"challenge","payload":{"info":"test"}})
        assert resp4.status_code == 200
        token = resp4.json()["token"]

        # get challenge via HTTP
        resp5 = await ac.get(f"/api/challenge/{token}")
        assert resp5.status_code == 200
        state = resp5.json()["state"]
        assert state["session_id"] == sid

        # resolve challenge
        resp6 = await ac.post(f"/api/challenge/{token}/resolve", json={"code":"000000"})
        assert resp6.status_code == 200
        assert resp6.json()["result"]["ok"] is True

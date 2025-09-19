"""
Persian:
    تست E2E ساده برای مسیرهای create session -> login (mocked) -> medias pagination.

English:
    Simple E2E test for create session -> login -> medias pagination flow using httpx AsyncClient.
"""

import pytest
from httpx import AsyncClient
from app.main import app
from app.db import init_db
import os
import asyncio

@pytest.mark.asyncio
async def test_e2e_create_login_list_medias(tmp_path, monkeypatch):
    # fresh DB
    dbfile = tmp_path / "e2e.db"
    os.environ["DATABASE_URL"] = f"sqlite:///{dbfile}"
    init_db()

    async with AsyncClient(app=app, base_url="http://test") as ac:
        # create session
        resp = await ac.post("/api/accounts/sessions", json={"account_id":"acct_e2e","proxy":None,"proxy_enabled":False,"locale":"en"})
        assert resp.status_code == 200
        session = resp.json()
        sid = session["id"]

        # monkeypatch session_manager to bypass real instagrapi
        # call login endpoint (will use MockInstaClient behavior from SessionManager)
        resp2 = await ac.post(f"/api/accounts/sessions/{sid}/login", params={"username":"user1","password":"pwd"})
        assert resp2.status_code == 200
        data = resp2.json()
        assert "result" in data

        # list medias (pagination)
        resp3 = await ac.get("/api/medias", params={"session_id": sid, "limit": 5})
        assert resp3.status_code == 200
        body = resp3.json()
        assert "items" in body and isinstance(body["items"], list)

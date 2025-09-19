"""
Persian:
    تست integration برای لاگین واقعی با instagrapi (اگر در محیط نصب شود).
    این تست برای محیط محلی یا staging است و network-dependent می‌باشد.

English:
    Integration test for real instagrapi login. This test is network-dependent and intended for local/staging runs.
"""

import os
import pytest
import asyncio
from app.services.insta_client_factory import InstaClientFactory
from app.services.insta_login_handler import handle_login_flow
from app.services.session_manager import SessionManager
from app.db import init_db

@pytest.mark.asyncio
async def test_real_login_flow(tmp_path):
    # DB setup
    dbfile = tmp_path / "insta_integ.db"
    os.environ["DATABASE_URL"] = f"sqlite:///{dbfile}"
    init_db()

    # credentials: prefer env, fallback to provided test account
    username = os.getenv("INSTA_TEST_USERNAME", "kidsprans")
    password = os.getenv("INSTA_TEST_PASSWORD", "mostafa2007")

    # create session manager and session
    sm = SessionManager()
    sess = await sm.create_session("acct_integ", proxy=None, proxy_enabled=False, locale="en")
    sid = sess.id

    # call login handler (this will attempt to use real instagrapi if installed)
    res = await handle_login_flow(sm, sid, username, password, locale="en")
    # Accept either success or a challenge_required result; ensure structured response
    assert isinstance(res, dict)
    assert "ok" in res
    if res.get("ok"):
        assert "login" in res
    else:
        # if not ok, must be either challenge_required or error mapping
        assert res.get("challenge_required") or res.get("error")

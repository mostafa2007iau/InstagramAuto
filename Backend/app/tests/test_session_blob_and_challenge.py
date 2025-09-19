"""
Persian:
    تست ذخیره و بازیابی session_blob و جریان چالش ساده.

English:
    Test session_blob persistence and simple challenge flow.
"""

import os
from app.services.session_manager import SessionManager
from app.db import init_db
from app.services.challenge_manager import create_challenge, get_challenge, resolve_challenge

def test_session_blob_persistence(tmp_path):
    dbfile = tmp_path / "test_blob.db"
    os.environ["DATABASE_URL"] = f"sqlite:///{dbfile}"
    init_db()
    sm = SessionManager()
    s = sm.create_session("acct_blob", locale="en")
    # ensure client exists
    assert s.id in sm.clients
    # simulate client with dump_session_bytes by monkeypatching
    client_wrapper = sm.clients[s.id]
    # monkeypatch dump to return bytes
    client_wrapper.dump_session_bytes = lambda: b"session-bytes-sample"
    sm.persist_client_session(s.id)
    # reload session from DB
    sess_db = sm.get_session(s.id)
    assert sess_db.session_blob is not None

def test_challenge_lifecycle():
    token = create_challenge("sess1", "2fa", {"info": "sample"})
    st = get_challenge(token)
    assert st is not None
    res = resolve_challenge(token, {"code": "123456"})
    assert res.get("ok", False) is True
    assert get_challenge(token) is None

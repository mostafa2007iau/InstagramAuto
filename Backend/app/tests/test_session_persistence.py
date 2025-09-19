from app.services.session_manager import SessionManager
from app.db import init_db, engine
from sqlmodel import Session
import os

def test_create_persist_and_export(tmp_path):
    # ensure fresh DB
    dbfile = tmp_path / "test.db"
    os.environ["DATABASE_URL"] = f"sqlite:///{dbfile}"
    init_db()
    sm = SessionManager()
    s = sm.create_session("acct_x", proxy="http://1.2.3.4:8080", proxy_enabled=True, locale="fa")
    assert s.id is not None
    exported = sm.export_session(s.id)
    assert "acct_x" in exported
    # import
    s2 = sm.import_session(exported)
    assert s2.account_id == "acct_x"

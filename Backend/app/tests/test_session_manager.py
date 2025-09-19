"""
Persian:
    تست‌های واحد برای SessionManager: probe فراخوانی می‌شود و proxy هنگام فعال بودن اعمال می‌گردد.

English:
    Unit tests for SessionManager ensuring probe is called and proxy is applied when enabled.
"""

import pytest
from app.services.session_manager import SessionManager

def test_create_session_and_apply_proxy():
    sm = SessionManager()
    meta = sm.create_session("acct1", proxy="http://127.0.0.1:8888", proxy_enabled=True, locale="en")
    sm.apply_proxy_if_enabled(meta.id)
    client = sm.clients[meta.id]
    assert client._proxy == "http://127.0.0.1:8888"

def test_probe_returns_medias_for_non_empty():
    sm = SessionManager()
    meta = sm.create_session("acct2", proxy_enabled=False, locale="en")
    res = sm.probe_medias(meta.id, "user_acct2", amount=3)
    assert isinstance(res, list)

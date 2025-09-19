"""
Persian:
    تست سرویس پروکسی (این تست شبکه‌ای است و می‌تواند mock شود).

English:
    Test proxy service (network dependent; should be mocked in CI).
"""

from app.services.proxy_service import test_proxy

def test_test_proxy_invalid():
    ok, msg = test_proxy("http://invalid:1234", locale="en", timeout=1)
    assert ok is False

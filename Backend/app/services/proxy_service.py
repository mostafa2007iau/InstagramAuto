"""
Persian:
    سرویس تست و اعتبارسنجی پروکسی که می‌تواند request ای ساده به مقصد نمونه بزند و نتیجه را محلی‌سازی‌شده برگرداند.

English:
    Proxy test/validation service that performs a simple request through a candidate proxy and returns localized result.
"""

from typing import Tuple
from app.i18n import translate
import requests

def test_proxy(proxy_url: str, locale: str = "en", timeout: int = 5) -> Tuple[bool, str]:
    """
    Persian:
        تست می‌کند آیا پروکسی داده شده کار می‌کند یا نه با یک درخواست HTTP سریع.

    English:
        Tests whether the given proxy works by making a quick HTTP request.
    """
    proxies = {"http": proxy_url, "https": proxy_url}
    try:
        resp = requests.get("https://httpbin.org/ip", proxies=proxies, timeout=timeout)
        if resp.status_code == 200:
            return True, translate("proxy.test.success", locale=locale)
        return False, translate("proxy.test.failure", locale=locale, reason=f"status={resp.status_code}")
    except Exception as e:
        return False, translate("proxy.test.failure", locale=locale, reason=str(e))

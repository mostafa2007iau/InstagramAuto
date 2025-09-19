"""
Persian:
    پیاده‌سازی ساده exponential backoff برای تلاش‌های مجدد.

English:
    Simple exponential backoff implementation for retry attempts.
"""

import time
from typing import Callable, Any

def retry_with_backoff(attempts: int, base: float = 0.5):
    def decorator(func: Callable[..., Any]):
        def wrapper(*args, **kwargs):
            last_exc = None
            for i in range(attempts):
                try:
                    return func(*args, **kwargs)
                except Exception as e:
                    last_exc = e
                    sleep_time = base * (2 ** i)
                    time.sleep(sleep_time)
            raise last_exc
        return wrapper
    return decorator

"""
Persian:
    Helpers برای اعمال rate-limit در مسیرها با fastapi-limiter.
    شامل نمونه dependency برای محدودیت پیش‌فرض (مثلاً 20 درخواست در دقیقه).

English:
    Helpers to apply rate-limit to endpoints using fastapi-limiter.
    Provides a sample dependency for default limits (e.g., 20 req/min).
"""

from fastapi_limiter.depends import RateLimiter

# Default rate: 20 requests per minute per client (by IP)
# در endpointها از Depends(default_rate_limit) استفاده کن
default_rate_limit = RateLimiter(times=20, seconds=60)
# مثال برای stricter limit:
strict_rate_limit = RateLimiter(times=5, seconds=60)

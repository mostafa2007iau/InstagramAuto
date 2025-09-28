"""
Persian:
    Helpers برای اعمال rate-limit در مسیرها با fastapi-limiter.
    شامل نمونه dependency برای محدودیت پیش‌فرض (مثلاً 20 درخواست در دقیقه).

English:
    Helpers to apply rate-limit to endpoints using fastapi-limiter.
    Provides a sample dependency for default limits (e.g., 20 req/min).
"""

from fastapi import Request
from starlette.middleware.base import BaseHTTPMiddleware
from fastapi.responses import Response
from fastapi_limiter.depends import RateLimiter

# نمونه dependency برای محدودیت نرخ
default_rate_limit = RateLimiter(times=20, seconds=60)
strict_rate_limit = RateLimiter(times=5, seconds=60)


class VerbosityMiddleware(BaseHTTPMiddleware):
    async def dispatch(self, request: Request, call_next):
        # مثال: خواندن کوئری‌پارامتر verbosity
        request.state.verbosity = request.query_params.get("verbosity", "normal")
        response: Response = await call_next(request)
        return response

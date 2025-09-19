"""
Persian:
    middleware خواندن header X-Client-Log-Level و قرار دادن آن در request.state.log_verbosity.

English:
    Middleware that reads X-Client-Log-Level header and stores it in request.state.log_verbosity.
"""

from starlette.middleware.base import BaseHTTPMiddleware
from starlette.requests import Request
from app.config import LogVerbosity

class VerbosityMiddleware(BaseHTTPMiddleware):
    async def dispatch(self, request: Request, call_next):
        header = request.headers.get("x-client-log-level", None)
        try:
            verbosity = LogVerbosity(header) if header else LogVerbosity.SUMMARY
        except Exception:
            verbosity = LogVerbosity.SUMMARY
        request.state.log_verbosity = verbosity
        response = await call_next(request)
        return response

"""
Persian:
    فراهم‌کننده‌های وابستگی برای FastAPI. این نسخه get_session_manager همان نمونه Singleton از SessionManager async را بازمی‌گرداند.
    دیگر فراهم‌کننده‌ها شامل locale و client log verbosity هستند.

English:
    Dependency providers for FastAPI. get_session_manager returns a singleton instance of the async SessionManager.
    Other providers include locale and client log verbosity.
"""

from fastapi import Request, Depends
from app.services.session_manager import SessionManager
from app.config import LOG_VERBOSITY_DEFAULT, LogVerbosity
from app.db import get_session

# Singleton session manager instance used by DI
_session_manager = None

def get_session_manager(db=Depends(get_session)) -> SessionManager:
    global _session_manager
    if _session_manager is None:
        _session_manager = SessionManager(db)
    return _session_manager

def get_locale(request: Request) -> str:
    # priority: query param 'locale' -> Accept-Language header -> DEFAULT
    q = request.query_params.get("locale")
    if q:
        return q
    accept = request.headers.get("accept-language", "")
    if accept.startswith("fa"):
        return "fa"
    return "en"

def get_client_log_verbosity(request: Request) -> LogVerbosity:
    return getattr(request.state, "log_verbosity", LOG_VERBOSITY_DEFAULT)

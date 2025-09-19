"""
Persian:
    قالب‌بندی پیام‌های لاگ براساس verbosity و locale برای قرار دادن در بدنه پاسخ یا metadata.

English:
    Format log payloads according to verbosity and locale for inclusion in response bodies or metadata.
"""

from app.config import LogVerbosity
from typing import Any, Dict

def format_log(payload: str, verbosity: LogVerbosity, locale: str) -> Dict[str, Any]:
    """
    Persian:
        بسته به verbosity یک آبجکت لاگ مناسب می‌سازد.

    English:
        Build a log object according to verbosity level.
    """
    if verbosity == LogVerbosity.NONE:
        return {}
    if verbosity == LogVerbosity.SUMMARY:
        return {"summary": payload[:200], "locale": locale}
    return {"full": payload, "locale": locale}

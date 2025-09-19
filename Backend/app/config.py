"""
Persian:
    پیکربندی برنامه. این نسخه مقادیر را از متغیرهای محیطی می‌خواند و در صورت نبودن مقادیر ضروری (مثل FERNET_KEY یا CURSOR_SECRET)
    برنامه را fail-fast می‌کند تا از رفتار ناامن جلوگیری شود.

English:
    Application configuration loader. This version reads critical secrets from environment and fail-fast
    if required secrets (FERNET_KEY, CURSOR_SECRET) are not present.
"""

import os
from enum import Enum

class LogVerbosity(str, Enum):
    NONE = "NONE"
    SUMMARY = "SUMMARY"
    FULL = "FULL"

# Critical secrets (must be provided by environment or secret manager)
FERNET_KEY = os.getenv("FERNET_KEY")
CURSOR_SECRET = os.getenv("CURSOR_SECRET")

if not FERNET_KEY:
    raise RuntimeError("FERNET_KEY is not set. Provide a secure Fernet key via environment or secret manager.")

if not CURSOR_SECRET:
    raise RuntimeError("CURSOR_SECRET is not set. Provide a CURSOR_SECRET via environment or secret manager.")

# App defaults
DEFAULT_LOCALE = os.getenv("DEFAULT_LOCALE", "en")
LOG_VERBOSITY_DEFAULT = LogVerbosity(os.getenv("LOG_VERBOSITY", "SUMMARY"))
INSTAGRAPI_TIMEOUT = int(os.getenv("INSTAGRAPI_TIMEOUT", "30"))
PROBE_RETRY_ATTEMPTS = int(os.getenv("PROBE_RETRY_ATTEMPTS", "3"))
PROBE_RETRY_BACKOFF_BASE = float(os.getenv("PROBE_RETRY_BACKOFF_BASE", "0.5"))

# Redis and DB configuration
DATABASE_URL = os.getenv("DATABASE_URL", "sqlite:///./dev.db")
REDIS_URL = os.getenv("REDIS_URL", "redis://localhost:6379/0")

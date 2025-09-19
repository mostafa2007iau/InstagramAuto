"""
Persian:
    کارخانهٔ ساخت و wrapper برای instagrapi.Client که:
    - در صورت موجود بودن instagrapi از آن استفاده می‌کند و در غیر این صورت یک Mock قابل تست فراهم می‌کند،
    - متد استاندارد get_user_medias_page(uid, limit, provider_cursor) را پیاده‌سازی می‌کند و (items, next_provider_cursor) برمی‌گرداند،
    - متدهای dump/load session را فراهم می‌کند (برای ذخیره encrypted session_blob) و helperهای encrypt/decrypt را عرضه می‌کند،
    - set_proxy, login, user_medias و send_direct_message (در صورت پشتیبانی client) را فراهم می‌کند.

English:
    Factory and wrapper for instagrapi.Client that:
    - uses instagrapi when available, falls back to a testable Mock otherwise,
    - exposes a standardized get_user_medias_page(uid, limit, provider_cursor) returning (items, next_provider_cursor),
    - provides dump/load session helpers (for persisting encrypted session_blob) and encrypt/decrypt helpers,
    - exposes set_proxy, login, user_medias and send_direct_message (if supported by the underlying client).
"""

from typing import Optional, Tuple, List, Any
import os
import importlib
import json
import base64
from cryptography.fernet import Fernet

from app.config import FERNET_KEY, INSTAGRAPI_TIMEOUT, REDIS_URL, CURSOR_SECRET

# Ensure FERNET_KEY provided in config (app.config enforces this)
fernet = Fernet(FERNET_KEY.encode() if isinstance(FERNET_KEY, str) else FERNET_KEY)

def _import_instagrapi():
    """
    Try to import instagrapi lazily. If not available, callers will fall back to Mock client.
    """
    try:
        return importlib.import_module("instagrapi")
    except Exception:
        return None

class BaseInstaClientWrapper:
    """
    Interface-like wrapper: concrete wrappers implement these methods.
    Methods may be sync or async depending on underlying client; callers should handle both.
    """
    def set_proxy(self, proxy: Optional[str]):
        raise NotImplementedError

    def login(self, username: str, password: str) -> Any:
        raise NotImplementedError

    def user_medias(self, uid: str, amount: int) -> List[dict]:
        raise NotImplementedError

    def get_user_medias_page(self, uid: str, limit: int, provider_cursor: Optional[str]) -> Tuple[List[dict], Optional[str]]:
        """
        Return (items, next_provider_cursor) where next_provider_cursor is provider-specific token (e.g., max_id)
        """
        raise NotImplementedError

    def dump_session_bytes(self) -> bytes:
        raise NotImplementedError

    def load_session_bytes(self, b: bytes):
        raise NotImplementedError

    def send_direct_message(self, *args, **kwargs):
        """
        Optional: send a direct message or perform a send action. Not all clients support this.
        """
        raise NotImplementedError

# Real instagrapi wrapper (if instagrapi available)
class InstaClientRealWrapper(BaseInstaClientWrapper):
    def __init__(self, client):
        self.client = client

    @classmethod
    def create_new(cls):
        instagrapi = _import_instagrapi()
        if not instagrapi:
            raise RuntimeError("instagrapi not installed")
        # instantiate Client per installed version API
        client = instagrapi.Client(timeout=INSTAGRAPI_TIMEOUT) if hasattr(instagrapi, "Client") else instagrapi.Client()
        return cls(client)

    def set_proxy(self, proxy: Optional[str]):
        try:
            if proxy:
                self.client.set_proxy(proxy)
            else:
                self.client.set_proxy(None)
        except Exception:
            # fallback to session proxies mutation
            if hasattr(self.client, "session"):
                if proxy:
                    self.client.session.proxies.update({"http": proxy, "https": proxy})
                else:
                    self.client.session.proxies.clear()

    def login(self, username: str, password: str):
        # instagrapi.Client.login may return dict or object or raise exceptions
        return self.client.login(username, password)

    def user_medias(self, uid: str, amount: int):
        # many versions accept numeric uid or username; caller decides
        return self.client.user_medias(uid, amount)

    def get_user_medias_page(self, uid: str, limit: int, provider_cursor: Optional[str]):
        """
        Try to support common instagrapi pagination patterns:
        - If provider_cursor is provided and client supports a param like max_id, attempt to pass it.
        - If client exposes a paginator/iterator, consume required count and return next token if available.
        - If client returns dict with next_max_id, return it as provider_cursor.

        Returns (items_list, next_provider_cursor_or_none)
        """
        # Attempt to call with provider_cursor where possible
        try:
            # prefer a client method that supports pagination token by position
            # many implementations accept (user_id, amount, max_id) or (user_id, amount, max_id=None)
            try:
                if provider_cursor is not None:
                    raw = self.client.user_medias(uid, limit, provider_cursor)
                else:
                    raw = self.client.user_medias(uid, limit)
            except TypeError:
                # signature didn't accept provider_cursor; call without it
                raw = self.client.user_medias(uid, limit)
        except Exception as e:
            # as a last resort, try iterator approach
            try:
                it = self.client.user_medias_iter(uid)
                items = []
                next_cursor = None
                for i, media in enumerate(it):
                    if i >= limit:
                        # provider-specific iterators may expose state; we don't have it here
                        break
                    items.append(media)
                return items, next_cursor
            except Exception:
                raise

        # Interpret raw
        if isinstance(raw, dict):
            items = raw.get("items", []) or raw.get("medias", []) or []
            next_cursor = raw.get("next_max_id") or raw.get("next_cursor") or raw.get("max_id")
            return items, next_cursor
        if isinstance(raw, list):
            # no next cursor info available
            return raw, None
        # fallback: try to coerce to list
        try:
            return list(raw), None
        except Exception:
            return [], None

    def dump_session_bytes(self) -> bytes:
        # prefer official dump_session_to_bytes if present
        if hasattr(self.client, "dump_session_to_bytes"):
            return self.client.dump_session_to_bytes()
        # fallback to dump_session (may write to file) -> try to retrieve settings
        if hasattr(self.client, "settings"):
            try:
                return json.dumps(self.client.settings).encode()
            except Exception:
                pass
        raise RuntimeError("No session dump method available on instagrapi client")

    def load_session_bytes(self, b: bytes):
        if hasattr(self.client, "load_session_from_bytes"):
            return self.client.load_session_from_bytes(b)
        if hasattr(self.client, "load_session"):
            try:
                return self.client.load_session(b)
            except Exception:
                # maybe expects string
                return self.client.load_session(b.decode() if isinstance(b, bytes) else b)
        # best-effort: set settings attribute
        try:
            self.client.settings = json.loads(b.decode() if isinstance(b, bytes) else b)
        except Exception:
            pass

    def send_direct_message(self, *args, **kwargs):
        # instagrapi client may have direct_message methods; try common ones
        if hasattr(self.client, "direct_send"):
            return self.client.direct_send(*args, **kwargs)
        if hasattr(self.client, "direct_message"):
            return self.client.direct_message(*args, **kwargs)
        # not supported
        raise NotImplementedError("send_direct_message not implemented by underlying instagrapi client")

# Mock wrapper used for tests and when instagrapi is not installed
class MockInstaClientWrapper(BaseInstaClientWrapper):
    def __init__(self):
        self._proxy = None
        self._store = {}  # simple in-memory medias per uid
        # prefill
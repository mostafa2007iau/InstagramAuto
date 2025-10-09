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

class InstaClientRealWrapper(BaseInstaClientWrapper):
    def __init__(self, client):
        self.client = client

    @classmethod
    def create_new(cls):
        instagrapi = _import_instagrapi()
        if not instagrapi:
            raise RuntimeError("instagrapi not installed")
        client = instagrapi.Client(timeout=INSTAGRAPI_TIMEOUT) if hasattr(instagrapi, "Client") else instagrapi.Client()
        return cls(client)

    def set_proxy(self, proxy: Optional[str]):
        try:
            if proxy:
                self.client.set_proxy(proxy)
            else:
                self.client.set_proxy(None)
        except Exception:
            if hasattr(self.client, "session"):
                if proxy:
                    self.client.session.proxies.update({"http": proxy, "https": proxy})
                else:
                    self.client.session.proxies.clear()

    def login(self, username: str, password: str):
        return self.client.login(username, password)

    def _ensure_numeric_uid(self, uid: str):
        """
        اگر uid رشته‌ی username باشه، تبدیلش می‌کنیم به user_id عددی.
        """
        if isinstance(uid, str) and not uid.isdigit():
            try:
                return self.client.user_id_from_username(uid)
            except Exception:
                return uid
        return uid

    def user_medias(self, uid: str, amount: int):
        uid = self._ensure_numeric_uid(uid)
        return self.client.user_medias(uid, amount)

    def get_user_medias_page(self, uid: str, limit: int, provider_cursor: Optional[str]):
        uid = self._ensure_numeric_uid(uid)
        try:
            if provider_cursor is not None:
                raw = self.client.user_medias(uid, limit, provider_cursor)
            else:
                raw = self.client.user_medias(uid, limit)
        except TypeError:
            raw = self.client.user_medias(uid, limit)
        except Exception:
            # fallback به iterator
            try:
                it = self.client.user_medias_iter(uid)
                items, next_cursor = [], None
                for i, media in enumerate(it):
                    if i >= limit:
                        break
                    items.append(media)
                return items, next_cursor
            except Exception:
                raise

        if isinstance(raw, dict):
            items = raw.get("items", []) or raw.get("medias", []) or []
            next_cursor = raw.get("next_max_id") or raw.get("next_cursor") or raw.get("max_id")
            return items, next_cursor
        if isinstance(raw, list):
            return raw, None
        try:
            return list(raw), None
        except Exception:
            return [], None

    def dump_session_bytes(self) -> bytes:
        if hasattr(self.client, "dump_session_to_bytes"):
            return self.client.dump_session_to_bytes()
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
                return self.client.load_session(b.decode() if isinstance(b, bytes) else b)
        try:
            self.client.settings = json.loads(b.decode() if isinstance(b, bytes) else b)
        except Exception:
            pass

    def send_direct_message(self, *args, **kwargs):
        if hasattr(self.client, "direct_send"):
            return self.client.direct_send(*args, **kwargs)
        if hasattr(self.client, "direct_message"):
            return self.client.direct_message(*args, **kwargs)
        raise NotImplementedError("send_direct_message not implemented by underlying instagrapi client")


# Mock wrapper used for tests and when instagrapi is not installed
class MockInstaClientWrapper(BaseInstaClientWrapper):
    def __init__(self):
        self._proxy = None
        self._store = {}  # simple in-memory medias per uid
        # prefill example medias for demonstration
        self._store_default = [{"id": f"m_{i}", "caption": f"sample {i}", "thumbnail_url": None} for i in range(50)]
        self._last_response = {}

    @classmethod
    def create_new(cls):
        return cls()

    def set_proxy(self, proxy: Optional[str]):
        self._proxy = proxy

    def login(self, username: str, password: str):
        # simple mock behavior:
        if username == "bad":
            # simulate bad password behavior
            return {"error": "bad_password", "status": "fail"}
        if username.startswith("challenge"):
            return {"challenge_required": True, "challenge_options": {"via": "sms"}}
        # success
        return {"status": "ok", "user_id": f"user_{username}"}

    def user_medias(self, uid: str, amount: int):
        # return first `amount` items from default store
        return self._store_default[:amount]

    def get_user_medias_page(self, uid: str, limit: int, provider_cursor: Optional[str]):
        # provider_cursor simulates an offset encoded as int string
        offset = 0
        if provider_cursor:
            try:
                offset = int(provider_cursor)
            except Exception:
                offset = 0
        items = self._store_default[offset: offset + limit]
        next_cursor = str(offset + limit) if (offset + limit) < len(self._store_default) else None
        return items, next_cursor

    def dump_session_bytes(self) -> bytes:
        return b"mock-session-bytes"

    def load_session_bytes(self, b: bytes):
        # no-op for mock
        return

    def send_direct_message(self, target, message):
        # simulate success
        return {"ok": True, "sent_to": target, "message": message}

# Factory that returns either real wrapper or mock wrapper
class InstaClientFactory:
    @staticmethod
    def create_new():
        instagrapi = _import_instagrapi()
        if instagrapi:
            try:
                return InstaClientRealWrapper.create_new()
            except Exception:
                # fallback to mock if instagrapi import works but initialization fails
                return MockInstaClientWrapper.create_new()
        else:
            return MockInstaClientWrapper.create_new()

# Helpers: encrypt/decrypt session bytes to base64 token
def encrypt_session_bytes(b: bytes) -> str:
    token = fernet.encrypt(b)
    return base64.urlsafe_b64encode(token).decode()

def decrypt_session_token(token: str) -> bytes:
    raw = base64.urlsafe_b64decode(token.encode())
    return fernet.decrypt(raw)

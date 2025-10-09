"""
Persian:
    مدیریت سشن‌ها و اتصال به اینستاگرام.
    دریافت پست‌های واقعی کاربر.

English:
    Session management and Instagram connection.
    Fetching real user posts.
"""

from typing import Dict, Any, List, Optional, Tuple
from datetime import datetime
import asyncio
import logging
from .insta_client_factory import InstaClientFactory, encrypt_session_bytes, decrypt_session_token
from app.schemas.session_schema import SessionCreateIn, SessionOut
from app.schemas.media_schema import MediaItem

logger = logging.getLogger(__name__)

class SessionManager:
    def __init__(self, db):
        self.db = db
        self._clients = {}

    async def create_session(
        self,
        account_id: str,
        proxy: Optional[str] = None,
        proxy_enabled: bool = False,
        locale: str = "fa"
    ) -> SessionOut:
        """Create a new session / ایجاد سشن جدید"""
        meta = await self.db.create_session(
            account_id=account_id,
            proxy=proxy,
            proxy_enabled=proxy_enabled,
            locale=locale
        )
        
        # Create client instance
        client = InstaClientFactory.create_new()
        if proxy_enabled and proxy:
            client.set_proxy(proxy)
        
        self._clients[meta.id] = client
        return meta

    async def login_and_probe(
        self,
        session_id: str,
        username: str,
        password: str
    ) -> Dict[str, Any]:
        """Login and probe medias / لاگین و دریافت پست‌ها"""
        client = await self._ensure_client(session_id)
        
        # 1. Login
        result = await client.login(username, password)
        if not result.get("ok", False):
            return result

        # 2. Try to get user ID and some posts to verify access
        try:
            if result.get("user_id"):
                items, _ = await client.get_user_medias_page(result["user_id"], limit=5)
                if items:
                    result["user_posts"] = items
        except Exception as e:
            logger.error(f"Error getting initial posts after login: {e}")

        # 3. Get account info if available
        try:
            if "account_info" in result:
                result["profile"] = {
                    "username": result["account_info"].get("username"),
                    "full_name": result["account_info"].get("full_name"),
                    "profile_pic_url": result["account_info"].get("profile_pic_url")
                }
        except Exception as e:
            logger.error(f"Error getting account info: {e}")

        return result

    async def _ensure_client(self, session_id: str):
        """Ensure client exists and is configured / اطمینان از وجود و پیکربندی کلاینت"""
        if session_id not in self._clients:
            self._clients[session_id] = InstaClientFactory.create_new()

            # Try to restore session if we have metadata
            meta = await self.db.get_session(session_id)
            if meta and meta.session_blob:
                try:
                    session_bytes = decrypt_session_token(meta.session_blob)
                    await self._clients[session_id].load_session_bytes(session_bytes)
                except Exception:
                    logger.warning(f"Failed to restore session {session_id}")

            # Apply proxy if enabled
            if meta and meta.proxy_enabled and meta.proxy:
                self._clients[session_id].set_proxy(meta.proxy)

        return self._clients[session_id]

    async def get_session(self, session_id: str) -> Optional[SessionOut]:
        """Get session metadata / دریافت متادیتای سشن"""
        return await self.db.get_session(session_id)

    async def delete_session(self, session_id: str):
        """Delete session / حذف سشن"""
        if session_id in self._clients:
            del self._clients[session_id]
        await self.db.delete_session(session_id)

    async def update_session(self, session_id: str, **updates):
        """Update session / به‌روزرسانی سشن"""
        await self.db.update_session(session_id, **updates)

    async def export_session(self, session_id: str) -> Dict[str, Any]:
        """Export session / خروجی گرفتن از سشن"""
        client = await self._ensure_client(session_id)
        session_bytes = client.dump_session_bytes()
        return {
            "session_blob": encrypt_session_bytes(session_bytes)
        }

    async def import_session(self, data: Dict[str, Any]) -> bool:
        """Import session / وارد کردن سشن"""
        session_id = data.get("id")
        if not session_id:
            return False
        
        client = await self._ensure_client(session_id)
        session_bytes = decrypt_session_token(data["session_blob"])
        client.load_session_bytes(session_bytes)
        return True

    async def apply_proxy_if_enabled(self, session_id: str):
        """Apply proxy if enabled / اعمال پروکسی در صورت فعال بودن"""
        meta = await self.get_session(session_id)
        if not meta:
            return
        
        client = await self._ensure_client(session_id)
        if meta.proxy_enabled and meta.proxy:
            client.set_proxy(meta.proxy)
        else:
            client.set_proxy(None)
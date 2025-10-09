"""
Persian:
    ?????? ?????????? ?? ??????? ?? Instagrapi.
    ????? ????? ?? API ?????????? ? ?????? ??????? ?????.

English:
    Instagram client using Instagrapi.
    Responsible for connecting to Instagram API and fetching user data.
"""

from instagrapi import Client
from instagrapi.exceptions import ClientError, LoginRequired
from typing import Optional, Tuple, List, Dict, Any
import logging
import json
import asyncio
from datetime import datetime

logger = logging.getLogger(__name__)

class InstagramClient:
    def __init__(self):
        self.client = Client()
        self.user_id = None
        self.username = None
        self._logged_in = False

    async def login(self, username: str, password: str) -> Dict[str, Any]:
        """Login to Instagram / ???? ?? ??????????"""
        try:
            # Clear any existing session
            self.client = Client()
            self._logged_in = False
            
            # Try to login
            logged_in = await asyncio.to_thread(
                self.client.login, username, password
            )

            if not logged_in:
                return {
                    "ok": False,
                    "message": "??? ?????? ?? ??? ???? ?????? ???",
                    "message_en": "Invalid username or password"
                }

            # Get user ID
            self.user_id = self.client.user_id
            self.username = username
            self._logged_in = True

            # Get account info
            account_info = await asyncio.to_thread(
                self.client.account_info
            )

            return {
                "ok": True,
                "user_id": self.user_id,
                "account_info": account_info
            }

        except ClientError as e:
            logger.error(f"Login failed: {e}")
            if "challenge_required" in str(e):
                return {
                    "ok": False,
                    "challenge_required": True,
                    "challenge_type": "??????",
                    "message": "???? ?? ????? ????",
                    "message_en": "Challenge required"
                }
            return {
                "ok": False,
                "message": str(e),
                "message_en": "Login failed"
            }
        except Exception as e:
            logger.exception("Unexpected error during login")
            return {
                "ok": False,
                "message": f"???? ?????????: {str(e)}",
                "message_en": f"Unexpected error: {str(e)}"
            }

    async def get_user_medias_page(
        self,
        user_id: str,
        limit: int = 20,
        cursor: Optional[str] = None
    ) -> Tuple[List[Dict], Optional[str]]:
        """Get user medias with pagination / ?????? ??????? ????? ?? ?????????"""
        if not self._logged_in:
            raise LoginRequired()

        try:
            # Convert username to user_id if needed
            if not str(user_id).isdigit():
                user_info = await asyncio.to_thread(
                    self.client.user_info_by_username, user_id
                )
                user_id = user_info.pk

            # Get user medias
            medias = await asyncio.to_thread(
                self.client.user_medias, user_id, limit
            )

            # Transform to our format
            items = []
            for media in medias:
                try:
                    item = {
                        "id": str(media.pk),
                        "caption": media.caption_text,
                        "thumbnail_url": media.thumbnail_url,
                        "media_type": media.media_type,
                        "taken_at": media.taken_at
                    }
                    items.append(item)
                except Exception as e:
                    logger.warning(f"Failed to process media {media.pk}: {e}")
                    continue

            # In instagrapi, pagination is handled differently
            # We'll implement cursor-based pagination later
            next_cursor = None

            return items, next_cursor

        except Exception as e:
            logger.exception(f"Error getting user medias: {e}")
            raise

    def set_proxy(self, proxy: Optional[str]):
        """Set proxy for client / ????? ?????? ???? ??????"""
        if proxy:
            self.client.set_proxy(proxy)
        else:
            self.client.set_proxy(None)

    async def export_session(self) -> Dict[str, Any]:
        """Export session data / ????? ????? ?? ??????? ???"""
        if not self._logged_in:
            return {}
        
        return {
            "settings": self.client.get_settings(),
            "session_id": self.client.session_id,
            "user_id": self.user_id,
            "username": self.username
        }

    async def import_session(self, data: Dict[str, Any]) -> bool:
        """Import session data / ???? ???? ??????? ???"""
        try:
            if "settings" in data:
                self.client.set_settings(data["settings"])
            if "session_id" in data:
                self.client.session_id = data["session_id"]
            if "user_id" in data:
                self.user_id = data["user_id"]
            if "username" in data:
                self.username = data["username"]
            
            # Verify session is valid
            self._logged_in = await asyncio.to_thread(
                self.client.get_timeline_feed
            ) is not None
            
            return self._logged_in
        except:
            logger.exception("Failed to import session")
            return False
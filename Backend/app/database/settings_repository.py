"""
Persian:
    SettingsRepository: ?????????? ??????? ????? (????).
English:
    Settings repository for account-level settings (stubbed implementation).
"""

from typing import Optional
from app.database.base import BaseRepository

class SettingsRepository(BaseRepository):
    def __init__(self, db=None):
        super().__init__(db)

    async def get_account_settings(self, account_id: str):
        """Return account settings. Currently returns defaults (no DB backing).
        Kept async so callers can await this method even if implementation is sync.
        """
        class Settings:
            reply_delay = 5
            dm_delay = 5
            like_delay = 2
            hourly_limit = 100
            daily_limit = 1000
            random_jitter = False
            jitter_min = 0
            jitter_max = 0
        return Settings()

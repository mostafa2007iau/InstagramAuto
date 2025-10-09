"""
Persian:
    ?????????? ??????? ????? (????).
English:
    Basic settings repository for account settings (stub).
"""

class SettingsRepository:
    async def get_account_settings(self, account_id):
        # TODO: Implement actual DB logic. For now, return default settings.
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

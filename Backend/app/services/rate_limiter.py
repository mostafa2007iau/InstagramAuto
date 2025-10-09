"""
Persian:
    ????? ????? ??? ???? ????? ???? ????? ??????.
    ????:
    - ??????? ????? ? ??????
    - ????? ??? ?????????
    - ??????? ???? ?? ??? ??????

English:
    Rate limiter service for limiting operations.
    Includes:
    - Hourly and daily limits
    - Delay between operations
    - Limits per operation type
"""

from datetime import datetime, timedelta
from typing import Dict, List
import asyncio
import logging
from app.schemas.job_schema import JobItem
from app.database.settings_repository import SettingsRepository

logger = logging.getLogger(__name__)

class RateLimiter:
    def __init__(self, settings_repo: SettingsRepository):
        self._settings_repo = settings_repo
        self._operation_counts: Dict[str, List[datetime]] = {}
        self._last_operation_time: Dict[str, datetime] = {}

    async def can_execute(self, job: JobItem) -> bool:
        """Check if a job can be executed based on rate limits"""
        try:
            settings = await self._settings_repo.get_account_settings(job.account_id)
            
            operation_type = job.type
            current_time = datetime.utcnow()

            # 1. Check minimum delay between operations
            if not await self._check_operation_delay(operation_type, current_time, settings):
                return False

            # 2. Check hourly limit
            if not await self._check_hourly_limit(operation_type, current_time, settings):
                return False

            # 3. Check daily limit
            if not await self._check_daily_limit(operation_type, current_time, settings):
                return False

            # 4. Add random jitter if enabled
            if settings.random_jitter:
                await self._apply_jitter(settings)

            return True

        except Exception as e:
            logger.error(f"Error in rate limiter for job {job.id}: {e}")
            return False

    async def _check_operation_delay(
        self,
        operation_type: str,
        current_time: datetime,
        settings: Any
    ) -> bool:
        """Check if enough time has passed since last operation"""
        last_time = self._last_operation_time.get(operation_type)
        if last_time is None:
            return True

        delay = {
            "reply": settings.reply_delay,
            "dm": settings.dm_delay,
            "like": settings.like_delay
        }.get(operation_type, 30)  # default 30 seconds

        time_passed = (current_time - last_time).total_seconds()
        return time_passed >= delay

    async def _check_hourly_limit(
        self,
        operation_type: str,
        current_time: datetime,
        settings: Any
    ) -> bool:
        """Check if hourly limit is reached"""
        if operation_type not in self._operation_counts:
            self._operation_counts[operation_type] = []

        # Remove operations older than 1 hour
        self._operation_counts[operation_type] = [
            t for t in self._operation_counts[operation_type]
            if (current_time - t) <= timedelta(hours=1)
        ]

        hourly_count = len(self._operation_counts[operation_type])
        return hourly_count < settings.hourly_limit

    async def _check_daily_limit(
        self,
        operation_type: str,
        current_time: datetime,
        settings: Any
    ) -> bool:
        """Check if daily limit is reached"""
        if operation_type not in self._operation_counts:
            self._operation_counts[operation_type] = []

        # Remove operations older than 24 hours
        self._operation_counts[operation_type] = [
            t for t in self._operation_counts[operation_type]
            if (current_time - t) <= timedelta(days=1)
        ]

        daily_count = len(self._operation_counts[operation_type])
        return daily_count < settings.daily_limit

    async def _apply_jitter(self, settings: Any):
        """Apply random delay for human-like behavior"""
        if settings.random_jitter:
            import random
            jitter = random.uniform(settings.jitter_min, settings.jitter_max)
            await asyncio.sleep(jitter)

    def record_operation(self, operation_type: str):
        """Record that an operation was performed"""
        current_time = datetime.utcnow()
        
        if operation_type not in self._operation_counts:
            self._operation_counts[operation_type] = []
        
        self._operation_counts[operation_type].append(current_time)
        self._last_operation_time[operation_type] = current_time
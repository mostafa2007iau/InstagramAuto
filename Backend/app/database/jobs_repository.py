"""
Persian:
    ?????????? ???? ?????? ????? ?? ???????.

English:
    Repository for managing jobs in the database.
"""

from datetime import datetime
from typing import List, Optional
from sqlalchemy import select, and_
from app.database.base import BaseRepository
from app.schemas.job_schema import JobItem, JobStatus
from app.models.job_model import Job

class JobsRepository(BaseRepository):
    async def create_job(self, job: JobItem) -> str:
        """Create a new job"""
        db_job = Job(
            type=job.type,
            status=job.status,
            rule_id=job.rule_id,
            media_id=job.media_id,
            target_user_id=job.target_user_id,
            comment_id=job.comment_id,
            payload=job.payload,
            account_id=job.account_id,
            session_blob=job.session_blob
        )
        self.db.add(db_job)
        await self.db.commit()
        return db_job.id

    async def get_job(self, job_id: str) -> Optional[JobItem]:
        """Get a job by ID"""
        job = await self.db.get(Job, job_id)
        if not job:
            return None
        return self._to_schema(job)

    async def update_job(self, job: JobItem):
        """Update a job"""
        db_job = await self.db.get(Job, job.id)
        if not db_job:
            return
        
        db_job.status = job.status
        db_job.completed_at = job.completed_at
        db_job.error = job.error
        db_job.attempts = job.attempts
        db_job.updated_at = datetime.utcnow()
        
        await self.db.commit()

    async def get_pending_jobs(self, limit: int = 100) -> List[JobItem]:
        """Get pending jobs"""
        query = select(Job).where(
            and_(
                Job.status == JobStatus.PENDING,
                Job.attempts < 3
            )
        ).order_by(Job.created_at).limit(limit)
        
        result = await self.db.execute(query)
        jobs = result.scalars().all()
        return [self._to_schema(job) for job in jobs]

    def _to_schema(self, job: Job) -> JobItem:
        """Convert DB model to schema"""
        return JobItem(
            id=job.id,
            type=job.type,
            status=job.status,
            rule_id=job.rule_id,
            media_id=job.media_id,
            target_user_id=job.target_user_id,
            comment_id=job.comment_id,
            payload=job.payload,
            created_at=job.created_at,
            updated_at=job.updated_at,
            completed_at=job.completed_at,
            error=job.error,
            attempts=job.attempts,
            account_id=job.account_id,
            session_blob=job.session_blob
        )
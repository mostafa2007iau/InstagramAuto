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
    def __init__(self, db):
        super().__init__(db)

    async def create_job(self, job: JobItem) -> str:
        """Create a new job"""
        db_job = Job(
            job_id=job.id,
            job_type=job.type,
            status=job.status,
            rule_id=getattr(job, 'rule_id', None),
            media_id=getattr(job, 'media_id', None),
            target_user_id=getattr(job, 'target_user_id', None),
            comment_id=getattr(job, 'comment_id', None),
            payload=job.payload,
            account_id=getattr(job, 'account_id', None),
            session_blob=getattr(job, 'session_blob', None),
            attempts=getattr(job, 'attempts', 0),
            last_error=getattr(job, 'error', None),
            created_at=getattr(job, 'created_at', datetime.utcnow()),
            updated_at=getattr(job, 'updated_at', datetime.utcnow()),
        )
        self.db.add(db_job)
        await self.db.commit()
        return db_job.job_id

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
        db_job.updated_at = datetime.utcnow()
        db_job.last_error = getattr(job, 'error', None)
        db_job.attempts = getattr(job, 'attempts', 0)
        db_job.completed_at = getattr(job, 'completed_at', None)
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
            id=job.job_id,
            type=job.job_type,
            status=job.status,
            rule_id=getattr(job, 'rule_id', None),
            media_id=getattr(job, 'media_id', None),
            target_user_id=getattr(job, 'target_user_id', None),
            comment_id=getattr(job, 'comment_id', None),
            payload=job.payload,
            created_at=job.created_at,
            updated_at=job.updated_at,
            completed_at=getattr(job, 'completed_at', None),
            error=job.last_error,
            attempts=getattr(job, 'attempts', 0),
            account_id=getattr(job, 'account_id', None),
            session_blob=getattr(job, 'session_blob', None)
        )
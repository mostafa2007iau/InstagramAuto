"""
Persian:
    صف ارسال (RQ) به‌روزرسانی‌شده:
    - ثبت job در DB (Job model) برای idempotency (قبلا اضافه شده)
    - enqueue به RQ
    - در صورت تکرار بیش از حد یا failure متوالی job به DLQ (جدول یا prefix) منتقل می‌شود
    - متریک‌ها را افزایش می‌دهد و Queue length را گزارش می‌کند (best-effort)

English:
    Updated sender queue (RQ):
    - records Job in DB for idempotency
    - enqueues job to RQ
    - moves repeatedly failing jobs to DLQ (simple implementation)
    - updates telemetry metrics and reports queue length
"""

from rq import Queue
from redis import Redis
from typing import Dict, Any, Optional
import os
import json
from app.services.telemetry_service import incr, gauge_set
from app.db import engine
from sqlmodel import Session, select
from app.models.job_model import Job
import uuid
import time

REDIS_URL = os.getenv("REDIS_URL", "redis://localhost:6379")
redis_conn = Redis.from_url(REDIS_URL)
queue = Queue("sender", connection=redis_conn)

# DLQ settings
DLQ_PREFIX = "dlq:sender"
MAX_ATTEMPTS = int(os.getenv("JOB_MAX_ATTEMPTS", "5"))

def _push_to_dlq(job_record: Job):
    key = f"{DLQ_PREFIX}:{job_record.job_id or job_record.id}"
    payload = {
        "job_id": job_record.job_id,
        "idempotency_key": job_record.idempotency_key,
        "job_type": job_record.job_type,
        "payload": job_record.payload,
        "status": job_record.status,
        "attempts": job_record.attempts,
        "last_error": job_record.last_error,
        "timestamp": int(time.time())
    }
    # store as JSON string with TTL (e.g., 30 days)
    redis_conn.set(key, json.dumps(payload), ex=60*60*24*30)

def enqueue_send(action: Dict[str, Any], idempotency_key: Optional[str] = None) -> Dict[str, Any]:
    with Session(engine) as db:
        if idempotency_key:
            stmt = select(Job).where(Job.idempotency_key == idempotency_key)
            existing = db.exec(stmt).first()
            if existing:
                return {"ok": True, "job_id": existing.job_id, "reused": True, "status": existing.status}
        # create Job record
        job_uuid = str(uuid.uuid4())
        job = Job(job_id=job_uuid, idempotency_key=idempotency_key, job_type="send", payload=json.dumps(action), status="queued", attempts=0)
        db.add(job)
        db.commit()
        db.refresh(job)
        # enqueue actual worker function
        rq_job = queue.enqueue("app.services.sender_worker.process_send", action, job_id=job_uuid, retry=MAX_ATTEMPTS)
        job.job_id = rq_job.get_id() if rq_job.get_id() else job_uuid
        db.add(job)
        db.commit()
        incr("jobs_enqueued")
        # best-effort queue length (approx)
        try:
            qlen = queue.count  # may raise depending on RQ version
            gauge_set("queue_length", qlen)
        except Exception:
            pass
        return {"ok": True, "job_id": job.job_id, "reused": False}

def handle_worker_failure(job_id: str, last_error: str):
    """
    Persian:
        هنگام شکست در worker این تابع فراخوانی شود تا وضعیت job به‌روزرسانی شود
        و اگر attempts>=MAX_ATTEMPTS شود، job به DLQ منتقل شود.

    English:
        Must be called when a worker records a failure. Updates Job in DB and moves to DLQ
        if attempts exceed MAX_ATTEMPTS.
    """
    with Session(engine) as db:
        job = db.exec(select(Job).where(Job.job_id == job_id)).first()
        if not job:
            return
        job.attempts = (job.attempts or 0) + 1
        job.last_error = last_error
        job.status = "failed" if job.attempts < MAX_ATTEMPTS else "dead"
        db.add(job)
        db.commit()
        if job.attempts >= MAX_ATTEMPTS:
            _push_to_dlq(job)
            # optional: remove job from active queue or mark dead
            job.status = "dead"
            db.add(job)
            db.commit()
        incr("jobs_failed")

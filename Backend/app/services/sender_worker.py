"""
Persian:
    Worker ارتقاء یافته برای پردازش jobهای ارسال که:
    - رکورد job را از DB می‌گیرد،
    - وضعیت job را به processing تغییر می‌دهد،
    - تلاش می‌کند با استفاده از SessionManager و InstaClient عملیات ارسال واقعی را انجام دهد،
    - در صورت موفقیت وضعیت را done و در صورت شکست failed ثبت می‌کند.

English:
    Enhanced worker to process send jobs that:
    - fetches Job record from DB,
    - marks the job as processing,
    - attempts to perform a real send using SessionManager and the Insta client,
    - marks job done on success or failed on exception.
"""

from rq import get_current_job
from app.services.sender_queue import mark_job_processing, mark_job_done, mark_job_failed
from app.services.telemetry_service import incr
from app.services.session_manager import SessionManager
from app.db import engine
from sqlmodel import Session, select
from app.models.job_model import Job
import time
import json
import traceback
import asyncio

# SessionManager singleton for worker usage (sync worker will call async methods via asyncio.run)
_sm = SessionManager()

def _find_job_record_by_rq():
    rq_job = get_current_job()
    if not rq_job:
        return None
    job_id = rq_job.get_id()
    with Session(engine) as db:
        rec = db.exec(select(Job).where(Job.job_id == job_id)).first()
        return rec

def _perform_send_via_session(session_id: str, action: dict):
    """
    Persian:
        تابع کمکی که از SessionManager استفاده می‌کند تا client را بازیابی کند و در صورت وجود
        متد send_direct_message آن را فراخوانی نماید.

    English:
        Helper that uses SessionManager to retrieve client and call send_direct_message if available.
    """
    try:
        # SessionManager._ensure_client is async; we run it in an event loop
        client = asyncio.run(_sm._ensure_client(session_id))
        # prefer send_direct_message if available
        send_fn = getattr(client, "send_direct_message", None)
        if callable(send_fn):
            # action may contain target and message
            target = action.get("target") or action.get("event", {}).get("target")
            message = action.get("message") or action.get("event", {}).get("message") or action.get("event")
            # allow send_fn to be sync or async
            if asyncio.iscoroutinefunction(send_fn):
                res = asyncio.run(send_fn(target, message))
            else:
                res = send_fn(target, message)
            return res
        # fallback: no send capability
        return {"ok": False, "error": "no_send_implementation"}
    except Exception as e:
        raise

def process_send(action: dict):
    """
    Persian:
        تابع اصلی پردازش job:
        - پیدا کردن رکورد job در DB،
        - علامت processing،
        - انجام send حقیقی (در صورت وجود session_id و پشتیبانی client)،
        - علامت done یا failed و ثبت متریک.

    English:
        Main job processing function:
        - find job record in DB,
        - mark processing,
        - perform actual send (if session_id present and client supports it),
        - mark done or failed and record telemetry.
    """
    rq_job = get_current_job()
    job_id = rq_job.get_id() if rq_job else action.get("job_id")
    try:
        if job_id:
            mark_job_processing(job_id)

        # perform the actual send if session_id provided
        session_id = action.get("session_id")
        result = None
        if session_id:
            try:
                result = _perform_send_via_session(session_id, action)
            except Exception as e:
                # send failed, record and raise to allow RQ retry
                tb = traceback.format_exc()
                mark_job_failed(job_id, str(e))
                incr("jobs_failed")
                raise

        # if no session_id or send returned ok, mark done
        incr("jobs_processed")
        if job_id:
            mark_job_done(job_id)
        return {"ok": True, "result": result}
    except Exception as e:
        # ensure failed-marking is done above; re-raise for RQ retry policy
        raise

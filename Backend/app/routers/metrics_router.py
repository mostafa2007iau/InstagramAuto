"""
Persian:
    endpoint ساده برای متریک‌های Prometheus با استفاده از prometheus_client.

English:
    Simple Prometheus metrics endpoint using prometheus_client.
"""

from fastapi import APIRouter, Response
from prometheus_client import generate_latest, CONTENT_TYPE_LATEST, Counter, CollectorRegistry

router = APIRouter(prefix="/metrics", tags=["metrics"])

# نمونه متریک محلی؛ در سرویس‌های دیگر هم از همین Counterها استفاده می‌شود.
JOB_ENQUEUED = Counter("insta_jobs_enqueued_total", "Jobs enqueued")
JOB_PROCESSED = Counter("insta_jobs_processed_total", "Jobs processed")

@router.get("", include_in_schema=False)
def metrics():
    data = generate_latest()
    return Response(content=data, media_type=CONTENT_TYPE_LATEST)

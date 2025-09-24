# app/services/telemetry_service.py

"""
Persian:
    Telemetry service ساده: wrapper برای Prometheus counters/gauges
    و یک helper برای tracing پایه.
    این سرویس thread-safe و process-safe برای استفاده در worker و وب است.

English:
    Lightweight telemetry service: wrapper around Prometheus counters/gauges
    and a basic tracing helper.
    Designed to be safe for use in web workers and background workers.
"""

import os
import time
from typing import Optional, Dict, Any, Tuple

from prometheus_client import (
    Counter,
    Gauge,
    Histogram,
    CollectorRegistry,
    generate_latest,
    CONTENT_TYPE_LATEST,
    multiprocess,
)

# یک رجیستری جدا برای پشتیبانی multiprocess (مثل Gunicorn)
REGISTRY = CollectorRegistry()
if os.getenv("PROMETHEUS_MULTIPROC_DIR"):
    multiprocess.MultiProcessCollector(REGISTRY)

# Counters
JOB_ENQUEUED    = Counter(
    "insta_jobs_enqueued_total",
    "Total number of jobs enqueued",
    registry=REGISTRY,
)
JOB_PROCESSED   = Counter(
    "insta_jobs_processed_total",
    "Total number of jobs processed successfully",
    registry=REGISTRY,
)
JOB_FAILED      = Counter(
    "insta_jobs_failed_total",
    "Total number of jobs failed",
    registry=REGISTRY,
)
PROBE_CALLS     = Counter(
    "insta_probe_calls_total",
    "Total number of media probe attempts",
    registry=REGISTRY,
)
LOGIN_ATTEMPTS  = Counter(
    "insta_login_attempts_total",
    "Total number of login attempts",
    registry=REGISTRY,
)
LOGIN_ERRORS    = Counter(
    "insta_login_errors_total",
    "Total number of login errors",
    registry=REGISTRY,
)

# Histogram
REQUEST_LATENCY = Histogram(
    "insta_request_latency_seconds",
    "Request latency seconds",
    registry=REGISTRY,
)

# Gauges
QUEUE_LENGTH    = Gauge(
    "insta_queue_length",
    "Approximate length of RQ queue (best-effort)",
    registry=REGISTRY,
)
ACTIVE_CLIENTS  = Gauge(
    "insta_active_clients",
    "Number of runtime instantiated clients",
    registry=REGISTRY,
)


def incr(metric_name: str, amount: int = 1) -> None:
    """
    Persian:
        افزایشی برای متریک‌های تعریف‌شده با نام.
        نام‌ها باید صریحاً در این ماژول تعریف شده باشند.

    English:
        Increment helper for named metrics defined in this module.
    """
    if metric_name == "jobs_enqueued":
        JOB_ENQUEUED.inc(amount)
    elif metric_name == "jobs_processed":
        JOB_PROCESSED.inc(amount)
    elif metric_name == "jobs_failed":
        JOB_FAILED.inc(amount)
    elif metric_name == "probe_calls":
        PROBE_CALLS.inc(amount)
    elif metric_name == "login_attempts":
        LOGIN_ATTEMPTS.inc(amount)
    elif metric_name == "login_errors":
        LOGIN_ERRORS.inc(amount)


def gauge_set(metric_name: str, value: int) -> None:
    """
    Persian:
        تنظیم مقدار Gauge بر اساس نام.

    English:
        Set a gauge metric by name.
    """
    if metric_name == "queue_length":
        QUEUE_LENGTH.set(value)
    elif metric_name == "active_clients":
        ACTIVE_CLIENTS.set(value)


def observe_latency(seconds: float) -> None:
    """
    Persian:
        ثبت زمان تأخیر درخواست در Histogram.

    English:
        Observe request latency in the histogram.
    """
    REQUEST_LATENCY.observe(seconds)


def get_metrics() -> Tuple[bytes, str]:
    """
    Persian:
        تولید payload متریک‌ها در فرمت Prometheus
        برای endpoint /metrics

    English:
        Return the latest metrics in Prometheus exposition format.

    Returns:
        Tuple[bytes, str]: (payload, content_type)
    """
    payload = generate_latest(REGISTRY)
    return payload, CONTENT_TYPE_LATEST

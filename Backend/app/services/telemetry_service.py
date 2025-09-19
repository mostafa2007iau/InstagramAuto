"""
Persian:
    Telemetry service ساده: wrapper برای Prometheus counters/gauges و یک helper برای tracing پایه.
    این سرویس thread-safe و process-safe برای استفاده در worker و وب است.

English:
    Lightweight telemetry service: wrapper around Prometheus counters/gauges and a basic tracing helper.
    Designed to be safe for use in web workers and background workers.
"""

from prometheus_client import Counter, Gauge, Histogram, generate_latest, CONTENT_TYPE_LATEST
from prometheus_client import CollectorRegistry
from prometheus_client import multiprocess
import os
import time
from typing import Optional, Dict

# Use a single global registry. For multiprocess Gunicorn, prometheus_client multiprocess support must be configured.
REGISTRY = CollectorRegistry()

# Common metrics (names chosen to be explicit)
JOB_ENQUEUED = Counter("insta_jobs_enqueued_total", "Total number of jobs enqueued", registry=REGISTRY)
JOB_PROCESSED = Counter("insta_jobs_processed_total", "Total number of jobs processed successfully", registry=REGISTRY)
JOB_FAILED = Counter("insta_jobs_failed_total", "Total number of jobs failed", registry=REGISTRY)
PROBE_CALLS = Counter("insta_probe_calls_total", "Total number of media probe attempts", registry=REGISTRY)
LOGIN_ATTEMPTS = Counter("insta_login_attempts_total", "Total number of login attempts", registry=REGISTRY)
LOGIN_ERRORS = Counter("insta_login_errors_total", "Total number of login errors", registry=REGISTRY)

REQUEST_LATENCY = Histogram("insta_request_latency_seconds", "Request latency seconds", registry=REGISTRY)

# Gauges
QUEUE_LENGTH = Gauge("insta_queue_length", "Approximate length of RQ queue (best-effort)", registry=REGISTRY)
ACTIVE_CLIENTS = Gauge("insta_active_clients", "Number of runtime instantiated clients", registry=REGISTRY)

def incr(metric_name: str, amount: int = 1):
    """
    Persian:
        افزایشی برای متریک‌های تعریف‌شده با نام. نام‌ها باید صریحاً در این ماژول تعریف شده باشند.

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
    else:
        # unknown metric: ignore silently
        return

def gauge_set(metric_name: str, value: int):
    if metric_name == "queue_length":
        QUEUE_LENGTH.set(value)
    elif metric_name == "active_clients":
        ACTIVE_CLIENTS.set(value)

def observe_latency(seconds: float):
    REQUEST_LATENCY.observe(seconds)

def metrics_response():
    """
    Persian:
        تولید payload متریک‌ها در فرمت Prometheus برای endpoint /metrics

    English:
        Generate metrics payload in Prometheus exposition format for /metrics endpoint.
    """
    data = generate_latest(REGISTRY)
    return data, CONTENT_TYPE_LATEST

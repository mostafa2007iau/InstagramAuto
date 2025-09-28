"""
Persian:
    Entrypoint FastAPI اصلی. این نسخه:
    - fastapi-limiter را در startup مقداردهی می‌کند (Redis-based rate limiter)
    - endpoint /metrics را که از telemetry_service می‌گیرد ثبت می‌کند
    - routerهای اصلی را include می‌کند

English:
    FastAPI entrypoint. This version:
    - initializes fastapi-limiter (Redis) on startup
    - exposes /metrics using telemetry_service
    - includes main routers
"""

import os
from fastapi import FastAPI, Response
from app.routers import (
    accounts_router,
    medias_router,
    rules_router,
    logs_router,
    challenge_router,
    challenge_ws_router,
    metrics_router,
    health_router,
    inbound_router,
    jobs_router,
    settings_router,
    stories_router,
)
from app.middleware.verbosity_middleware import VerbosityMiddleware, default_rate_limit, strict_rate_limit
from app.db import init_db
from app.services import telemetry_service
from fastapi_limiter import FastAPILimiter
import redis.asyncio as redis  # redis-py async client

app = FastAPI(
    title="InstaAutomation Backend",
    description="Backend for Insta automation with i18n, per-session proxy and pagination",
    version="v1",
)

# ✅ اضافه کردن middleware سفارشی
app.add_middleware(VerbosityMiddleware)


@app.on_event("startup")
async def startup_event():
    # initialize DB and other startup tasks
    init_db()

    # initialize Redis and fastapi-limiter
    redis_url = os.getenv("REDIS_URL", "redis://localhost:6379/0")
    try:
        client = redis.Redis.from_url(
            redis_url,
            encoding="utf-8",
            decode_responses=True,
        )
        await FastAPILimiter.init(client)
    except Exception:
        # if limiter init fails, continue without rate-limiting
        pass


# Health
@app.get("/health", summary="Health check / بررسی سلامت")
async def health():
    return {"status": "ok"}


# Metrics endpoint
@app.get("/metrics", include_in_schema=False)
async def metrics():
    data, content_type = telemetry_service.metrics_response()
    return Response(content=data, media_type=content_type)


# Include routers
app.include_router(accounts_router.router)
app.include_router(medias_router.router)
app.include_router(rules_router.router)
app.include_router(logs_router.router)
app.include_router(challenge_router.router)
app.include_router(challenge_ws_router.router)
app.include_router(jobs_router.router)
app.include_router(settings_router.router)
app.include_router(stories_router.router)

# metrics_router is optional (already have /metrics) but included if exists
try:
    app.include_router(metrics_router.router)
except Exception:
    pass

app.include_router(health_router.router)
app.include_router(inbound_router.router)

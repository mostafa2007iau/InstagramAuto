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
import asyncio
from fastapi import FastAPI, Response
from contextlib import asynccontextmanager
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
from app.db import init_db, engine
from app.services import telemetry_service
from app.services.job_processor import JobProcessor
from app.services.rate_limiter import RateLimiter
from app.database.jobs_repository import JobsRepository
from app.database.rules_repository import RulesRepository
from app.database.settings_repository import SettingsRepository
from app.services.insta_client_factory import InstaClientFactory
from app.services.reply_history import ReplyHistoryService
from fastapi_limiter import FastAPILimiter
import redis.asyncio as redis  # redis-py async client
from sqlmodel import Session

# Remove lifespan usage; manage job processor in startup/shutdown events

# Create a single DB session to reuse (simple approach for this app)
_db_session = Session(engine)

# Create repositories
jobs_repo = JobsRepository(db=_db_session)
rules_repo = RulesRepository(db=_db_session)
settings_repo = SettingsRepository(db=_db_session)

# Create services
rate_limiter = RateLimiter(settings_repo)
client_factory = InstaClientFactory()
reply_history = None
job_processor = None

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

    # Skip initializing fastapi-limiter to avoid compatibility issues with redis client
    # If you need rate limiting, configure with a supported redis client or install fastapi-limiter compatible deps.

    # Create reply history and job processor after DB init
    global reply_history, job_processor
    reply_history = ReplyHistoryService()
    job_processor = JobProcessor(
        client_factory=client_factory,
        jobs_repo=jobs_repo,
        rules_repo=rules_repo,
        rate_limiter=rate_limiter,
        reply_history=reply_history
    )
    # start background job processor
    asyncio.create_task(job_processor.start())


@app.on_event("shutdown")
async def shutdown_event():
    # Stop background job processor
    global job_processor
    if job_processor:
        await job_processor.stop()


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

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)

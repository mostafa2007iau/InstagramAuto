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
from app.routers import accounts_router, medias_router, rules_router, logs_router, challenge_router, challenge_ws_router, metrics_router, health_router, inbound_router
from app.middleware.verbosity_middleware import VerbosityMiddleware
from app.db import init_db
from app.services import telemetry_service
from fastapi_limiter import FastAPILimiter
import aioredis

app = FastAPI(
    title="InstaAutomation Backend",
    description="Backend for Insta automation with i18n, per-session proxy and pagination",
    version="v1"
)

# Middleware
app.add_middleware(VerbosityMiddleware)

@app.on_event("startup")
async def startup_event():
    # initialize DB and other startup tasks
    init_db()

    # Initialize aioredis and fastapi-limiter
    redis_url = os.getenv("REDIS_URL", "redis://localhost:6379/0")
    try:
        redis = await aioredis.from_url(redis_url, encoding="utf-8", decode_responses=True)
        await FastAPILimiter.init(redis)
    except Exception:
        # if limiter init fails, continue but rate-limiting endpoints will not be protected
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
# metrics_router is optional (already have /metrics) but included if exists
try:
    app.include_router(metrics_router.router)
except Exception:
    pass
app.include_router(health_router.router)
app.include_router(inbound_router.router)

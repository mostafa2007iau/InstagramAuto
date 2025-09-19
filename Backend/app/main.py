"""
Persian:
    Entrypoint کامل FastAPI که routerها و middleware را ثبت می‌کند و DB را در startup مقداردهی می‌کند.
    شامل ثبت routerهای async جدید (accounts, medias, rules, logs, challenge, challenge-ws, metrics, health).

English:
    Complete FastAPI entrypoint registering routers and middleware and initializing DB on startup.
    Includes async routers (accounts, medias, rules, logs, challenge, challenge-ws, metrics, health).
"""

from fastapi import FastAPI
from app.routers import accounts_router, medias_router, rules_router, logs_router, challenge_router, challenge_ws_router, metrics_router, health_router, inbound_router
from app.middleware.verbosity_middleware import VerbosityMiddleware
from app.db import init_db

app = FastAPI(
    title="InstaAutomation Backend",
    description="Backend for Insta automation with i18n, per-session proxy and pagination"
)

# Middleware
app.add_middleware(VerbosityMiddleware)

@app.on_event("startup")
async def startup_event():
    # initialize DB and any other startup tasks
    init_db()

# Include routers
app.include_router(accounts_router.router)
app.include_router(medias_router.router)
app.include_router(rules_router.router)
app.include_router(logs_router.router)
app.include_router(challenge_router.router)
app.include_router(challenge_ws_router.router)
app.include_router(metrics_router.router)
app.include_router(health_router.router)
app.include_router(inbound_router.router)

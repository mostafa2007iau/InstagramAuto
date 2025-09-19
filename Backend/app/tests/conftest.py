"""
Persian:
    pytest fixtures پایه برای DB و Redis برای اجرای تست‌های integration محلی/CI.
    این fixtureها DB را در tmp_path قرار می‌دهند و Redis connection را برمی‌گردانند (نیازمند Redis واقعی یا service در CI).

English:
    Basic pytest fixtures for DB and Redis to run integration tests locally/CI.
    Uses tmp_path for a sqlite DB file and respects REDIS_URL env var.
"""

import os
import pytest
import asyncio
from app.db import init_db
from sqlalchemy import create_engine
from sqlalchemy_utils import database_exists, create_database

@pytest.fixture(scope="session", autouse=True)
def setup_env_tmp_db(tmp_path_factory):
    tmp_dir = tmp_path_factory.mktemp("data")
    db_file = tmp_dir / "test.db"
    os.environ["DATABASE_URL"] = f"sqlite:///{db_file}"
    # ensure DB created by init_db call in tests
    init_db()
    yield
    # teardown: remove file handled by pytest tmpdir cleanup

@pytest.fixture(scope="session")
def redis_url():
    return os.environ.get("REDIS_URL", "redis://localhost:6379/0")

@pytest.fixture
def redis_client(redis_url):
    import redis
    r = redis.from_url(redis_url)
    # flush before test to ensure clean slate (only for local/dev)
    try:
        r.flushdb()
    except Exception:
        pass
    yield r
    # optional: flush after
    try:
        r.flushdb()
    except Exception:
        pass

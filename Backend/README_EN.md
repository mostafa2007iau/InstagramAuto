# InstaAutomation Backend (EN)

Overview
- FastAPI backend for Instagram automation with i18n, per-session proxy, pagination and media probe.

Quickstart
1. Copy .env or set DATABASE_URL (default sqlite ./dev.db)
2. Build and run:
   docker-compose up --build
3. Open http://localhost:8000/api/health

Testing
- Run pre-commit checks:
  python scripts/precommit_check_summaries.py
- Run tests:
  pytest

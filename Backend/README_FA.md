# InstaAutomation Backend (FA)

معرفی
- بک‌اند FastAPI برای اتوماسیون اینستاگرام با پشتیبانی i18n، پروکسی به ازای سشن، صفحه‌بندی و پروب مدیا.

شروع سریع
1. متغیر محیطی DATABASE_URL را تنظیم کنید (پیش‌فرض sqlite ./dev.db)
2. اجرا:
   docker-compose up --build
3. بازدید: http://localhost:8000/api/health

تست
- اجرای چک‌های pre-commit:
  python scripts/precommit_check_summaries.py
- اجرای تست‌ها:
  pytest

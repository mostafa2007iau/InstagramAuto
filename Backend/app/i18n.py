"""
Persian:
    فایل i18n ساده پروژه — کاتالوگ پیام‌های محلی‌شده.
    این نسخه کلیدهای لازم برای خطاهای instagrapi و پیام‌های لاگ را اضافه می‌کند.

English:
    Simple i18n catalog for the project. This version includes keys needed for instagrapi error messages and logs.
"""

_CATALOG = {
    # --------------------
    # accounts_router.py
    # --------------------
    "session.not_found": {"en": "Session not found", "fa": "سشن پیدا نشد"},
    "session.create_failed": {"en": "Failed to create session", "fa": "ایجاد سشن ناموفق بود"},
    "unknown.error": {"en": "Unknown error occurred", "fa": "خطای ناشناخته رخ داد"},
    # --------------------
    # challenge_router.py
    # --------------------
    "challenge.not_found": {"en": "Challenge not found or expired", "fa": "چالش پیدا نشد یا منقضی شده است"},
    "challenge.resolve_failed": {"en": "Failed to resolve challenge", "fa": "حل چالش ناموفق بود"},
    # --------------------
    # medias_router.py
    # --------------------
    "media.not_found": {"en": "Media not found", "fa": "مدیا پیدا نشد"},
    "media.fetch_failed": {"en": "Failed to fetch medias", "fa": "دریافت مدیاها ناموفق بود"},
    "media.client_not_supported": {"en": "Client does not support media retrieval", "fa": "کلاینت از دریافت مدیا پشتیبانی نمی‌کند"},
    # --------------------
    # rules_router.py / inbound_router.py
    # --------------------
    "event.account_id_required": {"en": "account_id and session_id are required", "fa": "account_id و session_id الزامی است"},
    "event.evaluate_failed": {"en": "Failed to evaluate rules for event", "fa": "ارزیابی قواعد برای رویداد ناموفق بود"},
    # --------------------
    # logs_router.py
    # --------------------
    "log.fetch_failed": {"en": "Failed to fetch logs", "fa": "دریافت لاگ‌ها ناموفق بود"},
    # --------------------
    # insta_login_handler.py
    # --------------------
    "login.session_missing": {"en": "Session is missing or invalid", "fa": "سشن وجود ندارد یا نامعتبر است"},
    "login.no_client_login": {"en": "Client does not support login", "fa": "کلاینت از ورود پشتیبانی نمی‌کند"},
    "login.failed": {"en": "Login failed", "fa": "ورود ناموفق بود"},
    "login.challenge_required": {"en": "Challenge required", "fa": "نیاز به چالش است"},
    "login.two_factor_required": {"en": "Two-factor authentication required", "fa": "احراز هویت دو مرحله‌ای لازم است"},
    # --------------------
    # session_manager.py
    # --------------------
    "session.db_error": {"en": "Database error in session manager", "fa": "خطای پایگاه داده در مدیریت سشن"},
    "session.client_create_failed": {"en": "Failed to create Instagram client", "fa": "ایجاد کلاینت اینستاگرام ناموفق بود"},
    "session.persist_failed": {"en": "Failed to persist session", "fa": "ذخیره سشن ناموفق بود"},
    "session.export_failed": {"en": "Failed to export session", "fa": "خروجی سشن ناموفق بود"},
    "session.probe_failed": {"en": "Failed to probe medias", "fa": "پروب مدیاها ناموفق بود"},
    # --------------------
    # proxy_service.py
    # --------------------
    "proxy.test.success": {"en": "Proxy is working", "fa": "پروکسی سالم است"},
    "proxy.test.failure": {"en": "Proxy test failed: {reason}", "fa": "تست پروکسی ناموفق بود: {reason}"},
    # --------------------
    # challenge_store_redis.py / challenge_manager.py
    # --------------------
    "challenge.redis_error": {"en": "Redis error in challenge store", "fa": "خطای ردیس در ذخیره چالش"},
    "challenge.pubsub_error": {"en": "Pub/Sub error in challenge store", "fa": "خطای Pub/Sub در ذخیره چالش"},
    # --------------------
    # comment_monitor_service.py
    # --------------------
    "comment.monitor_error": {"en": "Error in comment monitor service", "fa": "خطا در سرویس مانیتور کامنت"},
    # --------------------
    # dm_service.py
    # --------------------
    "dm.send_failed": {"en": "Failed to send DM", "fa": "ارسال دایرکت ناموفق بود"},
    # --------------------
    # insta_client_factory.py
    # --------------------
    "client.not_installed": {"en": "instagrapi not installed", "fa": "کتابخانه instagrapi نصب نیست"},
    "client.no_session_dump": {"en": "No session dump method available", "fa": "متد ذخیره سشن وجود ندارد"},
    # --------------------
    # reply_service.py
    # --------------------
    "reply.send_failed": {"en": "Failed to send reply", "fa": "ارسال پاسخ ناموفق بود"},
    # --------------------
    # rules_engine.py / rule_engine.py
    # --------------------
    "rule.evaluate_error": {"en": "Error evaluating rule expression", "fa": "خطا در ارزیابی قاعده"},
    # --------------------
    # sender_queue.py / sender_worker.py
    # --------------------
    "job.enqueue_failed": {"en": "Failed to enqueue job", "fa": "صف کردن job ناموفق بود"},
    "job.dlq_push_failed": {"en": "Failed to push job to DLQ", "fa": "انتقال job به DLQ ناموفق بود"},
    "job.process_failed": {"en": "Job processing failed", "fa": "پردازش job ناموفق بود"},
    # --------------------
    # telemetry_service.py
    # --------------------
    "telemetry.error": {"en": "Telemetry service error", "fa": "خطا در سرویس تله‌متری"},
    # --------------------
    # سایر کلیدهای عمومی و سراسری
    # --------------------
    "health.ok": {"en": "Service healthy", "fa": "سرویس سالم است"},
    "probe.empty_medias": {"en": "Probe returned no medias for user {uid}", "fa": "پروب هیچ مدیایی برای کاربر {uid} برنگرداند"},
    "2fa.required": {"en": "Two-factor authentication required", "fa": "احراز هویت دو مرحله‌ای لازم است"},
    "challenge.required": {"en": "A challenge is required to continue", "fa": "برای ادامه نیاز به چالش است"},
    "login.bad_password": {"en": "Bad username or password", "fa": "نام کاربری یا کلمه عبور اشتباه است"},
    "login.required": {"en": "Login is required", "fa": "ورود لازم است"},
    "login.rate_limited": {"en": "Too many attempts, please try later", "fa": "تعداد تلاش‌ها زیاد است، لطفاً بعداً تلاش کنید"},
    "login.error": {"en": "Login failed: {reason}", "fa": "ورود ناموفق بود: {reason}"},
    "privacy.error": {"en": "Operation blocked due to privacy settings", "fa": "عملیات به دلیل تنظیمات حریم خصوصی مسدود شد"},
    "network.error": {"en": "Network or remote error occurred", "fa": "خطای شبکه یا سرور راه دور رخ داد"},
    "db.error": {"en": "Database error occurred", "fa": "خطای پایگاه داده رخ داد"},
    "login.blocked": {"en": "Login blocked by Instagram", "fa": "ورود توسط اینستاگرام مسدود شد"},
    "login.internal_error": {"en": "Internal error during login", "fa": "خطای داخلی هنگام ورود"},
    "proxy.invalid": {"en": "Invalid proxy", "fa": "پروکسی نامعتبر است"},
    "proxy.test_failed": {"en": "Proxy test failed", "fa": "تست پروکسی ناموفق بود"},
    "rule.invalid": {"en": "Invalid rule", "fa": "قانون نامعتبر است"},
    "comment.failed": {"en": "Failed to post comment", "fa": "ارسال کامنت ناموفق بود"},
}

def translate(key: str, locale: str = "en", **kwargs) -> str:
    entry = _CATALOG.get(key)
    if not entry:
        # fallback to key
        return key
    text = entry.get(locale) or entry.get("en") or key
    try:
        return text.format(**kwargs) if kwargs else text
    except Exception:
        return text

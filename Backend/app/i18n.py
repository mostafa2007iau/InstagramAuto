"""
Persian:
    فایل i18n ساده پروژه — کاتالوگ پیام‌های محلی‌شده.
    این نسخه کلیدهای لازم برای خطاهای instagrapi و پیام‌های لاگ را اضافه می‌کند.

English:
    Simple i18n catalog for the project. This version includes keys needed for instagrapi error messages and logs.
"""

_CATALOG = {
    # existing keys ...
    "health.ok": {"en": "Service healthy", "fa": "سرویس سالم است"},
    "probe.empty_medias": {"en": "Probe returned no medias for user {uid}", "fa": "پروب هیچ مدیایی برای کاربر {uid} برنگرداند"},
    # instagrapi/login keys
    "2fa.required": {"en": "Two-factor authentication required", "fa": "احراز هویت دو مرحله‌ای لازم است"},
    "challenge.required": {"en": "A challenge is required to continue", "fa": "برای ادامه نیاز به چالش است"},
    "login.bad_password": {"en": "Bad username or password", "fa": "نام کاربری یا کلمه عبور اشتباه است"},
    "login.required": {"en": "Login is required", "fa": "ورود لازم است"},
    "login.rate_limited": {"en": "Too many attempts, please try later", "fa": "تعداد تلاش‌ها زیاد است، لطفاً بعداً تلاش کنید"},
    "login.error": {"en": "Login failed: {reason}", "fa": "ورود ناموفق بود: {reason}"},
    "privacy.error": {"en": "Operation blocked due to privacy settings", "fa": "عملیات به دلیل تنظیمات حریم خصوصی مسدود شد"},
    "network.error": {"en": "Network or remote error occurred", "fa": "خطای شبکه یا سرور راه دور رخ داد"}
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

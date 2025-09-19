"""
Persian:
    نگاشت خطاها و استثناهای متداول instagrapi به کدهای API و پیام‌های محلی‌شده.
    این ماژول یک نقطه مرکزی برای ترجمه استثناها به پیام‌های کاربرپسند فراهم می‌کند.

English:
    Map common instagrapi exceptions to API error codes and localized messages.
    This module centralizes translation of exceptions into user-friendly API responses.
"""

from app.i18n import translate

def map_instagram_exception(exc, locale="en"):
    """
    Persian:
        Exception را گرفته و دیکشنری ساختاریافته شامل fields زیر برمی‌گرداند:
          - code: شناسه کوتاه خطا
          - message: پیام محلی‌شده برای نمایش به کاربر
          - details: (اختیاری) رشته توضیحات برای لاگ یا debugging

    English:
        Accept an exception instance and return a structured dict:
          - code: short error identifier
          - message: localized message for client display
          - details: (optional) debug string for logs
    """
    name = exc.__class__.__name__
    details = str(exc)

    # Known instagrapi exceptions (common names may vary across versions)
    if name in ("TwoFactorRequired", "TwoFactorAuthRequired"):
        return {"code": "two_factor_required", "message": translate("2fa.required", locale=locale), "details": details}
    if name in ("ChallengeRequired", "ChallengeRequiredError", "ClientCheckpointRequired"):
        return {"code": "challenge_required", "message": translate("challenge.required", locale=locale), "details": details}
    if name in ("BadPassword", "BadCredentials", "InvalidPassword"):
        return {"code": "bad_password", "message": translate("login.bad_password", locale=locale), "details": details}
    if name in ("LoginRequired", "LoginRequiredError"):
        return {"code": "login_required", "message": translate("login.required", locale=locale), "details": details}
    if name in ("ClientThrottledError", "RateLimitError"):
        return {"code": "rate_limited", "message": translate("login.rate_limited", locale=locale), "details": details}
    if name in ("PrivacyError",):
        return {"code": "privacy_error", "message": translate("privacy.error", locale=locale), "details": details}
    if name in ("ClientError", "ClientRequestError", "HTTPError"):
        return {"code": "network_error", "message": translate("network.error", locale=locale), "details": details}

    # Fallback generic mapping
    return {"code": "login_error", "message": translate("login.error", locale=locale, reason=details), "details": details}

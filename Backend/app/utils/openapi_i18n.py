"""
Persian:
    helper برای تولید summary/description دو زبانه که در decorator endpoint قابل استفاده باشد.

English:
    Helper to produce bilingual summary/description usable as parameters for FastAPI endpoints.
"""

def i18n_summary(en: str, fa: str) -> str:
    # store both; UI یا frontend می‌تواند description محلی‌شده را استخراج کند via OpenAPI extensions
    return f"EN: {en}\nFA: {fa}"

def i18n_description(en: str, fa: str) -> str:
    return f"EN: {en}\nFA: {fa}"

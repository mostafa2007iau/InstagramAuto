"""
Persian:
    ابزار کمک برای قالب‌بندی لاگ‌های مشتری (client) با سطوح verbosity.

English:
    Small helper to format client logs with verbosity levels.
"""

def format_log(message: str, verbosity, locale: str = "en"):
    # verbosity may be enum or simple flag; keep simple formatting
    if str(verbosity).upper() == "FULL":
        return {"full": message}
    if str(verbosity).upper() == "SUMMARY":
        return {"summary": message[:200]}
    return {"none": ""}

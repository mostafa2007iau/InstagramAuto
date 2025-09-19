"""
Persian:
    اسکیمای پایه برای پاسخ‌های pagination که در تمامی endpointهای لیست استفاده می‌شود.

English:
    Base pagination schema used by list endpoints.
"""

from typing import Generic, List, Optional, TypeVar
from pydantic import BaseModel
from pydantic.generics import GenericModel

T = TypeVar("T")

class PaginationMeta(BaseModel):
    """
    Persian:
        متادیتای پاسخ صفحه‌بندی: next_cursor و تعداد بازگشتی.

    English:
        Pagination metadata: next_cursor token and count_returned.
    """
    next_cursor: Optional[str] = None
    count_returned: int = 0
    estimated_total: Optional[int] = None

class PaginatedResponse(GenericModel, Generic[T]):
    """
    Persian:
        پاسخ عمومی صفحه‌بندی شده با آیتم‌ها و متادیتا.

    English:
        Generic paginated response containing items and metadata.
    """
    items: List[T]
    meta: PaginationMeta

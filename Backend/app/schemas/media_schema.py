"""
Persian:
    اسکیمایی ساده برای مدیا آیتم‌ها که توسط medias endpoint برگردانده می‌شوند.

English:
    Simple media item schema returned by medias endpoint.
"""

from pydantic import BaseModel, Field

class MediaItem(BaseModel):
    id: str = Field(..., description="Media unique id / شناسه مدیا")
    caption: str = Field(None, description="Caption or alt text / کپشن یا متن جایگزین")
    thumbnail_url: str = Field(None, description="Thumb URL if available / آدرس تصویر کوچک در صورت وجود")

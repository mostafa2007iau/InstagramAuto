"""
Persian:
    اسکیمای مدیا با پشتیبانی از تصویر پیش‌فرض.

English:
    Media schema with default thumbnail support.
"""

from pydantic import BaseModel, Field, validator, HttpUrl
from typing import Optional
from datetime import datetime

class MediaItem(BaseModel):
    """Media item / آیتم مدیا"""
    id: str = Field(..., description="Media ID / شناسه مدیا")
    caption: Optional[str] = Field(None, description="Caption text / متن کپشن")
    thumbnail_url: Optional[str] = Field(None, description="Thumbnail URL / آدرس تصویر بندانگشتی")
    media_type: Optional[str] = Field(None, description="Media type (POST/REEL/etc) / نوع مدیا")
    taken_at: Optional[datetime] = Field(None, description="Taken at timestamp / زمان ثبت")

    @validator('thumbnail_url', pre=True)
    def set_default_thumbnail(cls, v):
        if not v:
            # Default placeholder image
            return "https://via.placeholder.com/320x320.png?text=No+Preview"
        return v

    @validator('thumbnail_url')
    def validate_thumbnail_url(cls, v):
        if not v:
            return None
        try:
            HttpUrl(v)
            return v
        except:
            return "https://via.placeholder.com/320x320.png?text=Invalid+URL"

    class Config:
        json_encoders = {
            datetime: lambda v: v.isoformat()
        }

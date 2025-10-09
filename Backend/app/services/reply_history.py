"""
Persian:
    ????? ?????? ???????? ??????.
    ??????? ??????? ??????? ?? ???????? ? ??????? ?? ???? ????.

English:
    Duplicate reply prevention service.
    Maintains history of replies to comments and prevents duplicate responses.
"""

from datetime import datetime, timedelta
from typing import Optional, Dict, List
import redis
import json
import logging
from app.config import REDIS_URL

logger = logging.getLogger(__name__)

class ReplyHistoryService:
    def __init__(self, redis_url: str = REDIS_URL):
        self.redis = redis.from_url(redis_url)
        self.REPLY_HISTORY_KEY = "reply_history:{account_id}:{media_id}"
        self.HISTORY_TTL = 60 * 60 * 24 * 30  # 30 days

    async def has_replied(self, account_id: str, media_id: str, comment_id: str) -> bool:
        """Check if we've already replied to this comment"""
        key = self.REPLY_HISTORY_KEY.format(account_id=account_id, media_id=media_id)
        return bool(self.redis.sismember(key, comment_id))

    async def record_reply(self, account_id: str, media_id: str, comment_id: str, 
                         rule_id: str, reply_text: str):
        """Record that we've replied to this comment"""
        key = self.REPLY_HISTORY_KEY.format(account_id=account_id, media_id=media_id)
        
        # Store basic reply history
        self.redis.sadd(key, comment_id)
        self.redis.expire(key, self.HISTORY_TTL)

        # Store detailed reply info
        detail_key = f"{key}:detail:{comment_id}"
        detail = {
            "rule_id": rule_id,
            "reply_text": reply_text,
            "timestamp": datetime.utcnow().isoformat()
        }
        self.redis.set(detail_key, json.dumps(detail))
        self.redis.expire(detail_key, self.HISTORY_TTL)

    async def get_reply_history(self, account_id: str, media_id: str, 
                              limit: int = 100) -> List[Dict]:
        """Get history of replies for a media"""
        key = self.REPLY_HISTORY_KEY.format(account_id=account_id, media_id=media_id)
        comment_ids = self.redis.smembers(key)
        
        history = []
        for comment_id in comment_ids:
            detail_key = f"{key}:detail:{comment_id}"
            detail_json = self.redis.get(detail_key)
            if detail_json:
                detail = json.loads(detail_json)
                detail["comment_id"] = comment_id
                history.append(detail)

        return sorted(history, 
                     key=lambda x: x["timestamp"], 
                     reverse=True)[:limit]

    async def clear_history(self, account_id: str, media_id: str = None):
        """Clear reply history for an account or specific media"""
        if media_id:
            key = self.REPLY_HISTORY_KEY.format(account_id=account_id, media_id=media_id)
            self.redis.delete(key)
        else:
            pattern = self.REPLY_HISTORY_KEY.format(account_id=account_id, media_id="*")
            for key in self.redis.scan_iter(pattern):
                self.redis.delete(key)
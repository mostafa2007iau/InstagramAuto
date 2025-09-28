"""
Persian:
    ????? ?????????? ????? ? ????? ???? ?????? ? ??????.
English:
    Comment monitoring, auto-reply, and DM sending service.

Install:
    pip install instagrapi fastapi sqlalchemy apscheduler
"""

import asyncio
from typing import List, Optional
from app.models.comment_model import Comment
from app.models.rule_model import Rule
from app.models.session_model import Session
from app.services.session_manager import SessionManager
from app.services.rule_engine import RuleEngine
from app.services.reply_service import ReplyService
from app.services.dm_service import DMService
from app.db import get_db
from app.logging_utils import log_action

class CommentMonitorService:
    """
    Persian:
        ????? ?????????? ????? ? ????? ?????? ????????.
    English:
        Service for monitoring comments and applying reply/DM rules.
    """
    def __init__(self, session_manager: SessionManager, rule_engine: RuleEngine, reply_service: ReplyService, dm_service: DMService):
        self.session_manager = session_manager
        self.rule_engine = rule_engine
        self.reply_service = reply_service
        self.dm_service = dm_service
        self.running = False

    async def start(self):
        self.running = True
        while self.running:
            await self.check_new_comments()
            await asyncio.sleep(30)  # configurable interval

    async def stop(self):
        self.running = False

    async def check_new_comments(self):
        """
        Persian:
            ????? ????????? ???? ? ????? ??????.
        English:
            Check new comments and apply rules.
        """
        db = next(get_db())
        sessions: List[Session] = db.query(Session).all()
        for sess in sessions:
            client = await self.session_manager._ensure_client(sess.id)
            medias = await client.user_medias(sess.account_id, 10)
            for media in medias:
                comments = await client.media_comments(media.id, 20)
                for comment in comments:
                    if not self._is_processed(comment):
                        rule = self.rule_engine.match_rule(sess.account_id, media.id, comment.text)
                        if rule:
                            await self.reply_service.send_reply(client, media.id, comment, rule)
                            if rule.send_dm:
                                await self.dm_service.send_dm(client, comment.user_id, rule)
                        self._mark_processed(comment)

    def _is_processed(self, comment: Comment) -> bool:
        # TODO: Implement comment processed check (e.g., DB flag)
        return False

    def _mark_processed(self, comment: Comment):
        # TODO: Mark comment as processed in DB
        pass

"""
Persian:
    ???????? ????? ???? ????? ?????? ??????.
    ????:
    - ????? ??????? ????????? ????
    - ????? ?????? ??? ????????
    - ????? ???? ? DM ?? ????? ??????????
    - ??????? ?? ???????? ??????

English:
    Job processor for automatic rule execution.
    Includes:
    - Periodic check for new comments
    - Apply rules to comments
    - Send replies and DMs with rate limiting
    - Prevent duplicate replies
"""

import asyncio
from datetime import datetime, timedelta
import logging
from typing import List, Dict, Any, Optional
from app.services.insta_client_factory import InstaClientFactory
from app.services.reply_history import ReplyHistoryService
from app.schemas.job_schema import JobItem, JobStatus
from app.schemas.rule_schema import RuleOut
from app.services.rate_limiter import RateLimiter
from app.database.jobs_repository import JobsRepository
from app.database.rules_repository import RulesRepository

logger = logging.getLogger(__name__)

class JobProcessor:
    def __init__(
        self,
        client_factory: InstaClientFactory,
        jobs_repo: JobsRepository,
        rules_repo: RulesRepository,
        rate_limiter: RateLimiter,
        reply_history: ReplyHistoryService
    ):
        self._client_factory = client_factory
        self._jobs_repo = jobs_repo
        self._rules_repo = rules_repo
        self._rate_limiter = rate_limiter
        self._reply_history = reply_history
        self._running = False
        self._check_interval = 60  # seconds

    async def start(self):
        """Start the job processor"""
        self._running = True
        while self._running:
            try:
                await self._process_cycle()
            except Exception as e:
                logger.exception("Error in job processor cycle: %s", e)
            await asyncio.sleep(self._check_interval)

    async def stop(self):
        """Stop the job processor"""
        self._running = False

    async def _process_cycle(self):
        """Run one processing cycle"""
        # 1. Get active rules
        rules = await self._rules_repo.get_active_rules()
        for rule in rules:
            try:
                client = self._client_factory.create_new()
                await client.load_session_bytes(rule.session_blob)

                # 2. Get new comments
                comments = await client.get_new_comments(rule.media_id)
                
                for comment in comments:
                    # 3. Check if we've already replied
                    if await self._reply_history.has_replied(
                        rule.account_id, 
                        rule.media_id,
                        comment["id"]
                    ):
                        continue

                    # 4. Apply rule conditions
                    if self._evaluate_rule_condition(rule, comment):
                        # 5. Create jobs for matches
                        reply_text = rule.get_random_reply()
                        await self._create_reply_job(rule, comment, reply_text)
                        
                        # Record that we're going to reply
                        await self._reply_history.record_reply(
                            rule.account_id,
                            rule.media_id,
                            comment["id"],
                            rule.id,
                            reply_text
                        )

                        # Create DM job if enabled
                        if rule.send_dm:
                            await self._create_dm_job(rule, comment)

            except Exception as e:
                logger.error(f"Error processing rule {rule.id}: {e}", exc_info=True)

        # 6. Process pending jobs
        await self._process_pending_jobs()

    def _evaluate_rule_condition(self, rule: RuleOut, comment: Dict[str, Any]) -> bool:
        """Evaluate if a comment matches rule conditions"""
        try:
            # Basic implementation - extend based on your condition format
            if "text" not in rule.condition:
                return False
            
            condition_text = rule.condition["text"].lower()
            comment_text = comment.get("text", "").lower()
            
            return condition_text in comment_text
        except Exception as e:
            logger.error(f"Error evaluating rule {rule.id}: {e}")
            return False

    async def _create_reply_job(self, rule: RuleOut, comment: Dict[str, Any], reply_text: str):
        """Create a job for replying to a comment"""
        job = JobItem(
            type="reply",
            status=JobStatus.PENDING,
            rule_id=rule.id,
            media_id=rule.media_id,
            target_user_id=comment["user_id"],
            comment_id=comment["id"],
            payload={
                "reply_text": reply_text,
                "comment_text": comment.get("text", ""),
            },
            account_id=rule.account_id,
            session_blob=rule.session_blob
        )
        await self._jobs_repo.create_job(job)

    async def _create_dm_job(self, rule: RuleOut, comment: Dict[str, Any]):
        """Create a job for sending DM"""
        # Format DM template with comment info
        dm_text = rule.dm_template["text"].format(
            username=comment.get("username", ""),
            comment=comment.get("text", "")
        )

        job = JobItem(
            type="dm",
            status=JobStatus.PENDING,
            rule_id=rule.id,
            media_id=rule.media_id,
            target_user_id=comment["user_id"],
            payload={
                "message": {
                    "text": dm_text,
                    "media_url": rule.dm_template.get("media_url")
                },
                "comment_text": comment.get("text", ""),
            },
            account_id=rule.account_id,
            session_blob=rule.session_blob
        )
        await self._jobs_repo.create_job(job)

    async def _process_pending_jobs(self):
        """Process pending jobs with rate limiting"""
        jobs = await self._jobs_repo.get_pending_jobs()
        
        for job in jobs:
            try:
                # Check rate limits
                if not await self._rate_limiter.can_execute(job):
                    continue

                client = self._client_factory.create_new()
                await client.load_session_bytes(job.session_blob)

                if job.type == "reply":
                    await self._execute_reply_job(client, job)
                elif job.type == "dm":
                    await self._execute_dm_job(client, job)

                # Mark as completed
                job.status = JobStatus.COMPLETED
                job.completed_at = datetime.utcnow()
                await self._jobs_repo.update_job(job)

            except Exception as e:
                logger.error(f"Error processing job {job.id}: {e}")
                job.status = JobStatus.FAILED
                job.error = str(e)
                await self._jobs_repo.update_job(job)

    async def _execute_reply_job(self, client: Any, job: JobItem):
        """Execute a reply job"""
        await client.reply_to_comment(
            media_id=job.media_id,
            comment_id=job.comment_id,
            text=job.payload["reply_text"]
        )

    async def _execute_dm_job(self, client: Any, job: JobItem):
        """Execute a DM job"""
        message = job.payload["message"]
        await client.send_direct_message(
            user_id=job.target_user_id,
            message=message["text"],
            media_url=message.get("media_url")
        )
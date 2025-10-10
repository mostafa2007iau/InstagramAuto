"""
Sessions repository: provides async methods to create/get/update/delete session metadata using a sync SQLModel Session
Operations run in a thread via asyncio.to_thread to be safe in async context.
"""
from typing import Optional, Dict, Any
from datetime import datetime
import asyncio
from sqlmodel import select
from app.database.base import BaseRepository
from app.models.session_db_model import SessionDB
from app.schemas.session_schema import SessionOut

class SessionsRepository(BaseRepository):
    def __init__(self, db):
        super().__init__(db)

    async def create_session(self, account_id: str, proxy: Optional[str] = None, proxy_enabled: bool = False, locale: str = "en") -> SessionOut:
        def _sync():
            session = SessionDB(
                id=str(datetime.utcnow().timestamp()).replace('.', '') ,
                account_id=account_id,
                proxy=proxy,
                proxy_enabled=proxy_enabled,
                locale_preference=locale,
                created_at=datetime.utcnow(),
                updated_at=datetime.utcnow()
            )
            self.db.add(session)
            self.db.commit()
            self.db.refresh(session)
            return session

        db_model = await asyncio.to_thread(_sync)
        return SessionOut(
            id=db_model.id,
            account_id=db_model.account_id,
            proxy=db_model.proxy,
            proxy_enabled=db_model.proxy_enabled,
            last_media_check=db_model.last_media_check,
            locale_preference=db_model.locale_preference
        )

    async def get_session(self, session_id: str) -> Optional[SessionOut]:
        def _sync():
            return self.db.get(SessionDB, session_id)

        db_model = await asyncio.to_thread(_sync)
        if not db_model:
            return None
        return SessionOut(
            id=db_model.id,
            account_id=db_model.account_id,
            proxy=db_model.proxy,
            proxy_enabled=db_model.proxy_enabled,
            last_media_check=db_model.last_media_check,
            locale_preference=db_model.locale_preference,
            # Include session_blob attr for callers expecting it
        )

    async def update_session(self, session_id: str, **updates: Any) -> bool:
        def _sync():
            db_model = self.db.get(SessionDB, session_id)
            if not db_model:
                return False
            for k, v in updates.items():
                if hasattr(db_model, k):
                    setattr(db_model, k, v)
            db_model.updated_at = datetime.utcnow()
            self.db.commit()
            self.db.refresh(db_model)
            return True

        return await asyncio.to_thread(_sync)

    async def delete_session(self, session_id: str) -> bool:
        def _sync():
            db_model = self.db.get(SessionDB, session_id)
            if not db_model:
                return False
            self.db.delete(db_model)
            self.db.commit()
            return True

        return await asyncio.to_thread(_sync)

"""
Persian:
    ?????????? ?????? ???? ?????? CRUD ?????? ?? ???????.
English:
    Rules repository for managing CRUD operations on rules in the database.
"""

from datetime import datetime
from typing import List, Optional
from sqlalchemy import select
from app.database.base import BaseRepository
from app.models.rule_model import Rule
from app.schemas.rule_schema import RuleIn, RuleOut, RuleUpdate

class RulesRepository(BaseRepository):
    def __init__(self, db):
        super().__init__(db)

    async def create_rule(self, rule_in: RuleIn) -> RuleOut:
        rule = Rule(
            account_id=rule_in.account_id,
            name=rule_in.name,
            expression=rule_in.condition,
            enabled=rule_in.enabled,
            created_at=datetime.utcnow(),
            updated_at=datetime.utcnow()
        )
        self.db.add(rule)
        await self.db.commit()
        await self.db.refresh(rule)
        return self._to_schema(rule)

    async def get_rule(self, rule_id: int) -> Optional[RuleOut]:
        rule = await self.db.get(Rule, rule_id)
        if not rule:
            return None
        return self._to_schema(rule)

    async def update_rule(self, rule_id: int, update: RuleUpdate) -> Optional[RuleOut]:
        rule = await self.db.get(Rule, rule_id)
        if not rule:
            return None
        for field, value in update.dict(exclude_unset=True).items():
            if hasattr(rule, field):
                setattr(rule, field, value)
        rule.updated_at = datetime.utcnow()
        await self.db.commit()
        await self.db.refresh(rule)
        return self._to_schema(rule)

    async def delete_rule(self, rule_id: int) -> bool:
        rule = await self.db.get(Rule, rule_id)
        if not rule:
            return False
        await self.db.delete(rule)
        await self.db.commit()
        return True

    async def get_active_rules(self, account_id: Optional[str] = None) -> List[RuleOut]:
        stmt = select(Rule).where(Rule.enabled == True)
        if account_id:
            stmt = stmt.where(Rule.account_id == account_id)
        try:
            result = await self.db.execute(stmt)
        except Exception:
            # If underlying DB session is synchronous (Session.execute) call it synchronously
            result = self.db.execute(stmt)
        rules = result.scalars().all()
        return [self._to_schema(r) for r in rules]

    def _to_schema(self, rule: Rule) -> RuleOut:
        return RuleOut(
            id=str(rule.id),
            account_id=rule.account_id,
            name=rule.name,
            condition=rule.expression,
            media_id=getattr(rule, 'media_id', None),
            enabled=rule.enabled,
            replies=[],  # Extend if you store replies in DB
            send_dm=False,  # Extend if you store send_dm in DB
            dm_template=None,  # Extend if you store dm_template in DB
            attachments=[],  # Extend if you store attachments in DB
            created_at=rule.created_at,
            updated_at=rule.updated_at
        )

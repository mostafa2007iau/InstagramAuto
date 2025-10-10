"""
Persian:
    BaseRepository: ???? ???? ???? ?????????????? ???????.
English:
    BaseRepository: Base class for database repositories.
"""

from sqlmodel import Session


class BaseRepository:
    def __init__(self, db: Session):
        # If a generator or dependency is passed, try to extract session
        if hasattr(db, '__iter__') and not isinstance(db, Session):
            try:
                # assume db is a generator from get_session(); get first yielded session
                self.db = next(db)
            except Exception:
                self.db = None
        else:
            self.db = db

    def ensure_db(self):
        if self.db is None:
            raise RuntimeError('Database session not initialized for repository')

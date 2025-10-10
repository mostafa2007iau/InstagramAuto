"""
Persian:
    راه‌انداز اتصال دیتابیس با SQLModel و تولید session factory.

English:
    Database bootstrap using SQLModel and provide session factory.
"""

from sqlmodel import SQLModel, create_engine, Session
import os

DATABASE_URL = os.getenv("DATABASE_URL", "sqlite:///./dev.db")

# echo را بسته به نیاز لاگ فعال/غیرفعال کنید
engine = create_engine(DATABASE_URL, echo=True, connect_args={"check_same_thread": False} if DATABASE_URL.startswith("sqlite") else {})

def init_db():
    SQLModel.metadata.create_all(engine)

def get_session():
    with Session(engine) as session:
        yield session

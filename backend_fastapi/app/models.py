from sqlalchemy import Boolean, Integer, String
from sqlalchemy.orm import Mapped, mapped_column

from .db import Base


class User(Base):
    __tablename__ = "Users"

    id: Mapped[int] = mapped_column("Id", Integer, primary_key=True, index=True)
    username: Mapped[str] = mapped_column("Username", String(50), unique=True, index=True, nullable=False)
    password_hash: Mapped[str] = mapped_column("Password", String(255), nullable=False)
    full_name: Mapped[str] = mapped_column("FullName", String(100), nullable=False)
    phone: Mapped[str | None] = mapped_column("Phone", String(30), nullable=True)
    gender: Mapped[str | None] = mapped_column("Gender", String(10), nullable=True)
    avatar: Mapped[str | None] = mapped_column("Avatar", String, nullable=True)
    role: Mapped[str] = mapped_column("Role", String(20), nullable=False, default="user")
    is_locked: Mapped[bool] = mapped_column("IsLocked", Boolean, nullable=False, default=False)


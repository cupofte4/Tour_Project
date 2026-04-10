from enum import Enum

from pydantic import BaseModel, Field, field_validator


class Role(str, Enum):
    user = "user"
    manager = "manager"


class RegisterRequest(BaseModel):
    username: str = Field(min_length=3, max_length=50, pattern=r"^[a-zA-Z0-9_\.]+$")
    password: str = Field(min_length=1, max_length=128)
    full_name: str = Field(min_length=1, max_length=100)
    role: Role | str | None = Field(default=None)
    phone: str | None = Field(default=None, max_length=30)
    gender: str | None = Field(default=None, max_length=10)
    avatar: str | None = None

    @field_validator("password")
    @classmethod
    def password_not_empty(cls, value: str) -> str:
        if not value or not value.strip():
            raise ValueError("Password must not be empty")
        return value

    @field_validator("role", mode="before")
    @classmethod
    def normalize_role(cls, value):
        if value is None or (isinstance(value, str) and not value.strip()):
            return Role.user

        if isinstance(value, Role):
            return value

        if isinstance(value, str):
            raw = value.strip().lower()
            if raw in {"user", "tôi muốn trải nghiệm app", "toi muon trai nghiem app"}:
                return Role.user
            if raw in {"manager", "tôi muốn kinh doanh", "toi muon kinh doanh"}:
                return Role.manager

        raise ValueError("Invalid role")


class LoginRequest(BaseModel):
    username: str = Field(min_length=3, max_length=50)
    password: str = Field(min_length=1, max_length=128)

    @field_validator("password")
    @classmethod
    def login_password_not_empty(cls, value: str) -> str:
        if not value or not value.strip():
            raise ValueError("Password must not be empty")
        return value


class TokenResponse(BaseModel):
    access_token: str
    token_type: str = "bearer"


class UserPublic(BaseModel):
    id: int
    username: str
    full_name: str
    role: str


class AuthResponse(BaseModel):
    token: TokenResponse
    user: UserPublic

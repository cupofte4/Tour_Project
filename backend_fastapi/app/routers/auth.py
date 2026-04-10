from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy import select
from sqlalchemy.exc import IntegrityError
from sqlalchemy.orm import Session

from ..db import get_db
from ..models import User
from ..schemas import AuthResponse, LoginRequest, RegisterRequest, TokenResponse, UserPublic
from ..security import create_access_token, hash_password, verify_password

router = APIRouter(prefix="/auth", tags=["auth"])


@router.post("/register", response_model=AuthResponse, status_code=status.HTTP_201_CREATED)
def register(payload: RegisterRequest, db: Session = Depends(get_db)):
    user = User(
        username=payload.username,
        password_hash=hash_password(payload.password),
        full_name=payload.full_name,
        phone=payload.phone,
        gender=payload.gender,
        avatar=payload.avatar,
        role=payload.role.value if hasattr(payload.role, "value") else str(payload.role or "user"),
        is_locked=False,
    )
    db.add(user)
    try:
        db.commit()
    except IntegrityError:
        db.rollback()
        raise HTTPException(status_code=400, detail="Username already exists")

    db.refresh(user)
    token = create_access_token(subject=str(user.id), role=user.role)
    return AuthResponse(
        token=TokenResponse(access_token=token),
        user=UserPublic(id=user.id, username=user.username, full_name=user.full_name, role=user.role),
    )


@router.post("/login", response_model=AuthResponse)
def login(payload: LoginRequest, db: Session = Depends(get_db)):
    user = db.scalar(select(User).where(User.username == payload.username))
    if not user or not verify_password(payload.password, user.password_hash):
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Invalid username or password")
    if user.is_locked:
        raise HTTPException(status_code=status.HTTP_403_FORBIDDEN, detail="Account locked")

    token = create_access_token(subject=str(user.id), role=user.role)
    return AuthResponse(
        token=TokenResponse(access_token=token),
        user=UserPublic(id=user.id, username=user.username, full_name=user.full_name, role=user.role),
    )

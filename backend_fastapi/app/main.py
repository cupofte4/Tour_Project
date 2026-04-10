from fastapi import FastAPI

from .db import Base, engine
from .routers.auth import router as auth_router

app = FastAPI(title="Tour Guide API")

Base.metadata.create_all(bind=engine)

app.include_router(auth_router)


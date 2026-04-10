# FastAPI backend (Auth)

## Run (dev)

```powershell
cd backend_fastapi
python -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install -r requirements.txt
$env:DATABASE_URL="mysql+pymysql://user:pass@localhost:3306/tourdb"
$env:JWT_SECRET_KEY="change-me"
uvicorn app.main:app --reload --port 8000
```

## Endpoints

- `POST /auth/register`
- `POST /auth/login`


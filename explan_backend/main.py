from fastapi import FastAPI
from pydantic import BaseModel
from database_service import get_connection

app = FastAPI()

# ===== MODEL =====
class LoginRequest(BaseModel):
    email: str
    password: str

# ===== API LOGIN =====
@app.post("/login")
def login(data: LoginRequest):
    conn = get_connection()
    cursor = conn.cursor(dictionary=True)

    query = "SELECT * FROM users WHERE email=%s AND password=%s"
    cursor.execute(query, (data.email, data.password))
    user = cursor.fetchone()

    cursor.close()
    conn.close()

    if user:
        return {"success": True, "user": user}
    else:
        return {"success": False}

# ===== API GET LOCATIONS =====
@app.get("/locations")
def get_locations():
    conn = get_connection()
    cursor = conn.cursor(dictionary=True)

    cursor.execute("SELECT * FROM location")
    data = cursor.fetchall()

    cursor.close()
    conn.close()

    return data
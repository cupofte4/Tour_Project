from fastapi import FastAPI
from database_service import get_connection

app = FastAPI()

@app.get("/locations")
def get_locations():
    conn = get_connection()
    cursor = conn.cursor(dictionary=True)

    cursor.execute("SELECT * FROM location")
    data = cursor.fetchall()

    cursor.close()
    conn.close()

    return data
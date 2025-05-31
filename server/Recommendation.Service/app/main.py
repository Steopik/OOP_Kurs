from fastapi import FastAPI, Path
from app.config import Config
from app.services.progress_api import get_user_read_books
from app.services.recommendation_engine import recommend_books

app = FastAPI(
    title="Recommendation Service",
    version="1.0.0",
    description="Рекомендательный микросервис для книг"
)

@app.get("/user/{user_id}/books")
async def read_user_books(user_id: str = Path(..., description="User ID")):
    books = await get_user_read_books(user_id)
    return {"user_id": user_id, "books": books}

@app.get("/user/{user_id}/recommendations")
async def get_recommendations(user_id: str = Path(...)):
    read_books = await get_user_read_books(user_id)
    recommendations = await recommend_books(read_books)
    return {"user_id": user_id, "recommendations": recommendations}

if __name__ == "__main__":
    import uvicorn
    uvicorn.run("app.main:app", host=Config.HOST, port=Config.PORT, reload=True)

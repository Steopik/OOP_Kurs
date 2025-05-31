# app/routers/recommender.py

from fastapi import APIRouter, Path
from typing import List, Dict
from services.progress_api import get_user_read_books
from recommender.recommender import get_recommendations
import logging

logger = logging.getLogger(__name__)

router = APIRouter(prefix="/recommend", tags=["Recommendation"])

# Примерный список всех пользователей (в будущем можно заменить API-запросом)
ALL_USERS = [
    "e1f8e1a3-1234-4567-89ab-1234567890aa",
    "e1f8e1a3-2222-4567-89ab-1234567890bb",
    "e1f8e1a3-3333-4567-89ab-1234567890cc",
    # ... другие пользователи
]

@router.get("/user/{user_id}")
async def recommend_books(user_id: str = Path(..., description="User ID to get recommendations for")):
    user_books_map: Dict[str, List[str]] = {}

    # Собираем книги всех пользователей
    for uid in ALL_USERS:
        books = await get_user_read_books(uid)
        user_books_map[uid] = books

    # Генерируем рекомендации
    recommended = get_recommendations(user_id, user_books_map)

    return {
        "user_id": user_id,
        "recommended_books": recommended
    }

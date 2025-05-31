import httpx
from app.config import Config
from typing import List
import logging

logger = logging.getLogger(__name__)

async def get_user_read_books(user_id: str) -> List[str]:
    url = f"{Config.READING_PROGRESS_SERVICE_URL}/Progress/{user_id}"
    params = { "status": 2 }

    try:
        async with httpx.AsyncClient() as client:
            response = await client.get(url, params=params)
            response.raise_for_status()
            data = response.json()
            return [entry["bookId"] for entry in data if "bookId" in entry]
    except httpx.HTTPError as e:
        logger.error(f"Error fetching read books for user {user_id}: {e}")
        return []

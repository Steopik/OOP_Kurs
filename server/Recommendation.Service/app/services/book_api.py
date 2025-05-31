import httpx
from typing import List
from app.config import Config
from app.models.book import Book

async def get_all_books() -> List[Book]:
    async with httpx.AsyncClient() as client:
        response = await client.get(f"{Config.BOOK_SERVICE_URL}/Book")
        response.raise_for_status()
        books = response.json()
        return [Book(**book) for book in books]

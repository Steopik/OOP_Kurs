from pydantic import BaseModel
from uuid import UUID

class Book(BaseModel):
    id: UUID
    title: str
    author: str
    genre: str
    pages: int
    filePath: str

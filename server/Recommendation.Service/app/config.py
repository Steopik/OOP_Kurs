# app/config.py
from dotenv import load_dotenv
import os

load_dotenv()

class Config:
    HOST = os.getenv("HOST", "127.0.0.1")
    PORT = int(os.getenv("PORT", 8000))

    READING_PROGRESS_SERVICE_URL = os.getenv("READING_PROGRESS_SERVICE_URL")
    BOOK_SERVICE_URL = os.getenv("BOOK_SERVICE_URL")

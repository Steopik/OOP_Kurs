# app/config.py
import os
from dotenv import load_dotenv

load_dotenv()

class Config:
    SMTP_HOST = os.getenv("SMTP_HOST", "smtp.gmail.com")
    SMTP_PORT = int(os.getenv("SMTP_PORT", "587"))
    SMTP_USER = os.getenv("SMTP_USER")
    SMTP_PASSWORD = os.getenv("SMTP_PASSWORD")
    FROM_EMAIL = os.getenv("FROM_EMAIL", SMTP_USER)

    ADMIN_EMAIL = os.getenv("ADMIN_EMAIL")

    LOG_LEVEL_CONSOLE = os.getenv("LOG_LEVEL_CONSOLE", "INFO")
    LOG_LEVEL_FILE = os.getenv("LOG_LEVEL_FILE", "DEBUG")
    LOG_FILE_PATH = os.getenv("LOG_FILE_PATH", "logs/app.log")

    QUEUE_PROCESS_INTERVAL = int(os.getenv("QUEUE_PROCESS_INTERVAL", "5"))

    APP_HOST = os.getenv("APP_HOST", "127.0.0.1")
    APP_PORT = int(os.getenv("APP_PORT", 5005))
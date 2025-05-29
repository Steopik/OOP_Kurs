# app/main.py
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel, EmailStr
from email_queue import add_to_queue
from logger_config import get_logger

logger = get_logger(__name__)

app = FastAPI()

class EmailRequest(BaseModel):
    to: EmailStr
    subject: str
    body: str

@app.post("/send/")
async def send_email(request: EmailRequest):
    logger.info(f"Received request to send email to {request.to}")
    add_to_queue(request.to, request.subject, request.body)
    return {"status": "queued"}

@app.get("/")
async def root():
    return {"message": "Email notification service is running"}


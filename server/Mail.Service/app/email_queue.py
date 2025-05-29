# app/email_queue.py
import asyncio
from email_sender import send_email
from logger_config import get_logger

logger = get_logger(__name__)
queue = asyncio.Queue()

async def process_queue():
    logger.info("Email queue processor started")
    while True:
        to, subject, body = await queue.get()
        try:
            await send_email(to, subject, body)
            logger.debug(f"Email sent to {to}")
        except Exception as e:
            logger.error(f"Failed to send email to {to}: {e}")
        finally:
            queue.task_done()

def add_to_queue(to, subject, body):
    logger.debug(f"Email added to queue for {to}")
    queue.put_nowait((to, subject, body))
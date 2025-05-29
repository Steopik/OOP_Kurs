# app/email_sender.py
import aiosmtplib
from email.message import EmailMessage
from logger_config import get_logger
from config import Config

logger = get_logger(__name__)

async def send_email(to: str, subject: str, body: str):
    message = EmailMessage()
    message["From"] = Config.FROM_EMAIL
    message["To"] = to
    message["Subject"] = subject
    message.set_content(body)

    try:
        await aiosmtplib.send(
            message,
            hostname=Config.SMTP_HOST,
            port=Config.SMTP_PORT,
            username=Config.SMTP_USER,
            password=Config.SMTP_PASSWORD,
            start_tls=True,
        )
    except Exception as e:
        logger.error(f"Ошибка при отправке email на {to}: {e}")
        raise
# app/logger_config.py
import logging
import os
from logging.handlers import SMTPHandler
from config import Config

def get_logger(name=__name__):
    logger = logging.getLogger(name)
    logger.setLevel(logging.DEBUG)

    formatter = logging.Formatter(
        "%(asctime)s - %(levelname)s - [%(module)s:%(lineno)d] - %(message)s"
    )

    # Консольный логгер
    ch = logging.StreamHandler()
    ch.setLevel(getattr(logging, Config.LOG_LEVEL_CONSOLE.upper()))
    ch.setFormatter(formatter)
    logger.addHandler(ch)

    # Файловый логгер
    log_dir = os.path.dirname(Config.LOG_FILE_PATH)
    if log_dir and not os.path.exists(log_dir):
        os.makedirs(log_dir)

    fh = logging.FileHandler(Config.LOG_FILE_PATH)
    fh.setLevel(getattr(logging, Config.LOG_LEVEL_FILE.upper()))
    fh.setFormatter(formatter)
    logger.addHandler(fh)

    

    return logger
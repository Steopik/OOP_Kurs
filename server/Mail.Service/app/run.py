# app/run.py

import asyncio
from main import app
from email_queue import process_queue
from config import Config
import uvicorn

async def run_api():

    config = uvicorn.Config(app, host=Config.APP_HOST, port=Config.APP_PORT)
    server = uvicorn.Server(config)
    await server.serve()


async def main():
    # Запускаем задачи параллельно
    api_task = asyncio.create_task(run_api())
    queue_task = asyncio.create_task(process_queue())

    await asyncio.gather(api_task, queue_task)


if __name__ == "__main__":
    asyncio.run(main())
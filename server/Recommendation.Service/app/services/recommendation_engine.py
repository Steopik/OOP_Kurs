import random
from typing import List, Dict
from sklearn.metrics.pairwise import cosine_similarity
from sklearn.feature_extraction.text import CountVectorizer
from app.services.book_api import get_all_books

async def recommend_books(user_read_books: List[str], top_n: int = 10) -> List[Dict]:
    all_books = await get_all_books()

    if len(user_read_books) < 1 or len(all_books) <= 1:
        return random.sample([b.dict() for b in all_books], min(top_n, len(all_books)))

    book_map = {str(book.id): book for book in all_books}
    user_books = [book_map[bid] for bid in user_read_books if bid in book_map]
    other_books = [b for b in all_books if str(b.id) not in user_read_books]

    if not user_books or not other_books:
        return random.sample([b.dict() for b in all_books], min(top_n, len(all_books)))

    corpus = [b.title + " " + b.author + " " + b.genre for b in all_books]
    vectorizer = CountVectorizer().fit_transform(corpus)
    similarity = cosine_similarity(vectorizer)

    id_to_index = {str(book.id): i for i, book in enumerate(all_books)}
    scores = {}

    for read_id in user_read_books:
        read_idx = id_to_index.get(read_id)
        if read_idx is None:
            continue
        for other_id in id_to_index:
            if other_id in user_read_books:
                continue
            other_idx = id_to_index[other_id]
            scores[other_id] = scores.get(other_id, 0) + similarity[read_idx][other_idx]

    recommended_ids = sorted(scores, key=scores.get, reverse=True)[:top_n]
    return [book_map[bid].dict() for bid in recommended_ids if bid in book_map]

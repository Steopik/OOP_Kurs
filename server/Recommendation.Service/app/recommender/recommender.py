# app/recommender/recommender.py

from typing import List, Dict
import pandas as pd
from sklearn.metrics.pairwise import cosine_similarity
from recommender.interaction_matrix import build_interaction_matrix

def get_recommendations(user_id: str, user_books: Dict[str, List[str]], top_n: int = 5) -> List[str]:
    """
    Возвращает топ-N рекомендаций для пользователя на основе Item-Based CF
    """
    interaction_matrix = build_interaction_matrix(user_books)
    if user_id not in interaction_matrix.index:
        return []

    # Транспонируем: теперь строки — книги, столбцы — пользователи
    item_matrix = interaction_matrix.T

    # Считаем схожесть между книгами
    similarity = cosine_similarity(item_matrix)
    similarity_df = pd.DataFrame(similarity, index=item_matrix.index, columns=item_matrix.index)

    # Книги, которые прочитал пользователь
    user_read_books = interaction_matrix.loc[user_id]
    read_books = user_read_books[user_read_books > 0].index

    # Набираем рекомендации
    scores = pd.Series(dtype=float)
    for book_id in read_books:
        similar_books = similarity_df[book_id].drop(labels=read_books, errors='ignore')  # исключаем уже прочитанные
        scores = scores.add(similar_books, fill_value=0)

    # Сортировка по убыванию и выбор top-N
    recommended_books = scores.sort_values(ascending=False).head(top_n).index.tolist()
    return recommended_books

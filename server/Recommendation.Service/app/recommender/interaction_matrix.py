# app/recommender/interaction_matrix.py

from typing import Dict, List
import pandas as pd

def build_interaction_matrix(user_books: Dict[str, List[str]]) -> pd.DataFrame:
    """
    Строит матрицу взаимодействия: строки — пользователи, столбцы — книги.
    Ячейки: 1 — пользователь прочитал книгу, 0 — нет.
    """
    rows = []
    for user_id, books in user_books.items():
        for book_id in books:
            rows.append({"user_id": user_id, "book_id": book_id})
    
    df = pd.DataFrame(rows)
    matrix = df.pivot_table(index="user_id", columns="book_id", aggfunc=lambda x: 1, fill_value=0)
    return matrix

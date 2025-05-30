using Review.Service.Models;
using Microsoft.EntityFrameworkCore;
using Review.Service.DTOs;

namespace Review.Service.Services;

public class ReviewService
{
    private readonly ReviewDbContext _context;

    public ReviewService(ReviewDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> CreateReview(CreateReviewDto dto, Guid userId)
    {
        var review = new BookReview
        {
            Id = Guid.NewGuid(),
            BookId = dto.BookId,
            UserId = userId,
            Rating = dto.Rating,
            Text = dto.Text
        };

        await _context.Reviews.AddAsync(review);
        await _context.SaveChangesAsync();
        return review.Id;
    }

    public async Task<List<BookReview>> GetByBook(Guid bookId) =>
        await _context.Reviews
            .Where(r => r.BookId == bookId)
            .ToListAsync();

    public async Task<double?> GetAverageRating(Guid bookId) =>
        await _context.Reviews
            .Where(r => r.BookId == bookId)
            .Select(r => (int?)r.Rating)
            .AverageAsync();

    public async Task<bool> UpdateReview(Guid id, UpdateReviewDto dto)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review == null)
            return false;

        if (dto.Rating.HasValue && dto.Rating >= 1 && dto.Rating <= 10)
            review.Rating = dto.Rating.Value;

        if (!string.IsNullOrEmpty(dto.Text))
        if (!string.IsNullOrEmpty(dto.Text))
            review.Text = dto.Text;

        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<BookReview?> GetReviewById(Guid id) =>
        await _context.Reviews.FindAsync(id);


    public async Task<bool> DeleteReview(Guid id, Guid userId, bool isUserAdmin)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review == null) return false;

        // Проверяем права
        if (review.UserId != userId && !isUserAdmin)
            return false;

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
        return true;
    }
}
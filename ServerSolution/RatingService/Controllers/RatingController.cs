using Microsoft.AspNetCore.Mvc;
using RatingService.Data;
using RatingService.Models;
using Microsoft.EntityFrameworkCore;


namespace RatingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RatingsController : ControllerBase
{
    private readonly RatingDbContext _context;

    public RatingsController(RatingDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Rate([FromBody] Rating rating)
    {
        if (rating.Score < 1 || rating.Score > 10)
            return BadRequest("Score must be between 1 and 10.");

        var existing = await _context.Ratings
            .FirstOrDefaultAsync(r => r.UserId == rating.UserId && r.BookId == rating.BookId);

        if (existing != null)
        {
            existing.Score = rating.Score;
            existing.RatedAt = DateTime.UtcNow;
        }
        else
        {
            await _context.Ratings.AddAsync(rating);
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("book/{bookId}")]
    public async Task<IActionResult> GetBookRating(int bookId)
    {
        var ratings = await _context.Ratings.Where(r => r.BookId == bookId).ToListAsync();
        if (!ratings.Any()) return Ok("No ratings yet.");

        var average = ratings.Average(r => r.Score);
        return Ok(new { AverageRating = average, Count = ratings.Count });
    }
}


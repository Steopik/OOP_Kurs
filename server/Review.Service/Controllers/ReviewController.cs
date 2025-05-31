using Microsoft.AspNetCore.Mvc;
using Review.Service.DTOs;
using Review.Service.Services;

[ApiController]
[Route("[controller]")]
public class ReviewController : ControllerBase
{
    private readonly ExternalApiService _externalApiService;
    private readonly ReviewService _reviewService;

    public ReviewController(ExternalApiService externalApiService, ReviewService reviewService)
    {
        _externalApiService = externalApiService;
        _reviewService = reviewService;
    }

    // POST /review
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReviewDto dto)
    {
        if (dto.UserId == Guid.Empty)
            return BadRequest("Не указан UserId");

        if (!await _externalApiService.BookExists(dto.BookId))
            return NotFound("Книга не найдена");

        if (!await _externalApiService.UserExists(dto.UserId))
            return NotFound("Пользователь не найден");

        var id = await _reviewService.CreateReview(dto, dto.UserId);
        return Created($"/review/{id}", new { Id = id });
    }

    // GET /review/book/{bookId}
    [HttpGet("book/{bookId}")]
    public async Task<IActionResult> GetByBook(Guid bookId) =>
        Ok(await _reviewService.GetByBook(bookId));

    // GET /review/book/{bookId}/rating
    [HttpGet("book/{bookId}/rating")]
    public async Task<IActionResult> GetRating(Guid bookId)
    {
        var avg = await _reviewService.GetAverageRating(bookId);
        return avg.HasValue ? Ok(new { AverageRating = avg }) : NotFound();
    }

    // PUT /review/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReviewDto dto)
    {
        var review = await _reviewService.GetReviewById(id);
        if (review == null)
            return NotFound("Отзыв не найден");

        if (dto.UserId == Guid.Empty || (review.UserId != dto.UserId && !await IsUserAdmin(dto.UserId)))
            return Forbid();

        var success = await _reviewService.UpdateReview(id, dto);
        return success
            ? Ok($"Отзыв {id} обновлён")
            : StatusCode(500, "Ошибка при обновлении");
    }

    // DELETE /review/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid userId)
    {
        if (userId == Guid.Empty)
            return BadRequest("UserId не указан");

        var isAdmin = await IsUserAdmin(userId);
        var success = await _reviewService.DeleteReview(id, userId);

        return success
            ? Ok($"Отзыв {id} удалён")
            : Forbid("Нет прав на удаление");
    }

    // Вспомогательные методы
    private async Task<bool> IsUserAdmin(Guid userId)
    {
        return await _externalApiService.IsUserAdmin(userId);
    }
}
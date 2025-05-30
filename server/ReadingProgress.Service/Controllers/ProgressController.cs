using Microsoft.AspNetCore.Mvc;
using ReadingProgress.Service.DTOs;
using ReadingProgress.Service.Models;
using ReadingProgress.Service.Services;

[ApiController]
[Route("[controller]")]
public class ProgressController : ControllerBase
{
    private readonly ExternalApiService _externalApiService;
    private readonly ProgressService _progressService;

    public ProgressController(ExternalApiService externalApiService, ProgressService progressService)
    {
        _externalApiService = externalApiService;
        _progressService = progressService;
    }

    // POST /progress/update
    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] UpdateProgressDto dto)
    {
        if (dto.UserId == Guid.Empty || dto.BookId == Guid.Empty)
            return BadRequest("UserId или BookId не указан");

        if (!await _externalApiService.UserExists(dto.UserId))
            return NotFound("Пользователь не найден");

        if (!await _externalApiService.BookExists(dto.BookId))
            return NotFound("Книга не найдена");

        var success = await _progressService.UpdateProgress(dto);
        return success ? Ok(new { Message = "Прогресс обновлён" }) : StatusCode(500);
    }

    // GET /progress/{userId}/{bookId}
    [HttpGet("{userId}/{bookId}")]
    public async Task<IActionResult> Get(Guid userId, Guid bookId)
    {
        if (!await _externalApiService.UserExists(userId))
            return NotFound("Пользователь не найден");

        if (!await _externalApiService.BookExists(bookId))
            return NotFound("Книга не найдена");

        var progress = await _progressService.GetProgress(userId, bookId);
        return progress != null
            ? Ok(progress)
            : NotFound("Прогресс не найден");
    }

    // GET /progress/{userId}?status=reading
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetAll(Guid userId, [FromQuery] ReadingStatus status)
    {
        if (!await _externalApiService.UserExists(userId))
            return NotFound("Пользователь не найден");

        var result = await _progressService.GetUserProgressByStatus(userId, status);
        return Ok(result);
    }

    // DELETE /progress/reset/{userId}/{bookId}
    [HttpDelete("reset/{userId}/{bookId}")]
    public async Task<IActionResult> Reset(Guid userId, Guid bookId)
    {
        if (!await _externalApiService.UserExists(userId))
            return NotFound("Пользователь не найден");

        if (!await _externalApiService.BookExists(bookId))
            return NotFound("Книга не найдена");

        var isAdmin = await _externalApiService.IsUserAdmin(userId);

        var success = await _progressService.ResetProgress(userId, bookId, isAdmin);
        return success ? Ok("Прогресс сброшен") : Forbid();
    }
}
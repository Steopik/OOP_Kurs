using BookStatusService.Data;
using BookStatusService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace BookStatusService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookStatusController : ControllerBase
{
    private readonly BookStatusDbContext _context;

    public BookStatusController(BookStatusDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> SetStatus([FromBody] BookUserStatus status)
    {
        var existing = await _context.BookStatuses
            .FirstOrDefaultAsync(s => s.UserId == status.UserId && s.BookId == status.BookId);

        if (existing != null)
        {
            existing.Status = status.Status;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            await _context.BookStatuses.AddAsync(status);
        }

        await _context.SaveChangesAsync();
        return Ok(status);
    }


    [HttpGet]
    public async Task<IActionResult> GetStatus([FromQuery] int bookId, [FromQuery] int userId)
    {
        var existing = await _context.BookStatuses
            .FirstOrDefaultAsync(s => s.UserId == userId && s.BookId == bookId);

        if (existing != null) return Ok(existing);
        return BadRequest("status does not exist");
    }
}


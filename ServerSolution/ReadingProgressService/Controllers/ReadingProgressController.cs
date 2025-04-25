using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadingProgressService.Data;
using ReadingProgressService.Models;
using System.Text.Json;

namespace ReadingProgressService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReadingProgressController : ControllerBase
{
    private readonly ReadingProgressDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly string _bookUrl = "http://localhost:5054/api/books";
    private readonly string _userUrl = "http://localhost:5034/api/Users";

    public ReadingProgressController(ReadingProgressDbContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClient = httpClientFactory.CreateClient();
    }

    [HttpGet]
    public async Task<IActionResult> GetProgress([FromQuery] int userId, [FromQuery] int bookId)
    {
        var progress = await _context.ReadingProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.BookId == bookId);

        if (progress == null) return NotFound("No reading progress found.");
        return Ok(progress);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProgress([FromBody] ReadingProgress progress)
    {
        Book? book = await GetBookByIdAsync(progress.BookId);
        User? user = await GetUserByIdAsync(progress.UserId);
        

        if (book == null || user == null) return NotFound(string.Empty);

        if (progress.CurrentPage < 1 || progress.CurrentPage > book.Length) return BadRequest("Progress value is out of range.");

        await _context.ReadingProgresses.AddAsync(progress);
        await _context.SaveChangesAsync();
        return Ok(progress);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProgress([FromBody] ReadingProgress progress)
    {
        var existing = await _context.ReadingProgresses
            .FirstOrDefaultAsync(p => p.UserId == progress.UserId && p.BookId == progress.BookId);

        if (existing == null)
            return NotFound("Progress record not found.");

        Book? book = await GetBookByIdAsync(progress.BookId);
        User? user = await GetUserByIdAsync(progress.UserId);


        if (book == null || user == null) return NotFound(string.Empty);

        if (progress.CurrentPage < 1 || progress.CurrentPage > book.Length) return BadRequest("Progress value is out of range.");

        existing.CurrentPage = progress.CurrentPage;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteProgress([FromBody] ReadingProgress progress)
    {
        var existing = await _context.ReadingProgresses
            .FirstOrDefaultAsync(p => p.UserId == progress.UserId && p.BookId == progress.BookId);

        if (existing == null)
            return NotFound("Progress record not found.");

        _context.ReadingProgresses.Remove(existing);
        await _context.SaveChangesAsync();
        return Ok("Entry was deleted");
    }

    private async Task<Book?> GetBookByIdAsync(int bookId)
    {
        var response = await _httpClient.GetAsync($"{_bookUrl}/{bookId}");
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        var book = JsonSerializer.Deserialize<Book>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return book;
    }


    private async Task<User?> GetUserByIdAsync(int userId)
    {
        var response = await _httpClient.GetAsync($"{_userUrl}/{userId}");
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<User>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return user;
    }
}
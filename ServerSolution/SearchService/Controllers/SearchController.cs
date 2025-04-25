using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SearchService.Data;

namespace SearchService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly SearchDbContext _context;

    public SearchController(SearchDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? title, [FromQuery] string? author, [FromQuery] string? genre)
    {
        var query = _context.Books.AsQueryable();

        if (!string.IsNullOrEmpty(title))
            query = query.Where(b => b.Title.Contains(title));

        if (!string.IsNullOrEmpty(author))
            query = query.Where(b => b.Author.Contains(author));

        if (!string.IsNullOrEmpty(genre))
            query = query.Where(b => b.Genres.Contains(genre));

        var results = await query.ToListAsync();

        return Ok(results);
    }
}

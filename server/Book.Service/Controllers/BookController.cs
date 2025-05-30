using Book.Service.DTOs;
using Book.Service.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Book.Service.Controllers;

[ApiController]
[Route("[controller]")]
public class BookController : ControllerBase
{
    private readonly BookService _bookService;

    public BookController(BookService bookService)
    {
        _bookService = bookService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] UploadBookRequestDto dto, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Файл не выбран");

        var book = new Book.Service.Models.Book
        {
            Title = dto.Title,
            Author = dto.Author,
            Genre = dto.Genre,
            FilePath = string.Empty
        };

        var bookId = await _bookService.AddBookAsync(book, file.OpenReadStream());
        return CreatedAtAction(nameof(GetById), new { id = bookId }, bookId);
    }

    // GET /book/read/{id}?page=...
    [HttpGet("read/{id}")]
    public async Task<IActionResult> Read(Guid id, [FromQuery] int page = 1)
    {
        var stream = await _bookService.GetSinglePage(id, page);
        if (stream == null)
            return NotFound("Книга или страница не найдены");

        return File(stream, "application/pdf", $"page_{page}_{id}.pdf");
    }

    // GET /book/pages/{id}?start=...&end=...
    [HttpGet("pages/{id}")]
    public async Task<IActionResult> GetPages(Guid id, [FromQuery] int start = 1, [FromQuery] int end = 1)
    {
        var stream = await _bookService.GetPageRange(id, start, end);
        if (stream == null)
            return NotFound("Книга или страницы не найдены");

        return File(stream, "application/pdf", $"pages_{start}-{end}_{id}.pdf");
    }

    // GET /book/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var book = await _bookService.GetByIdAsync(id);
        return book != null ? Ok(book) : NotFound("Книга не найдена");
    }

    // GET /book
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var books = await _bookService.GetAllAsync();
        return Ok(books);
    }

    // GET /book/search?query=...
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        var result = await _bookService.SearchAsync(query);
        return Ok(result);
    }

    // GET /book/filter?author=...&genre=...
    [HttpGet("filter")]
    public async Task<IActionResult> Filter([FromQuery] BookFilterDto filter)
    {
        // Декодируем параметры из URL
        if (!string.IsNullOrEmpty(filter.Genre))
            filter.Genre = WebUtility.UrlDecode(filter.Genre);

        if (!string.IsNullOrEmpty(filter.Author))
            filter.Author = WebUtility.UrlDecode(filter.Author);

        var result = await _bookService.FilterAsync(filter);
        return Ok(result);
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _bookService.DeleteBookAsync(id);
        return success ? Ok($"Книга {id} удалена") : NotFound("Книга не найдена");
    }
}
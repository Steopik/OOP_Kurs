using BookService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookService.Data;
using Spire.Pdf;
using Microsoft.AspNetCore.Http.HttpResults;


namespace BookService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BooksController : ControllerBase
{
    private readonly BookDbContext _context;

    public BooksController(BookDbContext context)
    {
        _context = context;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadBook(IFormFile file, [FromForm] BookUploadModel bookModel)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var filePath = Path.Combine("ServerData", "books", bookModel.Author, file.FileName);
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), filePath);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var book = new Book
        {
            Title = bookModel.Title,
            Author = bookModel.Author,
            Genres = bookModel.Genres,
            Length = bookModel.Length,
            Description = bookModel.Description,
            FilePath = filePath 
        };

        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Book uploaded successfully", filePath = filePath });
    }

    [HttpGet("download/{id}")]
    public async Task<IActionResult> DownloadBook(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null)
            return NotFound();

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), book.FilePath);
        if (!System.IO.File.Exists(filePath))
            return NotFound();

        var fileBytes = System.IO.File.ReadAllBytes(filePath);
        return File(fileBytes, "application/octet-stream", Path.GetFileName(filePath));
    }

    [HttpGet]
    public async Task<IActionResult> GetBooks()
    {
        var books = await _context.Books.ToListAsync();
        return Ok(books);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBook(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null)
            return NotFound();

        return Ok(book);
    }


    [HttpGet("pages/{id}")]
    public async Task<IActionResult> GetBookPages(int id, [FromQuery] int begin, [FromQuery] int end)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null)
            return NotFound();

        if (begin < 1 || end > book.Length || begin > end)
            return BadRequest("Invalid page range.");

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), book.FilePath);
        if (!System.IO.File.Exists(filePath))
            return NotFound("File not found.");

        try
        {
            string outputFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");

            PdfDocument sourceDoc = new PdfDocument();
            sourceDoc.LoadFromFile(filePath);

            PdfDocument newPdf = new PdfDocument();

            for (int i = begin - 1; i < end; i++)
            {
                if (i < sourceDoc.Pages.Count)
                {
                    newPdf.InsertPage(sourceDoc, i);
                }
            }

            newPdf.SaveToFile(outputFilePath);

            var fileBytes = System.IO.File.ReadAllBytes(outputFilePath);
            var fileName = $"{book.Title}_{begin}_{end}.pdf";
            return File(fileBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}\n{ex.StackTrace}");
            return StatusCode(500, $"Error extracting pages: {ex.Message}");
        }
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null)
        {
            return NotFound("Book not found.");
        }

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), book.FilePath);

        if (System.IO.File.Exists(filePath))
        {
            try
            {
                System.IO.File.Delete(filePath);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting file: {ex.Message}");
            }
        }

        _context.Books.Remove(book);

        await _context.SaveChangesAsync();

        return Ok("Book was deleted");
    }

}


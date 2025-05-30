using Book.Service.Models;
using Book.Service.DTOs;
using Microsoft.EntityFrameworkCore;
using Spire.Pdf;



namespace Book.Service.Services;
public class BookService
{
    private readonly BookDbContext _context;
    private readonly string _booksPath;
    private readonly PdfService _pdfService;

    public BookService(BookDbContext context, IConfiguration config, PdfService pdfService)
    {
        _context = context;
        _booksPath = Path.Combine(Directory.GetCurrentDirectory(), config["BookSettings:BooksFolderPath"]);
        _pdfService = pdfService;
    }

    public async Task<List<Book.Service.Models.Book>> GetAllAsync() => await _context.Books.ToListAsync();

    public async Task<List<Book.Service.Models.Book>> SearchAsync(string query)
    {
        if (string.IsNullOrEmpty(query))
            return await _context.Books.ToListAsync();

        var queryUpper = query.ToUpper();
        return await _context.Books
            .Where(b => b.Title.ToUpper().Contains(queryUpper) ||
                        b.Author.ToUpper().Contains(queryUpper))
            .ToListAsync();
    }

    public async Task<List<Book.Service.Models.Book>> FilterAsync(BookFilterDto filter)
    {
        var query = _context.Books.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Author))
        {
            var author = filter.Author;
            query = query.Where(b => b.Author == author);
        }

        if (!string.IsNullOrWhiteSpace(filter.Genre))
        {
            var genre = filter.Genre;
            query = query.Where(b => b.Genre == genre); 


        }

        if (filter.MinPages.HasValue)
            query = query.Where(b => b.Pages >= filter.MinPages.Value);

        if (filter.MaxPages.HasValue)
            query = query.Where(b => b.Pages <= filter.MaxPages.Value);

        return await query.ToListAsync();
    }


    public async Task<Book.Service.Models.Book?> GetByIdAsync(Guid id) =>
        await _context.Books.FirstOrDefaultAsync(b => b.Id == id);

    public string GetFilePath(Book.Service.Models.Book book) =>
        Path.Combine(_booksPath, book.Author, $"{book.Title}.pdf");

    public FileStream? GetFileStream(Book.Service.Models.Book book)
    {
        var path = GetFilePath(book);
        return File.Exists(path) ? File.OpenRead(path) : null;
    }

    public async Task<Guid> AddBookAsync(Book.Service.Models.Book book, Stream fileStream)
    {
        var folder = Path.Combine(_booksPath, book.Author);
        Directory.CreateDirectory(folder);

        var filePath = Path.Combine(folder, $"{book.Title}.pdf");

        // Сохраняем файл
        using (var output = File.Create(filePath))
        {
            await fileStream.CopyToAsync(output);
        }

        book.FilePath = filePath;
        book.Pages = CountPages(filePath); // метод для подсчёта страниц

        await _context.Books.AddAsync(book);
        await _context.SaveChangesAsync();

        return book.Id;
    }


    private int CountPages(string filePath)
    {
        using var document = new PdfDocument();
        document.LoadFromFile(filePath);
        return document.Pages.Count;
    }

    public async Task<Stream?> GetSinglePage(Guid bookId, int pageNumber) =>
        await _pdfService.GetSinglePage(bookId, pageNumber, _context.Books.First(b => b.Id == bookId).FilePath);

    // Получить диапазон страниц
    public async Task<Stream?> GetPageRange(Guid bookId, int startPage, int endPage) =>
        await _pdfService.GetPageRange(bookId, startPage, endPage, _context.Books.First(b => b.Id == bookId).FilePath);



    public async Task<bool> DeleteBookAsync(Guid id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null) return false;

        var filePath = book.FilePath;

        // Удаляем файл
        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
            }
            catch
            {
                return false; // Не можем удалить файл
            }
        }

        // Удаляем запись из БД
        _context.Books.Remove(book);
        await _context.SaveChangesAsync();

        return true;
    }
}
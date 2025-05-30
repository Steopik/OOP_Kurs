using Spire.Pdf;

namespace Book.Service.Services;


public class PdfService
{
    private readonly string _booksPath;

    public PdfService(IConfiguration config)
    {
        _booksPath = Path.Combine(Directory.GetCurrentDirectory(), config["BookSettings:BooksFolderPath"]);
    }

    // Получить одну страницу как PDF
    public async Task<Stream?> GetSinglePage(Guid bookId, int pageNumber, string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_page.pdf");

        using var document = new PdfDocument();
        document.LoadFromFile(filePath);

        // Сохраняем только нужную страницу
        document.SaveToFile(tempFile, pageNumber - 1, pageNumber - 1, FileFormat.PDF);

        var stream = File.OpenRead(tempFile);
        var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        stream.Close();
        File.Delete(tempFile);

        memoryStream.Position = 0;
        return memoryStream;
    }

    // Получить диапазон страниц как PDF
    public async Task<Stream?> GetPageRange(Guid bookId, int startPage, int endPage, string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_pages.pdf");

        using var document = new PdfDocument();
        document.LoadFromFile(filePath);

        // Сохраняем диапазон страниц
        document.SaveToFile(tempFile, startPage - 1, endPage - 1, FileFormat.PDF);

        var stream = File.OpenRead(tempFile);
        var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        stream.Close();
        File.Delete(tempFile);

        memoryStream.Position = 0;
        return memoryStream;
    }
}
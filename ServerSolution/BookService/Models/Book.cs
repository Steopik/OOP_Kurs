namespace BookService.Models;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Author { get; set; } = null!;
    public string Genres { get; set; } = null!;  
    public int Length { get; set; }
    public string Description { get; set; } = null!;
    public string FilePath { get; set; } = null!; 
}

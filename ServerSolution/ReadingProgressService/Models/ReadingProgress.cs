namespace ReadingProgressService.Models;

public class ReadingProgress
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int BookId { get; set; }
    public int CurrentPage { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

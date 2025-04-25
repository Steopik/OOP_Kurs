namespace BookStatusService.Models;

public class BookUserStatus
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int BookId { get; set; }
    public BookStatus Status { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}


namespace RatingService.Models;

public class Rating
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int BookId { get; set; }
    public int Score { get; set; } 
    public DateTime RatedAt { get; set; } = DateTime.UtcNow;
}



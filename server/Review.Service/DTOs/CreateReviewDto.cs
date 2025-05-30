namespace Review.Service.DTOs;

public class CreateReviewDto
{
    public Guid BookId { get; set; }
    public int Rating { get; set; } // от 1 до 10
    public string Text { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}
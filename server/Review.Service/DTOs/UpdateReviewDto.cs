namespace Review.Service.DTOs;

public class UpdateReviewDto
{
    public int? Rating { get; set; }
    public string? Text { get; set; }
    public Guid UserId { get; set; }
}
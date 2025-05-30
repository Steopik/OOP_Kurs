using ReadingProgress.Service.Models;

namespace ReadingProgress.Service.DTOs;

public class UpdateProgressDto
{
    public Guid UserId { get; set; }
    public Guid BookId { get; set; }
    public int? Page { get; set; }
    public ReadingStatus Status { get; set; } = ReadingStatus.Planned;
}
using System;

namespace ReadingProgress.Service.Models;
public enum ReadingStatus
{
    Reading,     // Сейчас читаю
    Planned,    // В планах
    Finished,   // Прочитано
    Dropped     // Бросил
}
public class ReadingProgress
{
    public Guid UserId { get; set; }
    public Guid BookId { get; set; }

    public int? CurrentPage { get; set; } = null;
    public DateTime LastReadAt { get; set; } = DateTime.UtcNow;

    public ReadingStatus Status { get; set; } = ReadingStatus.Planned;
}
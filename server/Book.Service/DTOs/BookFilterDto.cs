namespace Book.Service.DTOs;

public class BookFilterDto
{
    public string? Author { get; set; }
    public string? Genre { get; set; }
    public int? MinPages { get; set; }
    public int? MaxPages { get; set; }
}
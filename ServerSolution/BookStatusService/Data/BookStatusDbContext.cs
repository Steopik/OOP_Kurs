using BookStatusService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStatusService.Data;

public class BookStatusDbContext : DbContext
{
    public BookStatusDbContext(DbContextOptions<BookStatusDbContext> options) : base(options) { }

    public DbSet<BookUserStatus> BookStatuses { get; set; }
}

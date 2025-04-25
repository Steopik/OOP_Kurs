using Microsoft.EntityFrameworkCore;
using ReadingProgressService.Models;

namespace ReadingProgressService.Data;

public class ReadingProgressDbContext : DbContext
{
    public ReadingProgressDbContext(DbContextOptions<ReadingProgressDbContext> options) : base(options) { }

    public DbSet<ReadingProgress> ReadingProgresses { get; set; }
}

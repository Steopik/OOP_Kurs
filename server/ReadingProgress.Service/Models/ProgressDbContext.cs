using Microsoft.EntityFrameworkCore;
using ReadingProgress.Service.Models;
using System.Collections.Generic;

namespace ReadingProgress.Service.Models;

public class ProgressDbContext : DbContext
{
    public DbSet<ReadingProgress> Progresses { get; set; }

    public ProgressDbContext(DbContextOptions<ProgressDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReadingProgress>()
            .HasKey(p => new { p.UserId, p.BookId });
    }
}
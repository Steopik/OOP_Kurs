using Microsoft.EntityFrameworkCore;
using Review.Service.Models;

namespace Review.Service.Models;

public class ReviewDbContext : DbContext
{
    public DbSet<BookReview> Reviews { get; set; }

    public ReviewDbContext(DbContextOptions<ReviewDbContext> options)
        : base(options)
    {
    }
}
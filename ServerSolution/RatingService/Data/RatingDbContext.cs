using RatingService.Models;
using Microsoft.EntityFrameworkCore;

namespace RatingService.Data;

public class RatingDbContext : DbContext
{
    public RatingDbContext(DbContextOptions<RatingDbContext> options) : base(options) { }

    public DbSet<Rating> Ratings { get; set; }
}

using Microsoft.EntityFrameworkCore;
using SearchService.Models;
using System.Collections.Generic;

namespace SearchService.Data;

public class SearchDbContext : DbContext
{
    public SearchDbContext(DbContextOptions<SearchDbContext> options) : base(options) { }

    public DbSet<Book> Books => Set<Book>();
}

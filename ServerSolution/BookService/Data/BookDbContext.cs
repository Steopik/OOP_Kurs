using Microsoft.EntityFrameworkCore;
using BookService.Models;
using System.Collections.Generic;

namespace BookService.Data;

public class BookDbContext : DbContext
{
    public BookDbContext(DbContextOptions<BookDbContext> options) : base(options) { }

    public DbSet<Book> Books => Set<Book>();
}

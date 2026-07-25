using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class InMemDbContext : DbContext
{
    public InMemDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Inventory> Inventory => Set<Inventory>();
}

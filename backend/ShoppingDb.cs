using Microsoft.EntityFrameworkCore;

namespace Backend;

public class ShoppingDb : DbContext
{
    public ShoppingDb(DbContextOptions<ShoppingDb> options) : base(options) { }
    public DbSet<ShoppingItem> Items => Set<ShoppingItem>();
}

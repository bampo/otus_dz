using Microsoft.EntityFrameworkCore;
using OrderDal.Models;

namespace OrderDal;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    public DbSet<Order> Orders { get; set; }
}


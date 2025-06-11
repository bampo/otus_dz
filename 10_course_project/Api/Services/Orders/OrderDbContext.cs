using Microsoft.EntityFrameworkCore;
using Orders.Service.Models;
using Orders.Service.Saga;

namespace Orders.Service;

public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderSagaState>().HasKey(s => s.CorrelationId);
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderSagaState> SagaData { get; set; }
    public DbSet<OrderList> OrdersLists { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

}
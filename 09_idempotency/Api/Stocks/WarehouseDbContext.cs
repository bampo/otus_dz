using Microsoft.EntityFrameworkCore;
using Stocks.Models;

namespace Stocks;

public class WarehouseDbContext : DbContext
{
    public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options) : base(options)
    { }

    public DbSet<StockReservation> StockReservations { get; set; }
    public DbSet<Stock> Stocks { get; set; }
}
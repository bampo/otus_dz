using Microsoft.EntityFrameworkCore;
using Stocks.Models;

namespace Stocks;

public class WarehouseDbContext(DbContextOptions<WarehouseDbContext> options) : DbContext(options)
{

    public DbSet<StockReservation> StockReservations { get; set; }
    public DbSet<Stock> Stocks { get; set; }
}
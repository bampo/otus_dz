using Microsoft.EntityFrameworkCore;
using Stubs.Service.Models;

namespace Stubs.Service.DbContexts;

public class StubsDbContext(DbContextOptions<StubsDbContext> options) : DbContext(options)
{

    public DbSet<StockReservation> StockReservations { get; set; }
    public DbSet<Stock> Stocks { get; set; }

    public DbSet<Payment> Payments { get; set; }


    public DbSet<Delivery> Deliveries { get; set; }
    public DbSet<DeliverySlot> DeliverySlots { get; set; }
    public DbSet<DeliveryReservation> DeliveryReservations { get; set; }
}
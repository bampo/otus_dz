using Delivery.Service.Models;
using Microsoft.EntityFrameworkCore;

public class DeliveryDbContext : DbContext
{
    public DeliveryDbContext(DbContextOptions<DeliveryDbContext> options) : base(options)
    {
    }

    public DbSet<Delivery.Service.Models.Delivery> Deliveries { get; set; }
    public DbSet<DeliverySlot> DeliverySlots { get; set; }
    public DbSet<DeliveryReservation> DeliveryReservations { get; set; }
}
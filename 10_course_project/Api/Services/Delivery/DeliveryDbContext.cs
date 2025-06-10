using Delivery.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Service;

public class DeliveryDbContext(DbContextOptions<DeliveryDbContext> options) : DbContext(options)
{

    public DbSet<Delivery.Service.Models.Delivery> Deliveries { get; set; }
    public DbSet<DeliverySlot> DeliverySlots { get; set; }
    public DbSet<DeliveryReservation> DeliveryReservations { get; set; }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Delivery.Service.Models;

public class Delivery
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid OrderId { get; set; }

    [Required]
    public int TimeSlot { get; set; }

    public string? CourierId { get; set; }

    [Required]
    public DeliveryStatus Status { get; set; } // Reserved, Canceled
}

public enum DeliveryStatus
{
    Reserved = 0,
    Cancelled = 1
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Delivery.Service.Models;

public class DeliverySlot
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public int TimeSlot { get; set; }

    [Required]
    public bool IsAvailable { get; set; }
}
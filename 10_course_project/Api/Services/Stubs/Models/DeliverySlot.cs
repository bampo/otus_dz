using System.ComponentModel.DataAnnotations;

namespace Stubs.Service.Models;

public class DeliverySlot
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public int TimeSlot { get; set; }

    [Required]
    public bool IsAvailable { get; set; }
}
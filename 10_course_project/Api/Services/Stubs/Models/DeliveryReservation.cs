using System.ComponentModel.DataAnnotations;

namespace Stubs.Service.Models;

public class DeliveryReservation
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid OrderId { get; set; }

    [Required]
    public int TimeSlot { get; set; }
}
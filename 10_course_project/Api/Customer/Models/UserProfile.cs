#nullable disable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Customers.Models;

public class UserProfile
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Address { get; set; }

    // Add other profile fields as needed

    [ForeignKey("Customer")]
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; }
}
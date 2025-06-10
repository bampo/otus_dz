using Customers.Models;
using Microsoft.EntityFrameworkCore;

namespace Customers;

public class CustomerDbContext(DbContextOptions<CustomerDbContext> options) : DbContext(options)
{

    public DbSet<Customer> Customers { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}
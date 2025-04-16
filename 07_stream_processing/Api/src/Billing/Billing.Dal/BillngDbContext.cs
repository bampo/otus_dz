using Microsoft.EntityFrameworkCore;
using Billing.Dal.Models;

namespace Billing.Dal;
public class BillngDbContext : DbContext
{
    public BillngDbContext(DbContextOptions<BillngDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
}


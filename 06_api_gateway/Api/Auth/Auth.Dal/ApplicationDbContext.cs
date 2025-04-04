using Microsoft.EntityFrameworkCore;

namespace Auth.Dal
{
    public class ApplicationDbContext(string connectionString) : DbContext
    {
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(connectionString);
        }
    }
}

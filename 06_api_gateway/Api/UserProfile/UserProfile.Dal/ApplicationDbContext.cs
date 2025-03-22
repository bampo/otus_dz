using Microsoft.EntityFrameworkCore;

namespace UserProfile.Dal
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

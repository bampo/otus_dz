using Microsoft.EntityFrameworkCore;

namespace EfDal
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

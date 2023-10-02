using ArmyAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ArmyAPI.Data
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options) : base(options) { 
        }

        public DbSet<Users> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Users>().ToTable("users");
        }
    }
}

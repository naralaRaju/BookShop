using EcomerceRazo.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Category> Categories { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category { id = 1, DisplayOrder = 1, Name = "Action" },
                new Category { id = 2, Name = "Scifi", DisplayOrder = 2 },
                new Category { id = 3, Name = "History", DisplayOrder = 3 }
                );
        }

    }
}

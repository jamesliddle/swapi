using Microsoft.EntityFrameworkCore;
using StarshipApi.Models;

namespace StarshipApi.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Starship> Starships { get; set; } = null!;

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Starship>(entity =>
            {
                entity.HasIndex(s => s.Name);
                entity.HasIndex(s => s.Model);
                entity.HasIndex(s => s.Manufacturer);
                entity.HasIndex(s => s.CostInCredits);
            });
        }
    }
}

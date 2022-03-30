using Microsoft.EntityFrameworkCore;
using MinimalApiDotNet.Models;

namespace MinimalApiDotNet.Data
{
    public class MinimalContextDb : DbContext
    {
        public MinimalContextDb(DbContextOptions<MinimalContextDb> options) : base(options) { }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Product>()
                .Property(p => p.Name)
                .IsRequired()
                .HasColumnType("varchar(200)");

            modelBuilder.Entity<Product>()
                .ToTable("Products");

            base.OnModelCreating(modelBuilder);
        }
    }
}

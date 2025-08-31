using Microsoft.EntityFrameworkCore;
using EcommerceStore.Models;

namespace EcommerceStore.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Product configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
            });

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Password).HasMaxLength(255);
                entity.Property(e => e.Address).HasMaxLength(500);
            });

            // Order configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.UserName).HasMaxLength(100);
                entity.Property(e => e.UserEmail).HasMaxLength(100);
                entity.Property(e => e.ShippingAddress).HasMaxLength(500);
                entity.Property(e => e.Status).HasMaxLength(50);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Orders)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // OrderItem configuration
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ProductName).HasMaxLength(200);

                entity.HasOne(e => e.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Product)
                    .WithMany()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Smartphone", Description = "Latest model smartphone", Price = 699.99m, ImageUrl = "https://hukut.com/_next/image?url=https%3A%2F%2Fcdn.hukut.com%2FCategory%2520Page%2520Web%2520View%2520(1).webp1754585005576&w=1920&q=75", Stock = 50, Category = "Electronics" },
                new Product { Id = 2, Name = "Laptop", Description = "High-performance laptop", Price = 999.99m, ImageUrl = "https://via.placeholder.com/200x200?text=Laptop", Stock = 30, Category = "Electronics" },
                new Product { Id = 3, Name = "T-Shirt", Description = "Cotton T-shirt", Price = 19.99m, ImageUrl = "https://via.placeholder.com/200x200?text=T-Shirt", Stock = 100, Category = "Clothing" },
                new Product { Id = 4, Name = "Jeans", Description = "Blue jeans", Price = 49.99m, ImageUrl = "https://via.placeholder.com/200x200?text=Jeans", Stock = 75, Category = "Clothing" },
                new Product { Id = 5, Name = "Headphones", Description = "Wireless headphones", Price = 199.99m, ImageUrl = "https://via.placeholder.com/200x200?text=Headphones", Stock = 40, Category = "Electronics" },
                new Product { Id = 6, Name = "Sneakers", Description = "Running sneakers", Price = 79.99m, ImageUrl = "https://via.placeholder.com/200x200?text=Sneakers", Stock = 60, Category = "Footwear" }
            );
        }
    }
}
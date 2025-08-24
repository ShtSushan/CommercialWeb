using Microsoft.EntityFrameworkCore;
using SimpleECommerceAPI.Models;

namespace SimpleECommerceAPI.Data
{
    public class ECommerceContext : DbContext
    {
        public ECommerceContext(DbContextOptions<ECommerceContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed Products
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Smartphone", Description = "Latest model smartphone", Price = 699.99m, ImageUrl = "https://via.placeholder.com/200x200?text=Phone", Stock = 50, Category = "Electronics" },
                new Product { Id = 2, Name = "Laptop", Description = "High-performance laptop", Price = 999.99m, ImageUrl = "https://via.placeholder.com/200x200?text=Laptop", Stock = 30, Category = "Electronics" },
                new Product { Id = 3, Name = "T-Shirt", Description = "Cotton T-shirt", Price = 19.99m, ImageUrl = "https://via.placeholder.com/200x200?text=T-Shirt", Stock = 100, Category = "Clothing" },
                new Product { Id = 4, Name = "Jeans", Description = "Blue jeans", Price = 49.99m, ImageUrl = "https://via.placeholder.com/200x200?text=Jeans", Stock = 75, Category = "Clothing" },
                new Product { Id = 5, Name = "Headphones", Description = "Wireless headphones", Price = 199.99m, ImageUrl = "https://via.placeholder.com/200x200?text=Headphones", Stock = 40, Category = "Electronics" },
                new Product { Id = 6, Name = "Sneakers", Description = "Running sneakers", Price = 79.99m, ImageUrl = "https://via.placeholder.com/200x200?text=Sneakers", Stock = 60, Category = "Footwear" }, 
                new Product {  Id=7,  Name = "Air Jordan", Description="Running Jordan", Price=80m, ImageUrl= "https://i5.walmartimages.com/seo/Nike-Air-Jordan-1-Mid-Men-s-Basketball-Shoes-Size-9-5_18b5ec2b-f032-475c-bc56-cd1fa9e13868.770d9ba5e1ea2ae48b980a16ae1bd341.jpeg",  Stock = 60, Category = "Footwear" }
            );

            // Configure Order Items as owned entity
            modelBuilder.Entity<Order>()
                .OwnsMany(o => o.Items, item =>
                {
                    item.Property(i => i.ProductId);
                    item.Property(i => i.ProductName);
                    item.Property(i => i.Price);
                    item.Property(i => i.Quantity);
                    item.Property(i => i.Total);
                });
        }
    }
}
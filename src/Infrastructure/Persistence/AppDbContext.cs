using Microsoft.EntityFrameworkCore;
using Domain.Models;

namespace Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    //Definir tabelas
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderProduct> OrderProducts { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<User> Users { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UserConfiguration());

        modelBuilder.Entity<Order>()
        .Property(o => o.Status)
        .HasConversion<string>();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.UserEmail)
            .IsUnique();

        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.CustomerEmail)
            .IsUnique();  

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany()
            .HasForeignKey(o => o.CustomerId);

        modelBuilder.Entity<OrderProduct>()
            .HasKey(op => new {op.OrderId, op.ProductId});
        
        modelBuilder.Entity<OrderProduct>()
            .HasOne(op => op.Order)
            .WithMany(o => o.OrderProducts)
            .HasForeignKey(op => op.OrderId);

        modelBuilder.Entity<OrderProduct>()
            .HasOne(op => op.Product)
            .WithMany(p => p.OrderProducts)
            .HasForeignKey(op => op.ProductId);

        modelBuilder.Entity<OrderProduct>()
            .Property(op => op.ProductName)
            .HasColumnType("text");
    }
}

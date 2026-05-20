using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;

namespace NexusERP.Infrastructure.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Product> Products { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<SystemAuditLog> SystemAuditLogs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Map Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductId);
                entity.Property(e => e.Price).HasColumnType("decimal(10,2)");
                entity.Property(e => e.CostPrice).HasColumnType("decimal(10,2)");

                entity.Ignore(e => e.CategoryName);
            });

            // 2. Map Inventory Transactions
            modelBuilder.Entity<InventoryTransaction>(entity =>
            {
                entity.HasKey(e => e.TransactionId);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Profit).HasColumnType("decimal(18,2)");

                entity.Property(e => e.TransactionType)
                      .HasConversion(
                          v => v.ToString(),
                          v => (TransactionAction)System.Enum.Parse(typeof(TransactionAction), v));


                entity.Ignore(e => e.ProductName);
                entity.Ignore(e => e.SupplierName);
            });

            // 3. Map Users
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);

                 entity.Property(e => e.Role)
                      .HasConversion(
                          v => v.ToString(),
                          v => (UserRole)System.Enum.Parse(typeof(UserRole), v));
                entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETDATE()")
                  .ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<Category>().HasKey(e => e.CategoryId);
            modelBuilder.Entity<Supplier>().HasKey(e => e.SupplierId);
            modelBuilder.Entity<SystemAuditLog>().HasKey(e => e.LogId);
        }
    }
}


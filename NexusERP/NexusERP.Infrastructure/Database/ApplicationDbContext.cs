using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NexusERP.Application.Interfaces;
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

            modelBuilder.Entity<Category>().HasQueryFilter(c => c.IsActive);
            modelBuilder.Entity<Product>().HasQueryFilter(p => p.IsActive);
            modelBuilder.Entity<Supplier>().HasQueryFilter(s => s.IsActive);
            modelBuilder.Entity<User>().HasQueryFilter(u => u.IsActive);

            // 1. Map Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductId);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CostPrice).HasColumnType("decimal(18,2)");

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
                          v => (TransactionAction)Enum.Parse(typeof(TransactionAction), v));
            });

            // 3. Map Users
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);

                 entity.Property(e => e.Role)
                      .HasConversion(
                          v => v.ToString(),
                          v => (UserRole)Enum.Parse(typeof(UserRole), v));
            });

            modelBuilder.Entity<Category>().HasKey(e => e.CategoryId);
            modelBuilder.Entity<Supplier>().HasKey(e => e.SupplierId);
            modelBuilder.Entity<SystemAuditLog>().HasKey(e => e.LogId);

        }

        public override int SaveChanges()
        {
            var entries = ChangeTracker.Entries();

            foreach (var entry in entries)
            {
                if (entry.Entity is ICreationTracked createdEntity && entry.State == EntityState.Added)
                {
                    createdEntity.CreatedAt = DateTime.UtcNow;
                }

                if (entry.Entity is IAuditTracked auditableEntity)
                {
                    if (entry.State == EntityState.Added)
                    {
                        auditableEntity.UpdatedAt = DateTime.UtcNow;
                    }

                    if (entry.State == EntityState.Modified)
                    {
                        entry.Property(nameof(ICreationTracked.CreatedAt)).IsModified = false; 
                        auditableEntity.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }

            return base.SaveChanges();
        }
    }
}


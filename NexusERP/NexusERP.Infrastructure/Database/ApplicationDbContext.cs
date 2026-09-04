using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NexusERP.Application.Interfaces;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using Microsoft.EntityFrameworkCore.Design;

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
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserAbsence> UserAbsences { get; set; }
        public DbSet<SalaryRecord> SalaryRecords { get; set; }
        public DbSet<Store> Stores { get; set; }

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

                entity.HasOne(e => e.Store)
                    .WithMany(s => s.Transactions)
                    .HasForeignKey(e => e.StoreId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 3. Map Users
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.HasOne(e => e.Role)
                        .WithMany()
                        .HasForeignKey(e => e.RoleId)
                        .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.RoleId);
                entity.Property(e => e.Permissions)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>());
            });

            modelBuilder.Entity<UserAbsence>(entity =>
            {
                entity.HasKey(e => e.AbsenceId);

                entity.Property(e => e.Type)
                    .HasConversion(
                        v => v.ToString(),
                        v => (AbsenceType)Enum.Parse(typeof(AbsenceType), v));

                entity.Property(e => e.Status)
                    .HasConversion(
                        v => v.ToString(),
                        v => (AbsenceStatus)Enum.Parse(typeof(AbsenceStatus), v));

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Absences)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ReviewedBy)
                    .WithMany()
                    .HasForeignKey(e => e.ReviewedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<SalaryRecord>(entity =>
            {
                entity.HasKey(e => e.SalaryRecordId);

                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.User)
                      .WithMany(u => u.SalaryRecords)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Category>().HasKey(e => e.CategoryId);
            modelBuilder.Entity<Supplier>().HasKey(e => e.SupplierId);
            modelBuilder.Entity<SystemAuditLog>().HasKey(e => e.LogId);


            modelBuilder.Entity<SystemAuditLog>()
            .HasIndex(log => new { log.CreatedAt, log.LogId })
            .IsDescending(true, true);
            
            modelBuilder.Entity<InventoryTransaction>()
                .HasIndex(t => new { t.CreatedAt, t.TransactionId })
                .IsDescending(true, true);

            modelBuilder.Entity<Store>(entity =>
            {
                entity.HasKey(e => e.StoreId);
                entity.HasQueryFilter(e => e.IsActive);

                entity.Property(e => e.Location)
                .HasColumnType("geography");
            });
        }


        private void UpdateAuditFields()
        {
            var entries = ChangeTracker.Entries();

            foreach (var entry in entries)
            {
                if (entry.Entity is ICreationTracked createdEntity &&
                    entry.State == EntityState.Added)
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
                        entry.Property(nameof(ICreationTracked.CreatedAt))
                             .IsModified = false;

                        auditableEntity.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }
        }

        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}


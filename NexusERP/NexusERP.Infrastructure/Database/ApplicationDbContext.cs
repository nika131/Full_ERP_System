using Microsoft.EntityFrameworkCore;
using NexusERP.Application.Interfaces;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using System.Text.Json;
using static NexusERP.Domain.Enums.ProductEnums;

namespace NexusERP.Infrastructure.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options
        ) : base(options)
        {
        }

        public DbSet<Company> Companies { get; set; }

        public DbSet<Product> Products { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }

        public DbSet<UserAbsence> UserAbsences { get; set; }
        public DbSet<SalaryRecord> SalaryRecords { get; set; }

        public DbSet<SystemAuditLog> SystemAuditLogs { get; set; }

        public DbSet<Store> Stores { get; set; }

        public DbSet<CashMovement> CashMovements { get; set; }
        public DbSet<Shift> Shifts { get; set; }

        public DbSet<Receipt> Receipts { get; set; }
        public DbSet<ReceiptItem> ReceiptItems { get; set; }

        public DbSet<SystemSetting> SystemSettings { get; set; }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<AuthSession> AuthSessions { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================================================
            // SOFT DELETE QUERY FILTERS
            // ============================================================

            modelBuilder.Entity<Company>()
                .HasQueryFilter(c => c.IsActive);

            modelBuilder.Entity<Category>()
                .HasQueryFilter(c => c.IsActive);

            modelBuilder.Entity<Product>()
                .HasQueryFilter(p => p.IsActive);

            modelBuilder.Entity<Supplier>()
                .HasQueryFilter(s => s.IsActive);

            modelBuilder.Entity<User>()
                .HasQueryFilter(u => u.IsActive);

            modelBuilder.Entity<Store>()
                .HasQueryFilter(s => s.IsActive);

            // ============================================================
            // COMPANY
            // ============================================================

            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasKey(e => e.CompanyId);

                entity.Property(e => e.Name)
                    .IsRequired();

                entity.HasIndex(e => e.JoinCodeHash)
                    .IsUnique()
                    .HasFilter("[JoinCodeHash] IS NOT NULL");
            });

            // ============================================================
            // USER
            // ============================================================

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.HasOne(e => e.Company)
                    .WithMany(c => c.Users)
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Account)
                    .WithMany(a => a.Users)
                    .HasForeignKey(e => e.AccountId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.CompanyId, e.AccountId })
                    .IsUnique()
                    .HasFilter("[AccountId] IS NOT NULL");

                entity.HasIndex(e => new
                {
                    e.CompanyId,
                    e.Username
                })
                .IsUnique();
            });

            // ============================================================
            // ROLE
            // ============================================================

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.RoleId);

                entity.HasOne(e => e.Company)
                    .WithMany(c => c.Roles)
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new
                {
                    e.CompanyId,
                    e.Name
                })
                .IsUnique();

                entity.Property(e => e.Permissions)
                    .HasConversion(
                        v => JsonSerializer.Serialize(
                            v,
                            (JsonSerializerOptions?)null
                        ),
                        v => JsonSerializer.Deserialize<List<string>>(
                            v,
                            (JsonSerializerOptions?)null
                        ) ?? new List<string>()
                    );
            });

            // ============================================================
            // PRODUCT
            // ============================================================

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductId);

                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.CompanyId);

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.CostPrice)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.VatRate)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.MarketDiscountRate)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.MaxDiscountPercentage)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.DisplayMode)
                    .HasConversion(
                        v => v.ToString().ToLower(),
                        v => string.IsNullOrEmpty(v)
                            ? ProductDisplayMode.Image
                            : (ProductDisplayMode)Enum.Parse(
                                typeof(ProductDisplayMode),
                                v,
                                true
                            )
                    );
            });

            // ============================================================
            // CATEGORY
            // ============================================================

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.CategoryId);

                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.CompanyId);
            });

            // ============================================================
            // SUPPLIER
            // ============================================================

            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.HasKey(e => e.SupplierId);

                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.CompanyId);
            });

            // ============================================================
            // STORE
            // ============================================================

            modelBuilder.Entity<Store>(entity =>
            {
                entity.HasKey(e => e.StoreId);

                entity.HasOne(e => e.Company)
                    .WithMany(c => c.Stores)
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.CompanyId);

                entity.Property(e => e.Location)
                    .HasColumnType("geography");

                entity.Property(e => e.MaxCartDiscountPercentage)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.PosLayoutJson)
                    .HasColumnType("nvarchar(max)");
            });

            // ============================================================
            // INVENTORY TRANSACTION
            // ============================================================

            modelBuilder.Entity<InventoryTransaction>(entity =>
            {
                entity.HasKey(e => e.TransactionId);

                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.CompanyId);

                entity.Property(e => e.TransactionType)
                    .HasConversion(
                        v => v.ToString(),
                        v => (TransactionAction)Enum.Parse(
                            typeof(TransactionAction),
                            v
                        )
                    );

                entity.HasIndex(t => new
                {
                    t.CreatedAt,
                    t.TransactionId
                })
                .IsDescending(true, true);
            });

            // ============================================================
            // USER ABSENCE
            // ============================================================

            modelBuilder.Entity<UserAbsence>(entity =>
            {
                entity.HasKey(e => e.AbsenceId);

                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.CompanyId);

                entity.Property(e => e.Type)
                    .HasConversion(
                        v => v.ToString(),
                        v => (AbsenceType)Enum.Parse(
                            typeof(AbsenceType),
                            v
                        )
                    );

                entity.Property(e => e.Status)
                    .HasConversion(
                        v => v.ToString(),
                        v => (AbsenceStatus)Enum.Parse(
                            typeof(AbsenceStatus),
                            v
                        )
                    );

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Absences)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ReviewedBy)
                    .WithMany()
                    .HasForeignKey(e => e.ReviewedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ============================================================
            // SALARY RECORD
            // ============================================================

            modelBuilder.Entity<SalaryRecord>(entity =>
            {
                entity.HasKey(e => e.SalaryRecordId);

                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.CompanyId);

                entity.Property(e => e.Amount)
                    .HasColumnType("decimal(18,2)");
            });

            // ============================================================
            // SYSTEM AUDIT LOG
            // ============================================================

            modelBuilder.Entity<SystemAuditLog>(entity =>
            {
                entity.HasKey(e => e.LogId);

                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.CompanyId);

                entity.HasIndex(e => new
                {
                    e.CreatedAt,
                    e.LogId
                })
                .IsDescending(true, true);
            });

            // ============================================================
            // SHIFT
            // ============================================================

            modelBuilder.Entity<Shift>(entity =>
            {
                entity.HasKey(e => e.ShiftId);

                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.CompanyId);

                entity.Property(e => e.StartingCash)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.ExpectedEndingCash)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.ActualEndingCash)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TotalSales)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TotalProfit)
                    .HasColumnType("decimal(18,2)");
            });

            // ============================================================
            // CASH MOVEMENT
            // ============================================================

            modelBuilder.Entity<CashMovement>(entity =>
            {
                entity.HasKey(e => e.MovementId);

                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.CompanyId);

                entity.Property(e => e.Amount)
                    .HasColumnType("decimal(18,2)");
            });

            // ============================================================
            // RECEIPT
            // ============================================================

            modelBuilder.Entity<Receipt>(entity =>
            {
                entity.HasKey(e => e.ReceiptId);

                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.CompanyId);

                entity.Property(e => e.SubTotal)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.CartDiscountPercentage)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TotalVatAmount)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.FinalTotal)
                    .HasColumnType("decimal(18,2)");
            });

            // ============================================================
            // RECEIPT ITEM
            // ============================================================

            modelBuilder.Entity<ReceiptItem>(entity =>
            {
                entity.HasKey(e => e.ItemId);

                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.CompanyId);

                entity.Property(e => e.UnitPrice)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.CostPrice)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.VatRate)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.MarketDiscountPercentage)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.ManualItemDiscountPercentage)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.LineTotal)
                    .HasColumnType("decimal(18,2)");
            });

            // ============================================================
            // SYSTEM SETTINGS
            // ============================================================

            modelBuilder.Entity<SystemSetting>(entity =>
            {
                entity.HasKey(e => new
                {
                    e.CompanyId,
                    e.SettingKey
                });

                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.SettingKey)
                    .HasMaxLength(100);
            });


            // ACCOUNT
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(e => e.AccountId);

                entity.Property(e => e.Email).HasMaxLength(320).IsRequired();
                entity.Property(e => e.NormalizedEmail).HasMaxLength(320).IsRequired();

                entity.Property(e => e.GoogleSubject).HasMaxLength(255);

                entity.HasIndex(e => e.NormalizedEmail).IsUnique();

                entity.HasIndex(e => e.GoogleSubject)
                    .IsUnique()
                    .HasFilter("[GoogleSubject] IS NOT NULL");

                entity.Property(e => e.EmailVerificationCodeHash).HasMaxLength(64);
            });

            // AUTH SESSION
            modelBuilder.Entity<AuthSession>(entity =>
            {
                entity.HasKey(e => e.SessionId);

                entity.HasOne(e => e.Account)
                    .WithMany(a => a.Sessions)
                    .HasForeignKey(e => e.AccountId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.RefreshTokenHash)
                    .HasMaxLength(64)
                    .IsRequired();

                entity.Property(e => e.ClientType)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.HasIndex(e => e.RefreshTokenHash).IsUnique();
                entity.HasIndex(e => new { e.AccountId, e.CompanyId });
            });


            // ============================================================
            // DEFAULT DELETE BEHAVIOR
            // ============================================================

            // Ensure all non-owned relationships use Restrict unless an
            // explicit relationship configuration already specifies it.
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var foreignKey in entityType.GetForeignKeys())
                {
                    if (!foreignKey.IsOwnership)
                    {
                        foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
                    }
                }
            }
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
                        entry.Property(
                            nameof(ICreationTracked.CreatedAt)
                        ).IsModified = false;

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

            return base.SaveChangesAsync(
                cancellationToken
            );
        }
    }
}
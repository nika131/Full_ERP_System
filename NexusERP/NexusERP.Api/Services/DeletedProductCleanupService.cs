using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NexusERP.Infrastructure.Database;

namespace NexusERP.Api.Services
{
    // Catalog maintenance only: receipts and financial data are never deleted.
    public sealed class DeletedProductCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DeletedProductCleanupService> _logger;
        public DeletedProductCleanupService(IServiceScopeFactory scopeFactory,
            ILogger<DeletedProductCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try { await PurgeExpiredAsync(stoppingToken); }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
                catch (Exception ex) { _logger.LogError(ex, "Expired product cleanup failed"); }

                try { await Task.Delay(TimeSpan.FromHours(24), stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }

        private async Task PurgeExpiredAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var cutoff = DateTime.UtcNow.AddDays(-30);
            var strategy = db.Database.CreateExecutionStrategy();

            // Batch 100 products per transaction; retries are safe.
            while (!cancellationToken.IsCancellationRequested)
            {
                int count = await strategy.ExecuteAsync(async () =>
                {
                    await using var tx = await db.Database.BeginTransactionAsync(
                        System.Data.IsolationLevel.Serializable, cancellationToken);
                    var ids = await db.Products.IgnoreQueryFilters()
                        .Where(p => !p.IsActive && p.DeletedAt.HasValue && p.DeletedAt <= cutoff)
                        .OrderBy(p => p.ProductId).Select(p => p.ProductId).Take(100)
                        .ToArrayAsync(cancellationToken);
                    if (ids.Length == 0) { await tx.CommitAsync(cancellationToken); return 0; }

                    await db.ReceiptItems.IgnoreQueryFilters()
                        .Where(i => i.ProductId.HasValue && ids.Contains(i.ProductId.Value))
                        .ExecuteUpdateAsync(s => s.SetProperty(i => i.ProductId, (int?)null), cancellationToken);
                    await db.InventoryTransactions.IgnoreQueryFilters()
                        .Where(i => i.ProductId.HasValue && ids.Contains(i.ProductId.Value))
                        .ExecuteUpdateAsync(s => s.SetProperty(i => i.ProductId, (int?)null), cancellationToken);
                    await db.Products.IgnoreQueryFilters().Where(p => ids.Contains(p.ProductId))
                        .ExecuteDeleteAsync(cancellationToken);

                    await tx.CommitAsync(cancellationToken);
                    return ids.Length;
                });
                if (count == 0) break;
                _logger.LogInformation("Removed {Count} expired catalog products", count);
            }
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFullTextIndexToAuditLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF NOT EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = 'AuditLogCatalog') " +
                                 "CREATE FULLTEXT CATALOG AuditLogCatalog AS DEFAULT;", suppressTransaction: true);

            migrationBuilder.Sql("CREATE FULLTEXT INDEX ON SystemAuditLogs(" +
                                 "  Action LANGUAGE 1033, " +
                                 "  EntityType LANGUAGE 1033, " +
                                 ") KEY INDEX PK_SystemAuditLogs ON AuditLogCatalog;", suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FULLTEXT INDEX ON SystemAuditLogs;", suppressTransaction: true);
        }
    }
}

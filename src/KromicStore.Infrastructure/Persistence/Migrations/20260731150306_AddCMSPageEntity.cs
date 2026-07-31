using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KromicStore.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCMSPageEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_fulfillments_created",
                table: "fulfillments");

            migrationBuilder.DropColumn(
                name: "CreatedOnUtc",
                table: "Wishlists");

            migrationBuilder.DropColumn(
                name: "ModifiedOnUtc",
                table: "Wishlists");

            migrationBuilder.DropColumn(
                name: "CreatedOnUtc",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ModifiedOnUtc",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CreatedOnUtc",
                table: "ProductCollections");

            migrationBuilder.DropColumn(
                name: "ModifiedOnUtc",
                table: "ProductCollections");

            migrationBuilder.DropColumn(
                name: "ModifiedOnUtc",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "created_at_utc",
                table: "fulfillments");

            migrationBuilder.DropColumn(
                name: "ModifiedOnUtc",
                table: "CheckoutSessions");

            migrationBuilder.DropColumn(
                name: "CreatedOnUtc",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "ModifiedOnUtc",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "CreatedOnUtc",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "ModifiedOnUtc",
                table: "Carts");

            migrationBuilder.RenameColumn(
                name: "CreatedOnUtc",
                table: "CheckoutSessions",
                newName: "CreatedAtUtc");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:hstore", ",,");

            migrationBuilder.AddColumn<int>(
                name: "ApiCallsUsedToday",
                schema: "public",
                table: "Tenants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EmailsSentThisMonth",
                schema: "public",
                table: "Tenants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsOnTrial",
                schema: "public",
                table: "Tenants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyRecurringRevenue",
                schema: "public",
                table: "Tenants",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "StorageUsedBytes",
                schema: "public",
                table: "Tenants",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionEndDate",
                schema: "public",
                table: "Tenants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubscriptionPlanId",
                schema: "public",
                table: "Tenants",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionStartDate",
                schema: "public",
                table: "Tenants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalRevenue",
                schema: "public",
                table: "Tenants",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "TrialEndsOn",
                schema: "public",
                table: "Tenants",
                type: "timestamp with time zone",
                nullable: true);

            // Raw SQL for PostgreSQL to properly cast hstore to jsonb
            migrationBuilder.Sql("ALTER TABLE public.\"email_outbox\" ALTER COLUMN \"TemplateVariables\" TYPE jsonb USING CASE WHEN \"TemplateVariables\" IS NOT NULL THEN jsonb(hstore_to_json(\"TemplateVariables\")) ELSE NULL END;");

            // Raw SQL for PostgreSQL to properly cast hstore to jsonb
            migrationBuilder.Sql("ALTER TABLE public.\"email_outbox\" ALTER COLUMN \"CustomHeaders\" TYPE jsonb USING CASE WHEN \"CustomHeaders\" IS NOT NULL THEN jsonb(hstore_to_json(\"CustomHeaders\")) ELSE NULL END;");

            migrationBuilder.CreateTable(
                name: "CMSPageSet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    MetaDescription = table.Column<string>(type: "text", nullable: true),
                    MetaKeywords = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PublishedDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ScheduledPublishDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CMSPageSet", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_fulfillments_created",
                table: "fulfillments",
                column: "created_on_utc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CMSPageSet");

            migrationBuilder.DropIndex(
                name: "idx_fulfillments_created",
                table: "fulfillments");

            migrationBuilder.DropColumn(
                name: "ApiCallsUsedToday",
                schema: "public",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "EmailsSentThisMonth",
                schema: "public",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "IsOnTrial",
                schema: "public",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "MonthlyRecurringRevenue",
                schema: "public",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "StorageUsedBytes",
                schema: "public",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "SubscriptionEndDate",
                schema: "public",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "SubscriptionPlanId",
                schema: "public",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "SubscriptionStartDate",
                schema: "public",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "TotalRevenue",
                schema: "public",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "TrialEndsOn",
                schema: "public",
                table: "Tenants");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "CheckoutSessions",
                newName: "CreatedOnUtc");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:hstore", ",,");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOnUtc",
                table: "Wishlists",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedOnUtc",
                table: "Wishlists",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOnUtc",
                table: "Products",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedOnUtc",
                table: "Products",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOnUtc",
                table: "ProductCollections",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedOnUtc",
                table: "ProductCollections",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedOnUtc",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at_utc",
                table: "fulfillments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Dictionary<string, string>>(
                name: "TemplateVariables",
                schema: "public",
                table: "email_outbox",
                type: "hstore",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<Dictionary<string, string>>(
                name: "CustomHeaders",
                schema: "public",
                table: "email_outbox",
                type: "hstore",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedOnUtc",
                table: "CheckoutSessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOnUtc",
                table: "Categories",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedOnUtc",
                table: "Categories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOnUtc",
                table: "Carts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedOnUtc",
                table: "Carts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_fulfillments_created",
                table: "fulfillments",
                column: "created_at_utc");
        }
    }
}

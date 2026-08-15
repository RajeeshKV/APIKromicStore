using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KromicStore.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedRolesAndAssignUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ──────────────────────────────────────────────────────────────────────────
            // ROLE SEEDING
            // ──────────────────────────────────────────────────────────────────────────
            // Seed 4 roles: SuperAdmin, TenantAdmin, StoreManager, Customer
            // These are the only roles in the system.
            // Use hardcoded GUIDs for consistency across environments.
            
            const string superAdminRoleId = "10000000-0000-0000-0000-000000000001";
            const string tenantAdminRoleId = "10000000-0000-0000-0000-000000000002";
            const string storeManagerRoleId = "10000000-0000-0000-0000-000000000003";
            const string customerRoleId = "10000000-0000-0000-0000-000000000004";

            var now = DateTime.UtcNow;
            
            migrationBuilder.Sql($@"
                INSERT INTO ""Roles"" (""Id"", ""Name"", ""CreatedOnUtc"", ""CreatedBy"", ""IsDeleted"") 
                VALUES 
                    ('{superAdminRoleId}', 'SuperAdmin', '{now:O}', 'System', false),
                    ('{tenantAdminRoleId}', 'TenantAdmin', '{now:O}', 'System', false),
                    ('{storeManagerRoleId}', 'StoreManager', '{now:O}', 'System', false),
                    ('{customerRoleId}', 'Customer', '{now:O}', 'System', false)
                ON CONFLICT DO NOTHING;
            ");

            // ──────────────────────────────────────────────────────────────────────────
            // USER ROLE ASSIGNMENT
            // ──────────────────────────────────────────────────────────────────────────
            // Assign all existing users to TenantAdmin role
            // (This is the default role for users with a TenantId)
            
            migrationBuilder.Sql($@"
                INSERT INTO ""UserRoles"" (""UserId"", ""RoleId"")
                SELECT ""Id"", '{tenantAdminRoleId}'
                FROM ""Users""
                WHERE ""IsDeleted"" = false
                AND ""Id"" NOT IN (SELECT ""UserId"" FROM ""UserRoles"")
                ON CONFLICT DO NOTHING;
            ");

            migrationBuilder.CreateTable(
                name: "AuditLogSet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    EntityType = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    EntityName = table.Column<string>(type: "text", nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorEmail = table.Column<string>(type: "text", nullable: true),
                    OccurredOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: true),
                    OldValues = table.Column<string>(type: "text", nullable: true),
                    NewValues = table.Column<string>(type: "text", nullable: true),
                    CorrelationId = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    IsSearchIndexed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogSet", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContactRequestSet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    Subject = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: true),
                    ReceivedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    InternalNotes = table.Column<string>(type: "text", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactRequestSet", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeatureFlagSet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Scope = table.Column<int>(type: "integer", nullable: false),
                    ConfigurationJson = table.Column<string>(type: "text", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureFlagSet", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlatformSettingsSet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlatformName = table.Column<string>(type: "text", nullable: false),
                    LogoUrl = table.Column<string>(type: "text", nullable: true),
                    FaviconUrl = table.Column<string>(type: "text", nullable: true),
                    SupportEmail = table.Column<string>(type: "text", nullable: false),
                    SupportPhoneNumber = table.Column<string>(type: "text", nullable: true),
                    ContactFormEmail = table.Column<string>(type: "text", nullable: true),
                    LandingPageUrl = table.Column<string>(type: "text", nullable: true),
                    FooterContent = table.Column<string>(type: "text", nullable: true),
                    PrivacyPolicyUrl = table.Column<string>(type: "text", nullable: true),
                    TermsOfServiceUrl = table.Column<string>(type: "text", nullable: true),
                    DefaultCurrency = table.Column<string>(type: "text", nullable: false),
                    DefaultTimezone = table.Column<string>(type: "text", nullable: false),
                    SmtpHost = table.Column<string>(type: "text", nullable: true),
                    SmtpPort = table.Column<int>(type: "integer", nullable: true),
                    SmtpUsername = table.Column<string>(type: "text", nullable: true),
                    SmtpPassword = table.Column<string>(type: "text", nullable: true),
                    SmtpUseSsl = table.Column<bool>(type: "boolean", nullable: false),
                    SmtpFromAddress = table.Column<string>(type: "text", nullable: true),
                    SmtpFromName = table.Column<string>(type: "text", nullable: true),
                    WelcomeEmailTemplate = table.Column<string>(type: "text", nullable: true),
                    ResetPasswordEmailTemplate = table.Column<string>(type: "text", nullable: true),
                    MaintenanceNoticeEmailTemplate = table.Column<string>(type: "text", nullable: true),
                    CloudinaryCloudName = table.Column<string>(type: "text", nullable: true),
                    CloudinaryApiKey = table.Column<string>(type: "text", nullable: true),
                    RazorpayKeyId = table.Column<string>(type: "text", nullable: true),
                    RazorpayEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    StripeEnabled = table.Column<bool>(type: "boolean", nullable: true),
                    PayPalEnabled = table.Column<bool>(type: "boolean", nullable: true),
                    MaintenanceMode = table.Column<bool>(type: "boolean", nullable: false),
                    MaintenanceMessage = table.Column<string>(type: "text", nullable: true),
                    AllowNewTenantSignups = table.Column<bool>(type: "boolean", nullable: false),
                    AllowTrialSignups = table.Column<bool>(type: "boolean", nullable: false),
                    RequireEmailVerification = table.Column<bool>(type: "boolean", nullable: false),
                    RequireManualApproval = table.Column<bool>(type: "boolean", nullable: false),
                    MaxTenantsPerPlatform = table.Column<int>(type: "integer", nullable: false),
                    MaxFreeTrialDays = table.Column<int>(type: "integer", nullable: false),
                    MinimumMonthlyPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    EnableAnalytics = table.Column<bool>(type: "boolean", nullable: false),
                    EnablePerformanceMonitoring = table.Column<bool>(type: "boolean", nullable: false),
                    EnableErrorTracking = table.Column<bool>(type: "boolean", nullable: false),
                    AnalyticsRetentionDays = table.Column<int>(type: "integer", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformSettingsSet", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlanSet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    MonthlyPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    AnnualPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    TrialPricePerDay = table.Column<decimal>(type: "numeric", nullable: true),
                    TrialDays = table.Column<int>(type: "integer", nullable: false),
                    MaxProducts = table.Column<int>(type: "integer", nullable: false),
                    MaxCategories = table.Column<int>(type: "integer", nullable: false),
                    MaxCollections = table.Column<int>(type: "integer", nullable: false),
                    MaxStaff = table.Column<int>(type: "integer", nullable: false),
                    MaxCustomers = table.Column<int>(type: "integer", nullable: false),
                    MaxStorageBytes = table.Column<long>(type: "bigint", nullable: false),
                    MaxEmailsPerMonth = table.Column<int>(type: "integer", nullable: false),
                    MaxApiCallsPerDay = table.Column<int>(type: "integer", nullable: false),
                    CanCustomizeDomain = table.Column<bool>(type: "boolean", nullable: false),
                    CanUseThemes = table.Column<bool>(type: "boolean", nullable: false),
                    CanUseCustomTheme = table.Column<bool>(type: "boolean", nullable: false),
                    CanUsePaymentGateway = table.Column<bool>(type: "boolean", nullable: false),
                    CanUseAdvancedReporting = table.Column<bool>(type: "boolean", nullable: false),
                    CanUseAnalytics = table.Column<bool>(type: "boolean", nullable: false),
                    CanUseEmailMarketing = table.Column<bool>(type: "boolean", nullable: false),
                    CanUseSeo = table.Column<bool>(type: "boolean", nullable: false),
                    CanUseMultipleCurrencies = table.Column<bool>(type: "boolean", nullable: false),
                    CanUseAdvancedInventory = table.Column<bool>(type: "boolean", nullable: false),
                    CanUseWebhooks = table.Column<bool>(type: "boolean", nullable: false),
                    CanUsePrioritySupportEmail = table.Column<bool>(type: "boolean", nullable: false),
                    CanUsePrioritySupportPhone = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsTrial = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlanSet", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ThemeSet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    PreviewImageUrl = table.Column<string>(type: "text", nullable: true),
                    ThumbnailImageUrl = table.Column<string>(type: "text", nullable: true),
                    ConfigurationSchema = table.Column<string>(type: "text", nullable: true),
                    DefaultConfiguration = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    PublishedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TimesUsed = table.Column<int>(type: "integer", nullable: false),
                    CurrentVersion = table.Column<int>(type: "integer", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThemeSet", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContactRequestReply",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContactRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    RepliedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    WasEmailSentToRequester = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactRequestReply", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactRequestReply_ContactRequestSet_ContactRequestId",
                        column: x => x.ContactRequestId,
                        principalTable: "ContactRequestSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeatureFlagAssignment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FeatureFlagId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentType = table.Column<int>(type: "integer", nullable: false),
                    AssignedToEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AssignedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureFlagAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeatureFlagAssignment_FeatureFlagSet_FeatureFlagId",
                        column: x => x.FeatureFlagId,
                        principalTable: "FeatureFlagSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ThemeAssets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ThemeId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StoragePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    AssetType = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    PublicUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThemeAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThemeAssets_ThemeSet_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "ThemeSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ThemeVersion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ThemeId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    ChangesSummary = table.Column<string>(type: "text", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThemeVersion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThemeVersion_ThemeSet_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "ThemeSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactRequestReply_ContactRequestId",
                table: "ContactRequestReply",
                column: "ContactRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureFlagAssignment_FeatureFlagId",
                table: "FeatureFlagAssignment",
                column: "FeatureFlagId");

            migrationBuilder.CreateIndex(
                name: "IX_ThemeAssets_AssetType",
                table: "ThemeAssets",
                column: "AssetType");

            migrationBuilder.CreateIndex(
                name: "IX_ThemeAssets_IsActive",
                table: "ThemeAssets",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ThemeAssets_ThemeId",
                table: "ThemeAssets",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_ThemeAssets_ThemeId_AssetType",
                table: "ThemeAssets",
                columns: new[] { "ThemeId", "AssetType" });

            migrationBuilder.CreateIndex(
                name: "IX_ThemeVersion_ThemeId",
                table: "ThemeVersion",
                column: "ThemeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogSet");

            migrationBuilder.DropTable(
                name: "ContactRequestReply");

            migrationBuilder.DropTable(
                name: "FeatureFlagAssignment");

            migrationBuilder.DropTable(
                name: "PlatformSettingsSet");

            migrationBuilder.DropTable(
                name: "SubscriptionPlanSet");

            migrationBuilder.DropTable(
                name: "ThemeAssets");

            migrationBuilder.DropTable(
                name: "ThemeVersion");

            migrationBuilder.DropTable(
                name: "ContactRequestSet");

            migrationBuilder.DropTable(
                name: "FeatureFlagSet");

            migrationBuilder.DropTable(
                name: "ThemeSet");
        }
    }
}

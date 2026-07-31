using KromicStore.API.DependencyInjection;
using KromicStore.Application;
using KromicStore.Infrastructure;
using KromicStore.Infrastructure.Configuration;
using KromicStore.Infrastructure.Health;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.AddSerilogLogging();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

// Add Application layer
builder.Services.AddApplication();

// Add Infrastructure layer
builder.Services.AddInfrastructure(builder.Configuration);

// Add Authentication
builder.Services.AddAuthenticationServices(builder.Configuration);

// Add Swagger
builder.Services.AddSwaggerGen();

// Add and configure Health Checks
builder.Services
    .AddHealthChecks()
    .AddCheck<TenantResolutionHealthCheck>("Tenant Resolution", tags: new[] { "startup" })
    .AddCheck<BrevoHealthCheck>("Brevo Email Service", tags: new[] { "external" })
    .AddCheck<CloudinaryHealthCheck>("Cloudinary Media Service", tags: new[] { "external" })
    .AddCheck<RazorpayHealthCheck>("Razorpay Payment Gateway", tags: new[] { "external" });

// Register background services
builder.Services.AddHostedService<KromicStore.Infrastructure.BackgroundJobs.EmailOutboxBackgroundWorker>();

var app = builder.Build();

// Validate platform configuration during startup
var configValidator = app.Services.GetRequiredService<PlatformConfigurationValidator>();
configValidator.ValidateAndLog();

// Mark application as initialized after startup
var startupState = app.Services.GetRequiredService<ApplicationStartupState>();
startupState.MarkDependencyInjectionReady();
startupState.MarkInitialized();

// Configure middleware pipeline
    app.UseSwagger();
    app.UseSwaggerUI();

// Use custom API middleware
app.UseApiMiddleware();

// Map endpoints
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

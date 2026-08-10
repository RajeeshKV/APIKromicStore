using KromicStore.API.DependencyInjection;
using KromicStore.Application;
using KromicStore.Infrastructure;
using KromicStore.Infrastructure.Configuration;
using KromicStore.Infrastructure.Extensions;
using KromicStore.Infrastructure.Health;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Disable file watching in production to prevent inotify limit errors on small instances
// (Render's free tier has limited file descriptors - this prevents IOException)
if (builder.Environment.IsProduction())
{
    builder.Host.UseDefaultServiceProvider(options => 
    {
        options.ValidateScopes = false;
        options.ValidateOnBuild = false;
    });
}

// Add Serilog
builder.AddSerilogLogging();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

// Configure CORS
builder.Services.Configure<CorsOptions>(builder.Configuration.GetSection(CorsOptions.SectionName));
builder.Services.AddCors(options =>
{
    var corsOptions = new CorsOptions();
    builder.Configuration.GetSection(CorsOptions.SectionName).Bind(corsOptions);
    
    var (isValid, errorMessage) = corsOptions.Validate();
    if (!isValid)
    {
        throw new InvalidOperationException($"Invalid CORS configuration: {errorMessage}");
    }

    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        var allowedOrigins = corsOptions.ParsedAllowedOrigins.ToArray();
        
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Add Application layer
builder.Services.AddApplication();

// Add Infrastructure layer
builder.Services.AddInfrastructure(builder.Configuration);

// Configure database migration options
builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection(DatabaseOptions.SectionName));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<DatabaseOptions>>().Value);

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

// Apply database migrations before starting the application
// This ensures the database schema is up-to-date before any requests are processed
await app.Services.ApplyMigrationsAsync();

// Map endpoints
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

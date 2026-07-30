using KromicStore.API.DependencyInjection;
using KromicStore.Application;
using KromicStore.Infrastructure;

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

// Add Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use custom API middleware
app.UseApiMiddleware();

// Map endpoints
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

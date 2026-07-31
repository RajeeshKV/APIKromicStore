# Docker & Startup Migration Deployment Guide

## Overview

The KromicStore Backend is configured for production deployment on **Render** (or any Docker-compatible platform) with automatic EF Core database migrations during startup.

**Key Features:**
- ✅ Multi-stage Docker build (optimized image size)
- ✅ Automatic database migrations on startup
- ✅ Production-ready error handling
- ✅ Comprehensive startup logging
- ✅ Health checks with migration safety
- ✅ No EF CLI required in runtime container

---

## Architecture

### Startup Flow

```
Container Starts
    ↓
entrypoint.sh (Environment Validation)
    ↓
Program.cs Executes
    ↓
ApplyMigrationsAsync()
    ├─ Validates DatabaseOptions
    ├─ Checks Pending Migrations
    ├─ Applies Pending Migrations
    └─ Logs Every Step
    ↓
Application Begins Accepting Requests
    ↓
Health Endpoint Available (/api/v1/health)
```

### Configuration Layers

```
appsettings.json (Default Settings)
    ├─ Database.ApplyMigrationsOnStartup: true
    ├─ Database.MigrationTimeoutSeconds: 300
    └─ Database.ContinueOnMigrationFailure: false
        ↓
Environment Variables (Production Overrides)
    ├─ ConnectionStrings__DefaultConnection
    ├─ ASPNETCORE_ENVIRONMENT
    └─ ASPNETCORE_URLS
```

---

## Docker Image

### Build Specification

**Multi-Stage Build:**
1. **Builder Stage** - Uses `mcr.microsoft.com/dotnet/sdk:8.0`
   - Restores dependencies
   - Builds application
   - Publishes release artifacts
   - Size: ~1GB (not included in runtime image)

2. **Runtime Stage** - Uses `mcr.microsoft.com/dotnet/aspnet:8.0-alpine`
   - Minimal base image (~200MB)
   - Only published application
   - curl for health checks
   - ca-certificates for HTTPS
   - entrypoint.sh for startup validation
   - **Final Size: ~250-350MB**

### Build Command

```bash
docker build -f Dockerfile.prod -t kromic-store-api:latest .
```

### Image Contents

```
Runtime Image Contents:
├─ /app/
│  ├─ KromicStore.API.dll
│  ├─ KromicStore.Application.dll
│  ├─ KromicStore.Domain.dll
│  ├─ KromicStore.Infrastructure.dll
│  └─ appsettings.json
├─ /entrypoint.sh
└─ /bin (curl, dotnet runtime)
```

---

## Environment Variables

### Required Variables

```bash
# Database Connection (PostgreSQL)
ConnectionStrings__DefaultConnection=Server=<host>;Port=5432;Database=<db>;User Id=<user>;Password=<pwd>;

# Environment
ASPNETCORE_ENVIRONMENT=Production

# Listen Port
ASPNETCORE_URLS=http://+:8080
```

### Optional Variables

```bash
# Migration Timeout (seconds, default: 300)
# WARNING: Increase only if migrations are long-running
Database__MigrationTimeoutSeconds=600

# Continue on Migration Failure (default: false)
# ONLY SET TO TRUE if you have alternative migration strategies
Database__ContinueOnMigrationFailure=false

# Disable Migrations on Startup (default: true)
# Set to false to skip migrations
Database__ApplyMigrationsOnStartup=true
```

### External Service Variables

```bash
# Brevo Email Service
Brevo__ApiKey=<api_key>
Brevo__WebhookSecret=<webhook_secret>
Brevo__Enabled=true

# Cloudinary Media Service
Cloudinary__CloudName=<cloud_name>
Cloudinary__ApiKey=<api_key>
Cloudinary__ApiSecret=<api_secret>
Cloudinary__Enabled=true

# Razorpay Payment Gateway
Razorpay__KeyId=<key_id>
Razorpay__KeySecret=<key_secret>
Razorpay__WebhookSecret=<webhook_secret>
Razorpay__Enabled=true

# JWT Configuration
Jwt__Secret=<secret_key_min_32_chars>
```

---

## Deployment to Render

### 1. Setup PostgreSQL Database

```bash
# On Render:
# 1. Create PostgreSQL database
# 2. Copy connection string
# 3. Note the credentials
```

### 2. Deploy Application

**Via Render Dashboard:**

1. Create new Web Service
2. Connect GitHub repository
3. Set build command:
   ```
   dotnet restore && dotnet build -c Release
   ```
4. Set start command:
   ```
   # Leave empty - Dockerfile will be used
   ```
5. Select `Dockerfile.prod` as Dockerfile

**Environment Variables:**

```
ConnectionStrings__DefaultConnection=postgresql://<user>:<password>@<host>:5432/<database>
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
Jwt__Secret=<your-secret-key-min-32-chars>
Brevo__Enabled=false  # Enable when configured
Cloudinary__Enabled=false  # Enable when configured
Razorpay__Enabled=false  # Enable when configured
```

### 3. Verify Deployment

```bash
# Render provides logs automatically
# Look for:
# - "[STARTUP] Validating environment..."
# - "Checking for pending database migrations..."
# - "Database is up-to-date. No pending migrations found."
# - "Starting KromicStore.API application..."
```

---

## Local Testing

### Build & Run

```bash
# Build image
docker build -f Dockerfile.prod -t kromic-store-api:latest .

# Run container
docker run -d \
  --name kromic-store-api \
  -e "ASPNETCORE_ENVIRONMENT=Production" \
  -e "ASPNETCORE_URLS=http://+:8080" \
  -e "ConnectionStrings__DefaultConnection=Server=host.docker.internal;Port=5432;Database=kromic;User Id=postgres;Password=postgres;" \
  -p 8080:8080 \
  kromic-store-api:latest

# Check logs
docker logs -f kromic-store-api

# Test health endpoint
curl http://localhost:8080/api/v1/health

# Stop container
docker stop kromic-store-api
docker rm kromic-store-api
```

### Validation Script

```bash
# Run validation tests
chmod +x docker-validate.sh
./docker-validate.sh
```

Validates:
- ✅ Docker builds successfully
- ✅ Container starts without errors
- ✅ Application responds to health checks
- ✅ Migrations execute (if pending)
- ✅ Health endpoint returns 200 OK

---

## Migration Execution Details

### Startup Process

**File: `src/KromicStore.Infrastructure/Extensions/DatabaseExtensions.cs`**

The `ApplyMigrationsAsync()` method:

1. **Gets Configuration**
   ```csharp
   var options = serviceProvider.GetRequiredService<DatabaseOptions>();
   options.Validate();
   ```

2. **Checks If Enabled**
   ```csharp
   if (!options.ApplyMigrationsOnStartup) return;
   ```

3. **Resolves DbContext**
   ```csharp
   var dbContext = serviceProvider.GetRequiredService<KromicStoreDbContext>();
   ```

4. **Detects Pending Migrations**
   ```csharp
   var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
   ```

5. **Applies Pending Migrations**
   ```csharp
   await dbContext.Database.MigrateAsync(cts.Token);
   ```

6. **Handles Errors**
   - Timeout: Logs critical, stops startup (or continues if configured)
   - Schema Error: Logs critical, stops startup (or continues if configured)

### Logging

Every migration step is logged:

```
[STARTUP] Checking for pending database migrations...
[INFO] Found 3 pending migration(s):
       - 20260101000000_AddTenantEntity
       - 20260102000000_AddSubscriptionPlan
       - 20260103000000_AddFeatureFlags
[INFO] Applying pending migrations with timeout of 300 seconds...
[INFO] All pending migrations applied successfully
```

---

## Troubleshooting

### Issue: Container Exits Immediately

**Cause:** Usually database connection error or migration failure

**Solution:**
```bash
docker logs <container-id>
```

Check for:
- `ConnectionStrings__DefaultConnection` not set
- Database unreachable
- Schema incompatibility

**Fix:**
1. Verify environment variables
2. Verify database is accessible
3. Check migration compatibility

### Issue: Migrations Timeout

**Cause:** Long-running migrations or slow database

**Solution:**
```bash
# Increase timeout (set to 600 seconds)
Database__MigrationTimeoutSeconds=600
```

Or:
```bash
# Apply migrations manually before deployment
dotnet ef database update --project src/KromicStore.Infrastructure

# Then disable automatic migrations
Database__ApplyMigrationsOnStartup=false
```

### Issue: Migration Fails with Schema Error

**Cause:** Existing data conflicts or corrupted schema

**Solution:**
1. **Preserve Data:** Modify migration to handle existing data
2. **Backup & Reset:** Backup production database, reset to last known good state
3. **Manual Migration:** Run `dotnet ef database update` locally to verify

### Issue: Health Check Fails

**Cause:** Application didn't start due to migration error

**Solution:**
```bash
# Check container logs
docker logs <container-id>

# Look for migration error messages
grep -i "error\|critical\|exception" <logs>
```

---

## Health Check Behavior

### /api/v1/health Endpoint

**Response:**
```json
{
  "status": "Healthy",
  "checks": {
    "Tenant Resolution": "Healthy",
    "Brevo Email Service": "Degraded",
    "Cloudinary Media Service": "Degraded",
    "Razorpay Payment Gateway": "Degraded"
  }
}
```

**Important:** Health endpoint only becomes available **after** migrations complete successfully.

**Timing:**
- Migration check: 40-60+ seconds (depends on migration duration)
- Health endpoint response: <100ms
- Render load balancer timeout: 30s

**Note:** Render's health check has a 30s timeout. If migrations take longer, configure in Render dashboard.

---

## Performance Considerations

### Image Size

```
Runtime Image Breakdown:
├─ .NET Runtime: ~120MB
├─ Alpine Linux Base: ~70MB
├─ Application Assemblies: ~20MB
└─ Dependencies: ~40MB
────────────────────────
   Total: ~250MB (compressed on registry)
```

### Startup Time

**First Deployment (With Migrations):**
- Container startup: 5-10s
- Migration execution: 30-120s (depends on migration complexity)
- Application ready: 40-130s

**Subsequent Deployments (No New Migrations):**
- Container startup: 5-10s
- Migration check: 2-3s
- Application ready: 10-15s

---

## Best Practices

### ✅ DO

- Set `ConnectionStrings__DefaultConnection` before deployment
- Use environment-specific configuration
- Monitor startup logs for migration status
- Test locally with `docker-validate.sh` before deployment
- Keep migrations reversible and compatible
- Use appropriate `MigrationTimeoutSeconds` value
- Store secrets in environment variables (not code)

### ❌ DON'T

- Set `ContinueOnMigrationFailure=true` without understanding risks
- Disable `ApplyMigrationsOnStartup` without manual process
- Use `Database.Migrate()` commands in application code
- Include EF CLI tools in runtime image
- Deploy without testing locally first
- Use root user in container (handled by base image)

---

## Verification Checklist

Before deploying to production:

- [ ] Build succeeds: `dotnet build -c Release`
- [ ] All tests pass: `dotnet test`
- [ ] Docker image builds: `docker build -f Dockerfile.prod .`
- [ ] Container starts: `docker run ...`
- [ ] Health endpoint responds: `curl http://localhost:8080/api/v1/health`
- [ ] Logs show migration check: `docker logs ...`
- [ ] No errors in startup logs
- [ ] Application accepts requests
- [ ] Database schema is current

---

## Configuration Examples

### Development (With EF Migrations)

```bash
Database__ApplyMigrationsOnStartup=true
Database__MigrationTimeoutSeconds=600
Database__ContinueOnMigrationFailure=false
```

### Staging (Pre-Applied Migrations)

```bash
Database__ApplyMigrationsOnStartup=false
Database__ContinueOnMigrationFailure=false
# Migrations applied via separate pipeline
```

### Production (Automatic Migrations)

```bash
Database__ApplyMigrationsOnStartup=true
Database__MigrationTimeoutSeconds=300
Database__ContinueOnMigrationFailure=false
```

---

## References

- **EF Core Documentation:** https://docs.microsoft.com/en-us/ef/core/
- **Docker Best Practices:** https://docs.docker.com/develop/develop-images/dockerfile_best-practices/
- **Render Deployment:** https://render.com/docs
- **ASP.NET Core in Docker:** https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/

---

## Support

For issues with:

- **Database Migrations:** Check `src/KromicStore.Infrastructure/Persistence/Migrations/`
- **Configuration:** Review `src/KromicStore.API/appsettings.json`
- **Startup Logic:** See `src/KromicStore.API/Program.cs`
- **Extension Methods:** Check `src/KromicStore.Infrastructure/Extensions/DatabaseExtensions.cs`

---

*Last Updated: 2026-07-31*  
*Version: 1.0*  
*Status: Production-Ready*

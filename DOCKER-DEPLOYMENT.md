# Docker and Deployment Guide

## Overview

KromicStore Backend is containerized for easy deployment across multiple platforms:
- **Local Development**: docker-compose with PostgreSQL
- **Production**: Render.com, Fly.io, or any Docker-compatible platform

## Local Development

### Prerequisites
- Docker Desktop installed
- .env file configured (or use defaults)

### Start Services

```bash
docker-compose up -d
```

This will:
1. Start PostgreSQL container (port 5432)
2. Build and start API container (port 5000)
3. Apply EF Core migrations automatically
4. Seed SuperUser if missing

### Verify Services

```bash
# Check API health
curl http://localhost:5000/api/v1/health

# Check database connection
docker-compose logs api | grep -i "database\|connected"

# Access Swagger UI
http://localhost:5000/swagger
```

### Stop Services

```bash
docker-compose down

# Also remove volumes (WARNING: deletes data)
docker-compose down -v
```

## Production Deployment (Render.com)

### Prerequisites
- Render account
- GitHub repository
- Environment variables configured in Render dashboard

### Deployment Steps

1. **Create Web Service**
   - Select Docker (from Dockerfile.prod)
   - Branch: main
   - Build Command: (empty - uses Dockerfile)
   - Start Command: (empty - uses ENTRYPOINT)

2. **Configure Environment Variables**
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ConnectionStrings__DefaultConnection=<Supabase PostgreSQL URL>
   Jwt__Secret=<random 32+ char string>
   Jwt__Issuer=KromicStore
   Jwt__Audience=KromicStore
   MultiTenancy__BaseDomain=<your-render-domain.onrender.com>
   Brevo__Enabled=true
   Brevo__ApiKey=<your-brevo-api-key>
   Brevo__BaseUrl=https://api.brevo.com
   Cloudinary__Enabled=true
   Cloudinary__CloudName=<your-cloudinary-cloud>
   Cloudinary__ApiKey=<your-cloudinary-api-key>
   Cloudinary__ApiSecret=<your-cloudinary-secret>
   Razorpay__Enabled=true
   Razorpay__KeyId=<your-razorpay-key>
   Razorpay__KeySecret=<your-razorpay-secret>
   ```

3. **Configure Health Check**
   - Health check path: `/api/v1/health`
   - Initial delay: 40s
   - Interval: 30s
   - Timeout: 10s

4. **Deploy**
   - Push to main branch
   - Render automatically builds and deploys

### Monitoring

```bash
# View logs
render logs <service-id>

# Check health
curl https://<your-service>.onrender.com/api/v1/health
```

## Production Deployment (Fly.io)

### Prerequisites
- Fly.io account
- fly CLI installed
- GitHub repository

### Setup

```bash
# Create fly.toml
fly launch --generate-name

# Set environment variables
fly secrets set ASPNETCORE_ENVIRONMENT=Production
fly secrets set ConnectionStrings__DefaultConnection=<PostgreSQL URL>
# ... etc
```

### Deploy

```bash
fly deploy
```

## Docker Image Details

### Multi-Stage Build

1. **Builder Stage**: SDK image with full build tools
   - Restore NuGet packages
   - Compile C# code
   - Publish application

2. **Runtime Stage**: Alpine-based ASP.NET Core image
   - Minimal image size (~150MB)
   - Security-hardened Linux
   - Production-optimized

### Image Optimization

- ✅ Debug symbols removed (`DebugType=none`)
- ✅ Single file publish disabled (better for Docker)
- ✅ Ready-to-run compilation (`PublishReadyToRun=true`)
- ✅ Alpine Linux for reduced image size
- ✅ Health check configured

## Environment Variables

### Required (Production)
- `ConnectionStrings__DefaultConnection`: PostgreSQL connection string
- `Jwt__Secret`: 32+ character random string
- `Jwt__Issuer`: Token issuer (e.g., "KromicStore")
- `Jwt__Audience`: Token audience (e.g., "KromicStore")

### Optional (External Services)
- `Brevo__Enabled`: true/false
- `Brevo__ApiKey`: Brevo email service API key
- `Brevo__BaseUrl`: Brevo API endpoint
- `Cloudinary__Enabled`: true/false
- `Cloudinary__CloudName`: Cloudinary cloud name
- `Cloudinary__ApiKey`: Cloudinary API key
- `Cloudinary__ApiSecret`: Cloudinary API secret
- `Razorpay__Enabled`: true/false
- `Razorpay__KeyId`: Razorpay merchant key ID
- `Razorpay__KeySecret`: Razorpay merchant key secret

### Optional (Infrastructure)
- `ASPNETCORE_ENVIRONMENT`: Development/Production (default: Production)
- `ASPNETCORE_URLS`: Server URL (default: http://+:8080)
- `SERILOG_LOGLEVEL`: Error/Warning/Information/Debug (default: Information)

## Database Migrations

Migrations are automatically applied on startup:

1. Connect to database
2. Check pending migrations
3. Apply any pending migrations
4. Log results

To manually apply migrations:

```bash
# Inside container
dotnet ef database update --project /app

# From development machine
dotnet ef database update \
  --project src/KromicStore.Infrastructure \
  --startup-project src/KromicStore.API
```

## Health Checks

The API provides multiple health check endpoints:

### GET /api/v1/health
Returns detailed JSON with service status:
```json
{
  "status": "Healthy",
  "timestamp": "2026-07-31T12:00:00Z",
  "version": "1.0.0",
  "environment": "Production",
  "services": [
    {
      "name": "Database",
      "status": "Healthy",
      "duration": 50
    }
  ]
}
```

### HEAD /api/v1/health
Returns only status code (no body):
- 200: Healthy/Degraded
- 503: Unhealthy

Used by Docker, Kubernetes, load balancers for lightweight checks.

## Troubleshooting

### Container won't start
```bash
docker-compose logs api
# Check: ConnectionStrings__DefaultConnection is valid
# Check: Database is accessible
# Check: Ports 5000 (api) and 5432 (db) are available
```

### Health check failing
```bash
curl -v http://localhost:5000/api/v1/health
# Check: API container is running
# Check: Swagger UI works: http://localhost:5000/swagger
```

### Database connection errors
```bash
docker-compose logs postgres
# Check: PostgreSQL container is healthy
# Check: Database name and credentials match
```

### Migrations not applied
```bash
docker-compose exec api dotnet ef migrations list
# Check: All migrations are listed
# Check: Database schema matches latest migration
```

## Security Best Practices

✅ **Implemented**
- Non-root user (ASP.NET Core default)
- Alpine Linux for minimal surface area
- Health checks configured
- Environment variables for secrets
- Closed unused ports

✅ **Recommended**
- Use Docker Secrets or managed secret service
- Configure HTTPS/TLS in production
- Set resource limits (CPU, memory)
- Run health checks regularly
- Monitor container logs
- Enable audit logging

## Performance

### Image Size
- Builder stage: ~800MB (not deployed)
- Runtime stage: ~150MB (deployed)

### Startup Time
- Container start: <5s
- Health check initial: 40s
- Total ready: ~45s

### Resource Requirements
- CPU: 0.25-0.5 vCPU recommended
- Memory: 256MB-512MB recommended
- Storage: 2GB for image + database

## References

- [Docker Documentation](https://docs.docker.com)
- [ASP.NET Core in Docker](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/docker-application-architecture)
- [Render.com Docs](https://render.com/docs)
- [Fly.io Docs](https://fly.io/docs)

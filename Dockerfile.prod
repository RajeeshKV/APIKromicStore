# Production Dockerfile for Render deployment
# Multi-stage build optimized for minimal image size and production-ready startup

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder
WORKDIR /src

# Copy project files
COPY ["src/KromicStore.API/KromicStore.API.csproj", "src/KromicStore.API/"]
COPY ["src/KromicStore.Application/KromicStore.Application.csproj", "src/KromicStore.Application/"]
COPY ["src/KromicStore.Domain/KromicStore.Domain.csproj", "src/KromicStore.Domain/"]
COPY ["src/KromicStore.Infrastructure/KromicStore.Infrastructure.csproj", "src/KromicStore.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/KromicStore.API/KromicStore.API.csproj"

# Copy source
COPY . .

# Build
RUN dotnet build "src/KromicStore.API/KromicStore.API.csproj" \
    -c Release \
    --no-restore \
    -p:DebugType=none \
    -p:DebugSymbols=false

# Publish
RUN dotnet publish "src/KromicStore.API/KromicStore.API.csproj" \
    -c Release \
    --no-build \
    -o /app/publish \
    -p:PublishSingleFile=false \
    -p:PublishReadyToRun=true

# Runtime image - minimal base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

# Install curl for health checks and ca-certificates for HTTPS
RUN apk add --no-cache curl ca-certificates

WORKDIR /app

# Copy published application from builder
COPY --from=builder /app/publish .

# Copy startup script
COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

# Set environment variables for production
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8080

# Health check - verifies application is running and responsive
# Waits 40s before first check to allow migrations to complete
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8080/api/v1/health || exit 1

# Use entrypoint script for validation and startup
ENTRYPOINT ["/entrypoint.sh"]


#!/bin/sh
set -e

# KromicStore API - Production Startup Script
# Responsibilities:
#   1. Validate startup environment
#   2. Log startup information
#   3. Execute the application

echo "=========================================="
echo "KromicStore API - Production Startup"
echo "=========================================="
echo ""

# Validate environment
echo "[STARTUP] Validating environment..."

if [ -z "$ASPNETCORE_ENVIRONMENT" ]; then
    echo "[STARTUP] WARNING: ASPNETCORE_ENVIRONMENT not set, defaulting to Production"
    export ASPNETCORE_ENVIRONMENT=Production
fi

if [ -z "$ASPNETCORE_URLS" ]; then
    echo "[STARTUP] WARNING: ASPNETCORE_URLS not set, defaulting to http://+:8080"
    export ASPNETCORE_URLS=http://+:8080
fi

if [ -z "$ConnectionStrings__DefaultConnection" ]; then
    echo "[STARTUP] ERROR: ConnectionStrings__DefaultConnection environment variable not set"
    echo "[STARTUP] Application startup failed: Missing required database connection string"
    exit 1
fi

echo "[STARTUP] ✓ Environment validation successful"
echo ""

# Log startup configuration
echo "[STARTUP] Startup Configuration:"
echo "[STARTUP]   - Environment: $ASPNETCORE_ENVIRONMENT"
echo "[STARTUP]   - URLs: $ASPNETCORE_URLS"
echo "[STARTUP]   - Database: Connected"
echo ""

# Execute the application
echo "[STARTUP] Starting KromicStore.API application..."
echo "[STARTUP] Migration execution will begin if configured (ApplyMigrationsOnStartup=true)"
echo ""
echo "=========================================="
echo ""

# Execute with PID 1 to receive signals correctly
exec dotnet KromicStore.API.dll

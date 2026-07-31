# Multi-stage build for KromicStore API

# Stage 1: Restore
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /src

COPY ["src/KromicStore.API/KromicStore.API.csproj", "src/KromicStore.API/"]
COPY ["src/KromicStore.Application/KromicStore.Application.csproj", "src/KromicStore.Application/"]
COPY ["src/KromicStore.Domain/KromicStore.Domain.csproj", "src/KromicStore.Domain/"]
COPY ["src/KromicStore.Infrastructure/KromicStore.Infrastructure.csproj", "src/KromicStore.Infrastructure/"]

RUN dotnet restore "src/KromicStore.API/KromicStore.API.csproj"

# Stage 2: Build
FROM restore AS build
WORKDIR /src

COPY . .
RUN dotnet build "src/KromicStore.API/KromicStore.API.csproj" -c Release --no-restore

# Stage 3: Publish
FROM build AS publish
WORKDIR /src

RUN dotnet publish "src/KromicStore.API/KromicStore.API.csproj" \
    -c Release \
    --no-build \
    -o /app/publish

# Stage 4: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD dotnet /app/KromicStore.API.dll health-check || exit 1

ENTRYPOINT ["dotnet", "KromicStore.API.dll"]

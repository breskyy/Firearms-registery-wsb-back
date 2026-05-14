# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first to leverage Docker layer caching for restore
COPY src/EWeaponRegistry.Api/EWeaponRegistry.Api.csproj src/EWeaponRegistry.Api/
COPY src/EWeaponRegistry.Application/EWeaponRegistry.Application.csproj src/EWeaponRegistry.Application/
COPY src/EWeaponRegistry.Domain/EWeaponRegistry.Domain.csproj src/EWeaponRegistry.Domain/
COPY src/EWeaponRegistry.Infrastructure/EWeaponRegistry.Infrastructure.csproj src/EWeaponRegistry.Infrastructure/

# Restore dependencies for the API and its production dependencies only
RUN dotnet restore src/EWeaponRegistry.Api/EWeaponRegistry.Api.csproj

# Copy source code (tests excluded via .dockerignore)
COPY src/ src/

# Build the API project
RUN dotnet build src/EWeaponRegistry.Api/EWeaponRegistry.Api.csproj -c Release --no-restore

# Publish the application
RUN dotnet publish src/EWeaponRegistry.Api/EWeaponRegistry.Api.csproj -c Release -o /app/publish --no-build

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser

# Copy published application
COPY --from=build /app/publish .

# Set ownership to non-root user
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Bind Kestrel to the port Railway/Docker expects
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/api/v1/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "EWeaponRegistry.Api.dll"]

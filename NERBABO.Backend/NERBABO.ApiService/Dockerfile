# Use the official .NET 9 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["NERBABO.Backend/NERBABO.ApiService/NERBABO.ApiService.csproj", "NERBABO.Backend/NERBABO.ApiService/"]
COPY ["NERBABO.Backend/NERBABO.ServiceDefaults/NERBABO.ServiceDefaults.csproj", "NERBABO.Backend/NERBABO.ServiceDefaults/"]
RUN dotnet restore "NERBABO.Backend/NERBABO.ApiService/NERBABO.ApiService.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/NERBABO.Backend/NERBABO.ApiService"

# Build the application in Release mode
RUN dotnet build "NERBABO.ApiService.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "NERBABO.ApiService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the official .NET 9 runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

# Install curl for health checks (since docker-compose manages health via depends_on)
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create a non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Set the working directory
WORKDIR /app

# Copy the published application from the publish stage
COPY --from=publish /app/publish .

# Create necessary directories and set permissions
RUN mkdir -p /app/logs /app/temp /app/wwwroot/uploads && \
    chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose the port that the application will run on
EXPOSE 8080

# Set environment variables for production (will be overridden by docker-compose)
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

# Configure health check - using custom health endpoint with comprehensive checks
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:8080/api/general-info/alive || exit 1

# Set the entry point
ENTRYPOINT ["dotnet", "NERBABO.ApiService.dll"]
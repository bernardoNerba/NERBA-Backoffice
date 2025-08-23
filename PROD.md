# NERBABO Production Deployment Guide

## 🚀 Quick Start

### One-Command Deployment (Recommended)

```bash
chmod +x start.sh
./start.sh
```

The `start.sh` script provides a complete automated deployment with intelligent IP detection, secure password generation, and health checks.

## 🏗️ Architecture Overview

### Container Architecture

The application runs in a multi-container Docker environment with the following services:

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Angular SPA   │    │   .NET API      │    │  PostgreSQL DB  │
│   (Frontend)    │    │   (Backend)     │    │   (Database)    │
│   Port: 4200    │    │   Port: 5001    │    │   Port: 5432    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         └───────────────────────┼───────────────────────┘
                                 │
      ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
      │     Redis       │    │    PgAdmin      │    │ Redis Insight   │
      │   (Cache)       │    │  (DB Admin)     │    │ (Monitoring)    │
      │   Port: 6379    │    │   Port: 8080    │    │   Port: 5540    │
      └─────────────────┘    └─────────────────┘    └─────────────────┘
```

### Port Mapping

| Service | Internal Port | External Port | Purpose |
|---------|---------------|---------------|---------|
| Angular Frontend | 80 | 4200 | Web Application UI |
| .NET API | 8080 | 5001 | Backend REST API |
| PostgreSQL | 5432 | 5432 | Primary Database |
| Redis | 6379 | 6379 | Caching & Sessions |
| PgAdmin | 80 | 8080 | Database Administration |
| Redis Insight | 5540 | 5540 | Cache Monitoring |

## 🐳 Docker Configuration

### Container Specifications

#### Frontend Container (Angular + Nginx)
- **Base Image**: `node:18-alpine` → `nginx:alpine` (multi-stage)
- **Build Process**: 
  - Installs npm dependencies
  - Builds Angular app for production
  - Serves via nginx on port 80
- **Volume Mounts**: None (stateless)
- **Health Check**: HTTP response validation

```dockerfile
# Multi-stage build for optimized production image
FROM node:18-alpine as build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build -- --configuration production

FROM nginx:alpine
COPY --from=build /app/dist/nerbabo.frontend/browser/ /usr/share/nginx/html/
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
```

#### Backend Container (.NET API)
- **Base Image**: `mcr.microsoft.com/dotnet/sdk:9.0` → `mcr.microsoft.com/dotnet/aspnet:9.0`
- **Security**: Runs as non-root user (`appuser`)
- **Volumes**: `/app/logs`, `/app/temp` for file operations
- **Health Check**: Built-in health endpoints
- **File Storage**: 
  - Images: `/app/wwwroot/uploads/images/`
  - PDFs: `/app/wwwroot/storage/pdfs/`

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["NERBABO.Backend/NERBABO.ApiService/NERBABO.ApiService.csproj", "NERBABO.Backend/NERBABO.ApiService/"]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
RUN groupadd -r appuser && useradd -r -g appuser appuser
WORKDIR /app
COPY --from=build /app/publish .
RUN mkdir -p /app/logs /app/temp && chown -R appuser:appuser /app
USER appuser
EXPOSE 8080
```

#### Database Container (PostgreSQL)
- **Base Image**: `postgres:17.4`
- **Authentication**: SCRAM-SHA-256
- **Persistent Storage**: Named volume `postgres_data`
- **Health Check**: `pg_isready` command
- **Configuration**: Optimized for production workloads

### Docker Compose Services

```yaml
services:
  postgres:
    image: postgres:17.4
    container_name: nerbabo-postgres
    environment:
      POSTGRES_HOST_AUTH_METHOD: scram-sha-256
      POSTGRES_INITDB_ARGS: --auth-host=scram-sha-256 --auth-local=scram-sha-256
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
      POSTGRES_DB: nerbabo_db
    volumes:
      - postgres_data:/var/lib/postgresql/data
    networks:
      - nerbabo-network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 30s
      timeout: 10s
      retries: 3

  redis:
    image: redis:7.4
    container_name: nerbabo-redis
    environment:
      REDIS_PASSWORD: ${REDIS_PASSWORD}
    volumes:
      - redis_data:/data
    networks:
      - nerbabo-network

  api:
    build:
      context: .
      dockerfile: NERBABO.Backend/NERBABO.ApiService/Dockerfile
    container_name: nerbabo-api
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
    networks:
      - nerbabo-network

  angular:
    build:
      context: ./NERBABO.Frontend
    container_name: nerbabo-angular
    depends_on:
      - api
    networks:
      - nerbabo-network
```

## ⚙️ Nginx Configuration

### Reverse Proxy Setup

The nginx configuration in the frontend container handles:

1. **Static Asset Serving**: Serves Angular build files
2. **API Proxying**: Routes `/uploads/` to backend for file serving
3. **SPA Routing**: Handles Angular router with fallback to `index.html`
4. **Caching**: Optimized cache headers for static assets

```nginx
server {
    listen 80;
    server_name localhost;
    root /usr/share/nginx/html;
    index index.html;

    # Proxy uploaded images to the backend API
    location /uploads/ {
        proxy_pass http://nerbabo-api:8080;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        
        # Cache images for better performance
        expires 1y;
        add_header Cache-Control "public, immutable";
        
        # Handle errors gracefully
        proxy_intercept_errors on;
        error_page 404 = @missing_image;
    }
    
    # Handle static assets (JS, CSS, fonts)
    location ~* \.(js|css|ico|svg|woff|woff2|ttf|eot)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
        try_files $uri =404;
    }
    
    # Handle image files from frontend static assets
    location ~* \.(png|jpg|jpeg|gif)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
        try_files $uri =404;
    }

    # SPA routing - serve index.html for all unmatched routes
    location / {
        try_files $uri $uri/ /index.html;
    }

    # Fallback for missing images
    location @missing_image {
        return 404;
    }
}
```

### Key Routing Rules

- **`/uploads/`** → Proxied to `nerbabo-api:8080` for dynamic file serving
- **Static Assets** → Served directly from nginx with aggressive caching (1 year)
- **Angular Routes** → All unmatched routes fallback to `index.html` for SPA routing

## 🔧 Start.sh Script Features

### Intelligent IP Detection

The script uses multiple methods to detect the server IP:

```bash
get_local_ip() {
    local ip=""
    
    # Method 1: ip route (Linux - most reliable)
    if command -v ip >/dev/null 2>&1; then
        ip=$(ip route get 8.8.8.8 2>/dev/null | awk '{print $7}' | head -n1)
    fi
    
    # Method 2: hostname -I (fallback)
    if [ -z "$ip" ] && command -v hostname >/dev/null 2>&1; then
        ip=$(hostname -I | awk '{print $1}')
    fi
    
    # Method 3: ifconfig (macOS/BSD fallback)
    if [ -z "$ip" ] && command -v ifconfig >/dev/null 2>&1; then
        ip=$(ifconfig | grep -Eo 'inet (addr:)?([0-9]*\.){3}[0-9]*' | grep -Eo '([0-9]*\.){3}[0-9]*' | grep -v '127.0.0.1' | head -n1)
    fi
    
    # Default fallback
    if [ -z "$ip" ]; then
        ip="localhost"
    fi
    
    echo "$ip"
}
```

### Secure Configuration Generation

Automatically generates:
- **Database passwords**: Cryptographically secure random passwords
- **Redis passwords**: 32-character random passwords  
- **JWT keys**: 64-character signing keys
- **CORS origins**: Dynamic based on detected IP

### Automated Health Checks

Post-deployment validation:
- API health endpoint verification
- Frontend accessibility check
- Container status monitoring
- Service dependency validation

## 📁 File Storage Architecture

### Image Service (IImageService)

- **Storage Location**: `wwwroot/uploads/images/`
- **Supported Formats**: JPG, PNG, GIF, BMP (max 5MB)
- **Access Pattern**: 
  - Upload via API: `POST /api/frames` with multipart form
  - Access via URL: `GET /uploads/images/{filename}`
  - Nginx proxy: Frontend → API for dynamic serving
- **Security**: File validation, size limits, MIME type checking

### PDF Service (IPdfService)

- **Storage Location**: `wwwroot/storage/pdfs/` 
- **Generation**: On-demand report generation using QuestPDF
- **Caching**: SHA-256 content hashing for deduplication
- **Database Integration**: Metadata stored in `SavedPdfs` table
- **Access Pattern**: API-only access with authentication required

### File URL Generation

Images are served with fully qualified URLs:
```csharp
// Example from Frame.cs:50-53
ProgramLogoUrl = !string.IsNullOrEmpty(frame.ProgramLogo) 
    ? $"{Helper.UrlHelper.GetBaseUrl()}/uploads/images/{frame.ProgramLogo.Replace('\\', '/')}" 
    : null
```

## 🗄️ Database Configuration

### PostgreSQL Setup

- **Version**: PostgreSQL 17.4
- **Authentication**: SCRAM-SHA-256 (secure password hashing)
- **Database**: `nerbabo_db`
- **Schema**: Entity Framework Core migrations
- **Persistence**: Docker volume `postgres_data`

### Migration Process

Automatic migration on startup with error handling:

```csharp
// Program.cs migration logic with enhanced logging
try
{
    logger.LogInformation("Starting database migration...");
    var dbContext = services.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
    logger.LogInformation("Database migration completed successfully.");

    logger.LogInformation("Starting database seeding...");
    var seeder = new SeedDataHelp(userManager, roleManager, dbContext, builder.Configuration);
    await seeder.InitializeAsync();
    logger.LogInformation("Database seeding completed successfully.");
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    throw; // Prevent startup with incomplete database
}
```

### Seeded Data

- **Admin User**: `admin@nerbabo.local` / `Admin123!`
- **Roles**: Admin, User, FM, CQ
- **Default Configuration**: System settings and taxes

## 🔒 Security Configuration

### Authentication & Authorization

- **JWT Tokens**: Secure signing with configurable expiration
- **Role-based Access**: Multiple user roles with granular permissions
- **CORS Policy**: Dynamic origin configuration based on deployment IP

### Container Security

- **Non-root Execution**: API runs as dedicated `appuser`
- **Network Isolation**: Internal Docker network communication
- **Secret Management**: Environment variable based configuration
- **File Permissions**: Restricted file system access

### Data Protection

- **Password Hashing**: Identity Framework secure hashing
- **Database Encryption**: PostgreSQL SCRAM-SHA-256 authentication
- **Session Security**: Redis-based session management
- **Input Validation**: Comprehensive model validation

## 📊 Monitoring & Administration

### Available Interfaces

- **Application**: `http://{IP}:4200` - Main application
- **API Documentation**: Available in development mode
- **PgAdmin**: `http://{IP}:8080` - Database administration
- **Redis Insight**: `http://{IP}:5540` - Cache monitoring

### Log Management

```bash
# View all logs
docker-compose logs -f

# View specific service logs
docker-compose logs -f api
docker-compose logs -f angular
docker-compose logs -f postgres

# View logs with timestamps
docker-compose logs -f -t
```

### Health Monitoring

Health checks configured for all critical services:
- PostgreSQL: `pg_isready` check every 30s
- Redis: Connection validation
- API: Application health endpoints (development only)
- Frontend: HTTP response validation

## 🚀 Deployment Commands

### Initial Deployment

```bash
# Clone repository
git clone <repository-url>
cd NERBA-Backoffice

# Make start script executable
chmod +x start.sh

# Deploy application
./start.sh
```

### Ongoing Operations

```bash
# View status
docker-compose ps

# Stop application
docker-compose down

# Update and restart
git pull origin main
docker-compose down
docker-compose up -d --build

# Reset everything (careful!)
docker-compose down -v
docker system prune -f
./start.sh
```

### Backup Operations

```bash
# Backup database
docker exec nerbabo-postgres pg_dump -U postgres nerbabo_db > backup.sql

# Backup uploaded files
docker cp nerbabo-api:/app/wwwroot ./file_backup

# Restore database
docker exec -i nerbabo-postgres psql -U postgres nerbabo_db < backup.sql
```

## 🔍 Troubleshooting

### Common Issues

**Migration Failures**:
- Check PostgreSQL container health: `docker-compose ps`
- Verify database connection: Connection string in logs
- Check disk space: Migrations require temporary storage

**File Upload Issues**:
- Verify nginx proxy configuration points to `nerbabo-api`
- Check container networking: `docker network ls`
- Validate file permissions: `/app/wwwroot` ownership

**Performance Issues**:
- Monitor Redis cache hit rate via Redis Insight
- Check PostgreSQL connection pooling
- Review nginx access logs for static file serving

### Debug Mode

Enable detailed logging by setting environment variables:
```bash
# In .env file
ASPNETCORE_ENVIRONMENT=Development
Logging__LogLevel__Default=Debug
```

## 📈 Production Considerations

### Scaling

- **Database**: Configure connection pooling and read replicas
- **API**: Use container orchestration (Docker Swarm/Kubernetes)
- **Frontend**: CDN distribution for static assets
- **Cache**: Redis clustering for high availability

### SSL/TLS

For production internet deployment:
1. Obtain SSL certificates (Let's Encrypt recommended)
2. Configure nginx SSL termination
3. Update CORS origins to HTTPS
4. Set secure cookie flags

### Backup Strategy

- **Database**: Regular pg_dump exports with rotation
- **Files**: File system backups of upload directories
- **Configuration**: Version control for docker-compose.yml and .env templates

This comprehensive setup provides a robust, scalable foundation for the NERBABO application with production-ready security, monitoring, and maintenance capabilities.
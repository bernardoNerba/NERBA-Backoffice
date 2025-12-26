#!/bin/bash

# start.sh - Script to start the application with dynamic IP configuration

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}🚀 Starting NERBABO Application...${NC}"

# Function to get local IP address
get_local_ip() {
    # Try multiple methods to get the local IP
    local ip=""
    
    # Method 1: ip route (most reliable on Linux)
    if command -v ip >/dev/null 2>&1; then
        ip=$(ip route get 8.8.8.8 2>/dev/null | awk '{print $7}' | head -n1)
    fi
    
    # Method 2: hostname -I (fallback)
    if [ -z "$ip" ] && command -v hostname >/dev/null 2>&1; then
        ip=$(hostname -I | awk '{print $1}')
    fi
    
    # Method 3: ifconfig (another fallback)
    if [ -z "$ip" ] && command -v ifconfig >/dev/null 2>&1; then
        ip=$(ifconfig | grep -Eo 'inet (addr:)?([0-9]*\.){3}[0-9]*' | grep -Eo '([0-9]*\.){3}[0-9]*' | grep -v '127.0.0.1' | head -n1)
    fi
    
    # Default fallback
    if [ -z "$ip" ]; then
        ip="localhost"
    fi
    
    echo "$ip"
}

# Get the local IP address
LOCAL_IP=$(get_local_ip)
echo -e "${YELLOW}📍 Detected IP address: $LOCAL_IP${NC}"

# Create or update .env file
ENV_FILE=".env"

# Function to create .env file
create_env_file() {
    # Backup existing .env if it exists
    if [ -f "$ENV_FILE" ]; then
        cp "$ENV_FILE" "$ENV_FILE.backup.$(date +%Y%m%d_%H%M%S)"
        echo -e "${YELLOW}📋 Backed up existing .env file${NC}"
    fi

    # Create .env file with dynamic values
    cat > "$ENV_FILE" << EOF
# Auto-generated configuration - $(date)
# Server IP: $LOCAL_IP

# Database Configuration
POSTGRES_PASSWORD=${POSTGRES_PASSWORD:-$(openssl rand -base64 32 | tr -d "=+/")}
REDIS_PASSWORD=${REDIS_PASSWORD:-$(openssl rand -base64 32 | tr -d "=+/")}

# JWT Configuration
JWT_KEY=${JWT_KEY:-$(openssl rand -base64 64 | tr -d "=+/")}
JWT_EXPIRES_DAYS=7
JWT_ISSUER=http://$LOCAL_IP:5001

# Client URLs - Direct Docker Access (port 4200)
CLIENT_URL=http://$LOCAL_IP:4200
CLIENT_URL_HTTPS=https://$LOCAL_IP:4200

# Client URLs - Nginx Reverse Proxy (standard web ports)
CLIENT_URL_PROXY=http://$LOCAL_IP
CLIENT_URL_PROXY_HTTPS=https://$LOCAL_IP

# Admin Configuration
ADMIN_USERNAME=${ADMIN_USERNAME:-admin}
ADMIN_EMAIL=${ADMIN_EMAIL:-admin@nerbabo.local}
ADMIN_PASSWORD=${ADMIN_PASSWORD:-Admin123!}

# PgAdmin Configuration
PGADMIN_PASSWORD=${PGADMIN_PASSWORD:-admin123}

# Environment
ASPNETCORE_ENVIRONMENT=Production
EOF

    echo -e "${GREEN}✅ Generated .env file with IP: $LOCAL_IP${NC}"
}

# Check if .env file exists and prompt user
if [ -f "$ENV_FILE" ]; then
    echo -e "${YELLOW}🤔 An existing .env file was found.${NC}"
    read -p "Do you want to use the existing .env file? (y/n) " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        echo -e "${GREEN}👍 Using existing .env file.${NC}"
        # Extract IP from existing .env to use in health checks
        if grep -q "CLIENT_URL" "$ENV_FILE"; then
            LOCAL_IP=$(grep "CLIENT_URL" "$ENV_FILE" | cut -d'=' -f2 | sed 's|http://||' | cut -d':' -f1)
            echo -e "${YELLOW}📍 Detected IP address from .env: $LOCAL_IP${NC}"
        fi
    else
        echo -e "${YELLOW}✨ Creating a new .env file...${NC}"
        create_env_file
    fi
else
    create_env_file
fi

# Check if Docker is running
if ! docker info >/dev/null 2>&1; then
    echo -e "${RED}❌ Docker is not running. Please start Docker and try again.${NC}"
    exit 1
fi

# Check if docker-compose is available
if ! command -v docker-compose >/dev/null 2>&1 && ! docker compose version >/dev/null 2>&1; then
    echo -e "${RED}❌ docker-compose is not available. Please install Docker Compose.${NC}"
    exit 1
fi

# Use docker compose or docker-compose based on availability
DOCKER_COMPOSE_CMD="docker compose"
if ! docker compose version >/dev/null 2>&1; then
    DOCKER_COMPOSE_CMD="docker-compose"
fi

# Check if .NET SDK and EF tools are available
if ! command -v dotnet >/dev/null 2>&1; then
    echo -e "${RED}❌ .NET SDK is not installed. Please install .NET SDK 9.0 and try again.${NC}"
    exit 1
fi

# Check if this is first run by looking for existing migrations
MIGRATIONS_DIR="NERBABO.Backend/NERBABO.ApiService/Data/Migrations"
FIRST_RUN=false

if [ ! -d "$MIGRATIONS_DIR" ] || [ -z "$(ls -A "$MIGRATIONS_DIR" 2>/dev/null)" ]; then
    FIRST_RUN=true
    echo -e "${YELLOW}📦 First run detected - will create InitialMigration${NC}"
else
    echo -e "${GREEN}✅ Existing migrations found - skipping migration creation${NC}"
fi

# Create InitialMigration if this is the first run
if [ "$FIRST_RUN" = true ]; then
    echo -e "${GREEN}🔧 Creating InitialMigration...${NC}"
    
    # Navigate to backend directory and create migration
    if ! dotnet ef migrations add "InitialMigration" -o Data/Migrations --project NERBABO.Backend/NERBABO.ApiService; then
        echo -e "${RED}❌ Failed to create InitialMigration. Please ensure EF tools are installed: dotnet tool install --global dotnet-ef${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}✅ InitialMigration created successfully${NC}"
fi

# Stop existing containers if running
echo -e "${YELLOW}🛑 Stopping existing containers...${NC}"
$DOCKER_COMPOSE_CMD down

# Build and start the application
echo -e "${GREEN}🏗️  Building and starting containers...${NC}"
$DOCKER_COMPOSE_CMD up -d --build

# Wait for services to be ready
echo -e "${YELLOW}⏳ Waiting for services to start...${NC}"
sleep 10

# Check if services are healthy
echo -e "${GREEN}🔍 Checking service health...${NC}"

# Check API health with comprehensive checks
API_URL="http://$LOCAL_IP:5001"
if curl -f -s "$API_URL/api/general-info/alive" >/dev/null 2>&1; then
    echo -e "${GREEN}✅ API is alive at $API_URL${NC}"
    
    # Try comprehensive health check
    if curl -f -s "$API_URL/api/general-info/health" >/dev/null 2>&1; then
        echo -e "${GREEN}✅ API is fully healthy (database and cache connected)${NC}"
    else
        echo -e "${YELLOW}⚠️  API is alive but some services may not be ready${NC}"
    fi
else
    echo -e "${YELLOW}⚠️  API might still be starting up at $API_URL${NC}"
fi

# Check Angular app
ANGULAR_URL="http://$LOCAL_IP:4200"
if curl -f -s "$ANGULAR_URL" >/dev/null 2>&1; then
    echo -e "${GREEN}✅ Angular app is running at $ANGULAR_URL${NC}"
else
    echo -e "${YELLOW}⚠️  Angular app might still be starting up at $ANGULAR_URL${NC}"
fi

echo ""
echo -e "${GREEN}🎉 Application started successfully!${NC}"
echo ""
echo -e "${YELLOW}📍 Access your application at:${NC}"
echo -e "   🌐 Frontend:    $ANGULAR_URL"
echo -e "   🔧 API:        $API_URL"
echo -e "   🗄️  PgAdmin:    http://$LOCAL_IP:8080"
echo -e "   📊 Redis UI:   http://$LOCAL_IP:5540"
echo ""
echo -e "${GREEN}💡 To stop the application: $DOCKER_COMPOSE_CMD down${NC}"
echo -e "${GREEN}💡 To view logs: $DOCKER_COMPOSE_CMD logs -f${NC}"
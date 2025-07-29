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
POSTGRES_PASSWORD=${POSTGRES_PASSWORD:-$(openssl rand -base64 32 | tr -d "=+/" | cut -c1-32)}
REDIS_PASSWORD=${REDIS_PASSWORD:-$(openssl rand -base64 32 | tr -d "=+/" | cut -c1-32)}

# JWT Configuration
JWT_KEY=${JWT_KEY:-$(openssl rand -base64 64 | tr -d "=+/" | cut -c1-64)}
JWT_EXPIRES_DAYS=7
JWT_ISSUER=http://$LOCAL_IP:5001
CLIENT_URL=http://$LOCAL_IP:4200
CLIENT_URL_HTTPS=https://$LOCAL_IP:4200

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

# Check API health
API_URL="http://$LOCAL_IP:5001"
if curl -f -s "$API_URL/health" >/dev/null 2>&1; then
    echo -e "${GREEN}✅ API is healthy at $API_URL${NC}"
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
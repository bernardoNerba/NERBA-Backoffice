# NERBABO Production Setup - Quick Guide

## Prerequisites

- Docker and Docker Compose installed
- Server with minimum 4GB RAM and 20GB storage
- Network access to required ports (4200, 5001)

## 🚀 One-Command Deployment

### Automated Setup (Recommended)

```bash
chmod +x start.sh
./start.sh
```

This single command will:

- ✅ **Auto-detect your server IP** (works with dynamic IPs)
- ✅ **Generate secure passwords** for all services
- ✅ **Create environment configuration** automatically
- ✅ **Build and start all containers**
- ✅ **Run health checks** to verify everything is working
- ✅ **Display access URLs** for your applications

### Manual Setup (Alternative)

If you prefer manual configuration:

1. **Copy environment template:**

```bash
cp .env.example .env
```

2. **Edit configuration:**

```bash
nano .env  # Update with your server IP and passwords
```

3. **Start application:**

```bash
docker-compose up -d --build
```

## 🌟 Key Features

### Smart IP Detection

- **No static IP required** - automatically detects server IP
- **Works in any network environment** - home, office, cloud
- **Handles IP changes** - application continues working even if IP changes

### Automatic Configuration

- **Secure password generation** - creates strong passwords for all services
- **JWT key generation** - creates cryptographically secure authentication keys
- **CORS setup** - automatically configures cross-origin requests
- **Database initialization** - sets up PostgreSQL with proper schema

### Production-Ready Services

- **API Backend** - .NET Core API with authentication and authorization
- **Angular Frontend** - Responsive web application
- **PostgreSQL Database** - Reliable data storage with automated backups
- **Redis Cache** - High-performance caching layer
- **PgAdmin** - Database administration interface
- **Redis Insight** - Cache monitoring and management

## 📱 Access Your Application

After running `./start.sh`, you'll see output like:

```
🎉 Application started successfully!

📍 Access your application at:
   🌐 Frontend:    http://192.168.1.100:4200
   🔧 API:        http://192.168.1.100:5001
   🗄️  PgAdmin:    http://192.168.1.100:8080
   📊 Redis UI:   http://192.168.1.100:5540
```

### Default Admin Access

- **Username:** admin
- **Email:** admin@nerbabo.local
- **Password:** Admin123! (or check your .env file)

## 🔧 Common Operations

### View Application Status

```bash
docker-compose ps
```

### View Logs

```bash
docker-compose logs -f        # All services
docker-compose logs -f api    # API only
docker-compose logs -f angular # Frontend only
```

### Stop Application

```bash
docker-compose down
```

### Restart Application

```bash
docker-compose restart
```

### Update Application

```bash
git pull origin main
docker-compose down
docker-compose up -d --build
```

## 🛡️ Security Notes

- All passwords are auto-generated and stored in `.env` file
- Keep your `.env` file secure and don't commit it to version control
- The application is configured for internal network use
- For internet exposure, consider adding SSL certificates and firewall rules

## 🆘 Quick Troubleshooting

### If containers won't start:

```bash
docker system prune -f  # Clean up Docker
./start.sh              # Try again
```

### If can't access application:

- Check if firewall is blocking ports 4200 and 5001
- Verify IP address with: `ip addr show`
- Try accessing via localhost: `http://localhost:4200`

### If need to reset everything:

```bash
docker-compose down -v  # Remove all data
./start.sh              # Fresh start
```

## 📊 What Gets Created

The startup script creates:

- `.env` file with your configuration
- PostgreSQL database with application schema
- Redis cache instance
- All necessary Docker networks and volumes
- Admin user account
- Sample data (if configured)

**That's it!** Your NERBABO application is now running and accessible on your network. The smart IP detection ensures it will continue working even if your server IP changes.

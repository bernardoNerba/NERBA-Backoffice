# NERBABO Backoffice - Nginx Reverse Proxy Deployment Guide

## Overview

This guide covers the modifications made to enable proper deployment of NERBABO Backoffice using Docker with nginx as a reverse proxy. All changes support both **direct Docker access** (ports 4200/5001) and **nginx reverse proxy** (port 80/443).

---

## Changes Made

### 1. Frontend Updates ✅

#### File: `NERBABO.Frontend/src/environments/environment.production.ts`
**What changed:** Frontend now intelligently detects whether it's behind a reverse proxy or accessed directly.

```typescript
// Automatic detection:
// - Port 80/443 or no port → Use same origin (nginx routing)
// - Port 4200 → Use port 5001 for API (direct Docker access)
```

**Benefits:**
- Works seamlessly with nginx reverse proxy on standard ports
- Still works when accessing Docker containers directly
- No rebuild needed when switching deployment modes

#### File: `NERBABO.Frontend/src/app/core/services/api-config.service.ts`
**What changed:** Same intelligent detection logic for API URL determination.

---

### 2. Backend CORS Updates ✅

#### File: `NERBABO.Backend/NERBABO.ApiService/Program.cs`

**What changed:**
1. Added `using Microsoft.AspNetCore.HttpOverrides` and `using System.Net`
2. Configured ForwardedHeaders middleware for reverse proxy support
3. Updated CORS policy to be flexible with ports (matches by host and scheme)

**Key improvements:**
```csharp
// Now accepts requests from same host regardless of port
// Example: Both http://10.210.5.131:80 and http://10.210.5.131:4200 work
policy.SetIsOriginAllowed(origin => {
    // Match by host and scheme, flexible on port
    if (uri.Host == allowedUri.Host && uri.Scheme == allowedUri.Scheme)
        return true;
});
```

#### File: `NERBABO.Backend/NERBABO.ApiService/Helper/DnsHelper.cs`

**What changed:** Added standard web ports (80/443) to generated CORS origins.

**Before:**
```csharp
origins.Add($"http://{serverIP}:4200");  // Only port 4200
```

**After:**
```csharp
origins.Add($"http://{serverIP}");       // Port 80
origins.Add($"https://{serverIP}");      // Port 443
origins.Add($"http://{serverIP}:80");
origins.Add($"https://{serverIP}:443");
origins.Add($"http://{serverIP}:4200");  // Still supports direct access
```

---

### 3. Docker Compose Updates ✅

#### File: `docker-compose.yml`

**What changed:** Added additional CORS origin environment variables.

```yaml
# CORS Configuration
CORS__AllowedOrigins__0: ${CLIENT_URL:-http://localhost:4200}
CORS__AllowedOrigins__1: ${CLIENT_URL_HTTPS:-https://localhost:4200}
CORS__AllowedOrigins__2: ${CLIENT_URL_PROXY:-http://localhost}      # NEW
CORS__AllowedOrigins__3: ${CLIENT_URL_PROXY_HTTPS:-https://localhost}  # NEW
```

---

### 4. Environment Configuration Updates ✅

#### File: `.env`

**New variables added:**
```env
# Client URLs - Nginx Reverse Proxy (standard web ports)
CLIENT_URL_PROXY=http://10.210.5.131
CLIENT_URL_PROXY_HTTPS=https://10.210.5.131
```

#### File: `.env.example`
Updated with documentation for new variables.

#### File: `start.sh`
Automatically generates proxy URL variables during setup.

---

### 5. Nginx Configuration ✅

#### File: `nginx-reverse-proxy.conf` (NEW)

A production-ready nginx configuration with:
- ✅ Proper forwarded headers (`X-Real-IP`, `X-Forwarded-For`, `X-Forwarded-Proto`)
- ✅ Security headers (`X-Frame-Options`, `X-Content-Type-Options`, etc.)
- ✅ CORS preflight handling for OPTIONS requests
- ✅ Increased timeouts for large uploads and long-running requests
- ✅ Proper caching for static assets and uploads
- ✅ Health check endpoint
- ✅ PgAdmin routing (optional, can be disabled)
- ✅ **Swagger disabled by default (development only)**
- ✅ SSL/TLS configuration (commented, ready to enable)

---

## Deployment Instructions

### Option A: Deploy with Current Nginx Setup

Your existing nginx configuration at `/etc/nginx/sites-available/nerbabo` is already functional. To improve it:

1. **Backup current configuration:**
   ```bash
   sudo cp /etc/nginx/sites-available/nerbabo /etc/nginx/sites-available/nerbabo.backup
   ```

2. **Copy improved configuration:**
   ```bash
   sudo cp /home/nerba/NERBA/NERBA-Backoffice/nginx-reverse-proxy.conf /etc/nginx/sites-available/nerbabo
   ```

3. **Test configuration:**
   ```bash
   sudo nginx -t
   ```

4. **Reload nginx:**
   ```bash
   sudo systemctl reload nginx
   ```

### Option B: Fresh Deployment

1. **Update environment variables:**
   ```bash
   cd /home/nerba/NERBA/NERBA-Backoffice

   # Edit .env to set CLIENT_URL_PROXY variables
   nano .env

   # Set:
   CLIENT_URL_PROXY=http://YOUR_SERVER_IP
   CLIENT_URL_PROXY_HTTPS=https://YOUR_SERVER_IP
   ```

2. **Rebuild and restart Docker containers:**
   ```bash
   docker-compose down
   docker-compose build --no-cache
   docker-compose up -d
   ```

3. **Configure nginx** (as shown in Option A)

4. **Verify deployment:**
   ```bash
   # Check Docker containers
   docker-compose ps

   # Check nginx
   sudo systemctl status nginx

   # Test frontend
   curl http://YOUR_SERVER_IP

   # Test API health
   curl http://YOUR_SERVER_IP/health
   ```

---

## Architecture Diagrams

### Before (Direct Access)
```
User Browser → http://10.210.5.131:4200 → Frontend Container (port 4200)
User Browser → http://10.210.5.131:5001/api → Backend Container (port 5001)
```

### After (Nginx Reverse Proxy)
```
User Browser → http://10.210.5.131/
                ↓
            Nginx (port 80)
                ↓
    ┌───────────┴───────────┐
    ↓                       ↓
Frontend Container    Backend Container
(localhost:4200)      (localhost:5001/api)
```

---

## Port Mapping Summary

| Service | Docker Internal | Docker External | Nginx Proxy |
|---------|-----------------|-----------------|-------------|
| Frontend | 80 | 4200 | → / (port 80) |
| Backend API | 8080 | 5001 | → /api (port 80) |
| PostgreSQL | 5432 | 5432 | Not exposed |
| Redis | 6379 | 6379 | Not exposed |
| PgAdmin | 80 | 8080 | → /pgadmin (optional) |

---

## Security Considerations

### Production Checklist

- [ ] **Enable HTTPS/SSL** - Uncomment SSL configuration in nginx config
- [ ] **Get SSL certificate** - Use Let's Encrypt or your certificate authority
- [ ] **Disable PgAdmin** in production (comment out /pgadmin location in nginx)
- [ ] **Swagger is already disabled** in production (Program.cs checks `Environment.IsDevelopment()`)
- [ ] **Change default passwords** in `.env` file
- [ ] **Restrict database access** - Bind PostgreSQL to localhost only
- [ ] **Enable firewall** - Only allow ports 80, 443
- [ ] **Review CORS origins** - Ensure only your domain is allowed
- [ ] **Secure .env file** - `chmod 600 .env`

### SSL/TLS Setup with Let's Encrypt

```bash
# Install certbot
sudo apt install certbot python3-certbot-nginx

# Get certificate (replace with your domain)
sudo certbot --nginx -d yourdomain.com

# Certbot will automatically:
# - Obtain certificate
# - Modify nginx configuration
# - Set up auto-renewal

# Verify auto-renewal
sudo certbot renew --dry-run
```

### Nginx Configuration for HTTPS

Already included in `nginx-reverse-proxy.conf`, just uncomment:

```nginx
listen 443 ssl http2;
listen [::]:443 ssl http2;
ssl_certificate /etc/letsencrypt/live/yourdomain.com/fullchain.pem;
ssl_certificate_key /etc/letsencrypt/live/yourdomain.com/privkey.pem;
ssl_protocols TLSv1.2 TLSv1.3;
```

---

## Troubleshooting

### Issue: CORS errors in browser console

**Symptoms:**
```
Access to fetch at 'http://10.210.5.131/api/...' has been blocked by CORS policy
```

**Solution:**
1. Check that `.env` has correct `CLIENT_URL_PROXY` variables
2. Restart Docker containers: `docker-compose restart api`
3. Check nginx logs: `sudo tail -f /var/log/nginx/nerbabo_error.log`

### Issue: 502 Bad Gateway

**Symptoms:**
Nginx returns 502 error when accessing the app.

**Solution:**
1. Check Docker containers are running: `docker-compose ps`
2. Check backend health: `curl http://localhost:5001/health`
3. Check frontend health: `curl http://localhost:4200`
4. Review nginx error logs: `sudo tail -f /var/log/nginx/error.log`

### Issue: Frontend shows "Connection refused"

**Symptoms:**
Frontend loads but can't connect to API.

**Solution:**
1. Check browser console for actual error
2. Verify API URL detection:
   - Press F12 → Console
   - Type: `localStorage` or check Network tab for API calls
3. Ensure nginx is routing `/api/` correctly:
   ```bash
   curl -v http://localhost/api/general-info/alive
   ```

### Issue: Uploads fail with 413 Request Entity Too Large

**Solution:**
Already configured in `nginx-reverse-proxy.conf`:
```nginx
client_max_body_size 50M;
```

If you need larger uploads, increase this value and reload nginx.

---

## Testing Checklist

After deployment, verify:

- [ ] Frontend loads: `http://YOUR_SERVER_IP/`
- [ ] Login works
- [ ] API calls succeed (check browser Network tab)
- [ ] File uploads work
- [ ] Images display correctly
- [ ] Health check: `http://YOUR_SERVER_IP/health`
- [ ] No CORS errors in console
- [ ] No 502/504 errors

---

## Monitoring

### Check Application Logs

```bash
# Backend API logs
docker-compose logs -f api

# Frontend logs
docker-compose logs -f angular

# Database logs
docker-compose logs -f postgres

# Nginx access logs
sudo tail -f /var/log/nginx/nerbabo_access.log

# Nginx error logs
sudo tail -f /var/log/nginx/nerbabo_error.log
```

### Health Checks

```bash
# API health
curl http://localhost/health

# Docker container health
docker-compose ps
```

---

## Rollback Procedure

If something goes wrong:

```bash
# Restore nginx configuration
sudo cp /etc/nginx/sites-available/nerbabo.backup /etc/nginx/sites-available/nerbabo
sudo nginx -t
sudo systemctl reload nginx

# Restore Docker environment
cd /home/nerba/NERBA/NERBA-Backoffice
git checkout -- .env docker-compose.yml

# Restart containers
docker-compose restart
```

---

## Summary of Files Changed

### Frontend
- ✅ `NERBABO.Frontend/src/environments/environment.production.ts`
- ✅ `NERBABO.Frontend/src/app/core/services/api-config.service.ts`

### Backend
- ✅ `NERBABO.Backend/NERBABO.ApiService/Program.cs`
- ✅ `NERBABO.Backend/NERBABO.ApiService/Helper/DnsHelper.cs`

### Configuration
- ✅ `docker-compose.yml`
- ✅ `.env`
- ✅ `.env.example`
- ✅ `start.sh`

### New Files
- ✅ `nginx-reverse-proxy.conf` (improved nginx configuration)
- ✅ `NGINX-DEPLOYMENT-GUIDE.md` (this file)

---

## Next Steps

1. **Test the changes locally** with nginx reverse proxy
2. **Enable HTTPS** for production deployment
3. **Set up monitoring** and log aggregation
4. **Configure backups** for database and uploads
5. **Set up CI/CD** pipeline if needed

---

## Support

For issues or questions:
- Check logs: Backend, Frontend, Nginx
- Review configuration: `.env`, `docker-compose.yml`, nginx config
- Verify network connectivity: `curl`, `docker-compose ps`, `nginx -t`

---

**Document Version:** 1.0
**Last Updated:** 2025-11-24
**Prepared for:** NERBABO Backoffice Production Deployment

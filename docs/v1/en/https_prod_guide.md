# HTTPS Configuration Guide for NERBA Backoffice

This comprehensive guide explains how to enable HTTPS for the NERBA Backoffice application across different deployment scenarios.

## Table of Contents

- [Overview](#overview)
- [Deployment Scenarios](#deployment-scenarios)
- [Option 1: Nginx Reverse Proxy with Let's Encrypt (Recommended for Production)](#option-1-nginx-reverse-proxy-with-lets-encrypt-recommended-for-production)
- [Option 2: Nginx Reverse Proxy with Self-Signed Certificates](#option-2-nginx-reverse-proxy-with-self-signed-certificates)
- [Option 3: Direct Docker Container HTTPS](#option-3-direct-docker-container-https)
- [Option 4: ASP.NET Core Kestrel HTTPS Configuration](#option-4-aspnet-core-kestrel-https-configuration)
- [Security Best Practices](#security-best-practices)
- [Troubleshooting](#troubleshooting)
- [Certificate Renewal](#certificate-renewal)

---

## Overview

The NERBA Backoffice application can be configured to use HTTPS in multiple ways depending on your deployment scenario:

1. **Nginx Reverse Proxy** (Recommended): SSL/TLS termination at nginx level
2. **Direct Container HTTPS**: HTTPS enabled directly in Docker containers
3. **Kestrel HTTPS**: ASP.NET Core built-in HTTPS support

**Current Architecture:**
- Frontend: Angular 19 + nginx (runs on port 4200 in Docker)
- Backend: ASP.NET Core Web API (runs on port 5001 in Docker)
- Nginx Reverse Proxy: Routes traffic to containers (optional, port 80/443)

---

## Deployment Scenarios

### Scenario A: Production with Domain Name
**Use:** Option 1 - Nginx Reverse Proxy with Let's Encrypt
- Free, trusted SSL certificates
- Automatic renewal
- Best for internet-facing applications

### Scenario B: Internal/Private Network
**Use:** Option 2 - Nginx Reverse Proxy with Self-Signed Certificates
- No domain required
- Works with IP addresses
- Suitable for internal company networks

### Scenario C: Development Environment
**Use:** Option 3 or 4 - Self-signed certificates for containers
- Quick setup for local testing
- Good for HTTPS-related development work

---

## Option 1: Nginx Reverse Proxy with Let's Encrypt (Recommended for Production)

### Prerequisites

- A registered domain name pointing to your server's public IP
- Server accessible on ports 80 and 443 from the internet
- Root/sudo access to the server

### Step 1: Install Required Software

```bash
# Ubuntu/Debian
sudo apt update
sudo apt install nginx certbot python3-certbot-nginx

# CentOS/RHEL
sudo yum install nginx certbot python3-certbot-nginx

# Verify installation
nginx -v
certbot --version
```

### Step 2: Configure DNS

Ensure your domain points to your server:

```bash
# Check DNS propagation
nslookup yourdomain.com
dig yourdomain.com

# Should return your server's public IP address
```

### Step 3: Initial Nginx Configuration

Copy the provided nginx reverse proxy configuration:

```bash
sudo cp nginx-reverse-proxy.conf /etc/nginx/sites-available/nerbabo

# Ubuntu/Debian
sudo ln -sf /etc/nginx/sites-available/nerbabo /etc/nginx/sites-enabled/

# Remove default configuration
sudo rm -f /etc/nginx/sites-enabled/default

# Test configuration
sudo nginx -t

# Restart nginx
sudo systemctl restart nginx
```

### Step 4: Obtain Let's Encrypt Certificate

```bash
# Obtain certificate (interactive mode)
sudo certbot --nginx -d yourdomain.com -d www.yourdomain.com

# Or non-interactive mode
sudo certbot --nginx \
  -d yourdomain.com \
  -d www.yourdomain.com \
  --non-interactive \
  --agree-tos \
  --email your-email@example.com \
  --redirect
```

**What this does:**
- Obtains SSL certificate from Let's Encrypt
- Automatically configures nginx for HTTPS
- Sets up HTTP to HTTPS redirect
- Configures auto-renewal

### Step 5: Update Nginx Configuration for Application

Edit `/etc/nginx/sites-available/nerbabo` to enable the SSL sections:

```nginx
# Uncomment and update the HTTP to HTTPS redirect section
server {
    listen 80;
    listen [::]:80;
    server_name yourdomain.com www.yourdomain.com;

    # Redirect all HTTP traffic to HTTPS
    return 301 https://$server_name$request_uri;
}

# Main HTTPS Server Block
server {
    listen 443 ssl http2;
    listen [::]:443 ssl http2;
    server_name yourdomain.com www.yourdomain.com;

    # SSL Configuration (certbot will add these automatically)
    ssl_certificate /etc/letsencrypt/live/yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/yourdomain.com/privkey.pem;

    # Enhanced SSL Security
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers 'ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256:ECDHE-ECDSA-AES256-GCM-SHA384:ECDHE-RSA-AES256-GCM-SHA384:ECDHE-ECDSA-CHACHA20-POLY1305:ECDHE-RSA-CHACHA20-POLY1305:DHE-RSA-AES128-GCM-SHA256:DHE-RSA-AES256-GCM-SHA384';
    ssl_prefer_server_ciphers off;

    # SSL Session Configuration
    ssl_session_cache shared:SSL:10m;
    ssl_session_timeout 10m;
    ssl_session_tickets off;

    # OCSP Stapling
    ssl_stapling on;
    ssl_stapling_verify on;
    ssl_trusted_certificate /etc/letsencrypt/live/yourdomain.com/chain.pem;
    resolver 8.8.8.8 8.8.4.4 valid=300s;
    resolver_timeout 5s;

    # Security Headers
    add_header Strict-Transport-Security "max-age=63072000; includeSubDomains; preload" always;
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header Referrer-Policy "strict-origin-when-cross-origin" always;

    # ... (rest of the location blocks from nginx-reverse-proxy.conf)
    # Keep all existing location blocks unchanged
}
```

### Step 6: Update Environment Variables

Update your `.env` file:

```bash
# JWT Configuration - Update to HTTPS
JWT_ISSUER=https://yourdomain.com

# Client URLs - HTTPS
CLIENT_URL=https://yourdomain.com
CLIENT_URL_HTTPS=https://yourdomain.com
CLIENT_URL_PROXY=https://yourdomain.com
CLIENT_URL_PROXY_HTTPS=https://yourdomain.com
```

### Step 7: Deploy Application

```bash
# Restart nginx
sudo nginx -t && sudo systemctl reload nginx

# Deploy/restart Docker containers
docker-compose down
docker-compose up -d --build
```

### Step 8: Verify HTTPS Configuration

```bash
# Test SSL certificate
curl -I https://yourdomain.com

# Check SSL Labs rating
# Visit: https://www.ssllabs.com/ssltest/analyze.html?d=yourdomain.com

# Test automatic renewal
sudo certbot renew --dry-run
```

---

## Option 2: Nginx Reverse Proxy with Self-Signed Certificates

Suitable for internal networks or when you don't have a domain name.

### Step 1: Generate Self-Signed Certificate

```bash
# Create directory for certificates
sudo mkdir -p /etc/nginx/ssl

# Generate self-signed certificate (valid for 365 days)
sudo openssl req -x509 -nodes -days 365 -newkey rsa:4096 \
  -keyout /etc/nginx/ssl/nerbabo.key \
  -out /etc/nginx/ssl/nerbabo.crt \
  -subj "/C=PT/ST=Braganca/L=Braganca/O=NERBA/OU=IT/CN=nerbabo-backoffice"

# Generate stronger DH parameters (optional but recommended)
sudo openssl dhparam -out /etc/nginx/ssl/dhparam.pem 2048

# Set proper permissions
sudo chmod 600 /etc/nginx/ssl/nerbabo.key
sudo chmod 644 /etc/nginx/ssl/nerbabo.crt
```

**Advanced: Generate certificate with Subject Alternative Names (SAN)**

For supporting multiple IPs/hostnames:

```bash
# Create OpenSSL configuration file
cat > /tmp/openssl.cnf << EOF
[req]
default_bits = 4096
prompt = no
default_md = sha256
distinguished_name = dn
req_extensions = v3_req

[dn]
C=PT
ST=Braganca
L=Braganca
O=NERBA
OU=IT Department
CN=nerbabo-backoffice

[v3_req]
basicConstraints = CA:FALSE
keyUsage = nonRepudiation, digitalSignature, keyEncipherment
subjectAltName = @alt_names

[alt_names]
DNS.1 = localhost
DNS.2 = nerbabo.local
IP.1 = 192.168.1.100
IP.2 = 127.0.0.1
EOF

# Generate certificate with SAN
sudo openssl req -x509 -nodes -days 365 -newkey rsa:4096 \
  -keyout /etc/nginx/ssl/nerbabo.key \
  -out /etc/nginx/ssl/nerbabo.crt \
  -config /tmp/openssl.cnf \
  -extensions v3_req

# Clean up
rm /tmp/openssl.cnf
```

### Step 2: Configure Nginx with Self-Signed Certificate

Edit `/etc/nginx/sites-available/nerbabo`:

```nginx
# HTTP to HTTPS redirect
server {
    listen 80;
    listen [::]:80;
    server_name _;

    return 301 https://$host$request_uri;
}

# HTTPS Server
server {
    listen 443 ssl http2;
    listen [::]:443 ssl http2;
    server_name _;

    # Self-Signed Certificate
    ssl_certificate /etc/nginx/ssl/nerbabo.crt;
    ssl_certificate_key /etc/nginx/ssl/nerbabo.key;
    ssl_dhparam /etc/nginx/ssl/dhparam.pem;  # Optional

    # SSL Configuration
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;
    ssl_prefer_server_ciphers on;
    ssl_session_cache shared:SSL:10m;
    ssl_session_timeout 10m;

    # Security Headers (omit HSTS for self-signed certs)
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header Referrer-Policy "strict-origin-when-cross-origin" always;

    # ... (rest of configuration from nginx-reverse-proxy.conf)
}
```

### Step 3: Trust the Self-Signed Certificate

**On the Server:**

```bash
# Ubuntu/Debian
sudo cp /etc/nginx/ssl/nerbabo.crt /usr/local/share/ca-certificates/
sudo update-ca-certificates

# CentOS/RHEL
sudo cp /etc/nginx/ssl/nerbabo.crt /etc/pki/ca-trust/source/anchors/
sudo update-ca-trust
```

**On Client Machines:**

Distribute the certificate file (`nerbabo.crt`) and import it:

**Windows:**
1. Double-click the certificate file
2. Click "Install Certificate"
3. Choose "Local Machine"
4. Place in "Trusted Root Certification Authorities"

**macOS:**
```bash
sudo security add-trusted-cert -d -r trustRoot -k /Library/Keychains/System.keychain nerbabo.crt
```

**Linux:**
```bash
sudo cp nerbabo.crt /usr/local/share/ca-certificates/
sudo update-ca-certificates
```

**Browser (Manual):**
- Chrome/Edge: Settings → Privacy → Security → Manage certificates → Authorities → Import
- Firefox: Settings → Privacy → Certificates → View Certificates → Authorities → Import

### Step 4: Update Environment Variables and Deploy

Same as Option 1, Step 6 and 7, but use your IP or internal hostname instead of domain.

---

## Option 3: Direct Docker Container HTTPS

Enable HTTPS directly in the Angular nginx container and .NET API container.

### Step 1: Generate Certificates for Containers

```bash
# Create certificates directory
mkdir -p ./certs

# Generate certificate for frontend
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout ./certs/frontend.key \
  -out ./certs/frontend.crt \
  -subj "/CN=localhost"

# Generate certificate for API
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout ./certs/api.key \
  -out ./certs/api.crt \
  -subj "/CN=localhost"

# For .NET dev certs (alternative for API)
dotnet dev-certs https -ep ./certs/aspnetapp.pfx -p YourSecurePassword123!
dotnet dev-certs https --trust
```

### Step 2: Update Frontend Nginx Configuration

Edit `NERBABO.Frontend/nginx.conf`:

```nginx
server {
    listen 80;
    listen 443 ssl http2;
    server_name localhost;

    # SSL Configuration
    ssl_certificate /etc/nginx/ssl/frontend.crt;
    ssl_certificate_key /etc/nginx/ssl/frontend.key;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;

    root /usr/share/nginx/html;
    index index.html;

    # ... (rest of existing configuration)
}
```

### Step 3: Update Frontend Dockerfile

Edit `NERBABO.Frontend/Dockerfile`:

```dockerfile
# ... (existing build stage)

FROM nginx:alpine

# Copy SSL certificates
COPY certs/frontend.crt /etc/nginx/ssl/frontend.crt
COPY certs/frontend.key /etc/nginx/ssl/frontend.key

# Copy nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy built app
COPY --from=build /app/dist/nerbabo.frontend/browser/ /usr/share/nginx/html/

# Expose both HTTP and HTTPS
EXPOSE 80 443

CMD ["nginx", "-g", "daemon off;"]
```

### Step 4: Update API for HTTPS (Kestrel)

Edit `NERBABO.Backend/NERBABO.ApiService/appsettings.Production.json`:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://+:8080"
      },
      "Https": {
        "Url": "https://+:8443",
        "Certificate": {
          "Path": "/app/certs/aspnetapp.pfx",
          "Password": "YourSecurePassword123!"
        }
      }
    }
  }
}
```

### Step 5: Update Docker Compose

Edit `docker-compose.yml`:

```yaml
services:
  api:
    build:
      context: .
      dockerfile: NERBABO.Backend/NERBABO.ApiService/Dockerfile
    container_name: nerbabo-api
    restart: unless-stopped
    environment:
      ASPNETCORE_ENVIRONMENT: ${ASPNETCORE_ENVIRONMENT:-Production}
      ASPNETCORE_URLS: http://+:8080;https://+:8443
      ASPNETCORE_Kestrel__Certificates__Default__Password: YourSecurePassword123!
      ASPNETCORE_Kestrel__Certificates__Default__Path: /app/certs/aspnetapp.pfx
      # ... (rest of environment variables)
    ports:
      - "5001:8080"
      - "5002:8443"  # HTTPS port
    volumes:
      - api_uploads:/app/wwwroot
      - ./certs:/app/certs:ro  # Mount certificates as read-only
    # ... (rest of configuration)

  angular:
    build:
      context: ./NERBABO.Frontend
      dockerfile: Dockerfile
    container_name: nerbabo-angular
    restart: unless-stopped
    ports:
      - "4200:80"
      - "4201:443"  # HTTPS port
    # ... (rest of configuration)
```

### Step 6: Update Backend Dockerfile

Edit `NERBABO.Backend/NERBABO.ApiService/Dockerfile`:

Add before the final `EXPOSE` command:

```dockerfile
# ... (existing stages)

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
RUN groupadd -r appuser && useradd -r -g appuser appuser
WORKDIR /app
COPY --from=build /app/publish .
RUN mkdir -p /app/logs /app/temp /app/certs && chown -R appuser:appuser /app

# Expose both HTTP and HTTPS
EXPOSE 8080 8443

USER appuser
ENTRYPOINT ["dotnet", "NERBABO.ApiService.dll"]
```

### Step 7: Update Environment Variables

```bash
# .env file
JWT_ISSUER=https://your-server-ip:5002
CLIENT_URL=https://your-server-ip:4201
CLIENT_URL_HTTPS=https://your-server-ip:4201
```

### Step 8: Deploy

```bash
# Rebuild and restart containers
docker-compose down
docker-compose up -d --build

# Access via HTTPS
# Frontend: https://your-ip:4201
# API: https://your-ip:5002
```

---

## Option 4: ASP.NET Core Kestrel HTTPS Configuration

For development or when you only need HTTPS on the API.

### Method A: Using Development Certificates

```bash
# Trust the development certificate
dotnet dev-certs https --trust

# Run with HTTPS enabled
cd NERBABO.Backend/NERBABO.AppHost
ASPNETCORE_URLS="https://localhost:5001;http://localhost:5000" dotnet run
```

### Method B: Using Custom Certificate

Create `NERBABO.Backend/NERBABO.ApiService/appsettings.Development.json`:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5000"
      },
      "Https": {
        "Url": "https://localhost:5001",
        "Certificate": {
          "Path": "localhost.pfx",
          "Password": "YourPassword"
        }
      }
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

Generate the certificate:

```bash
# Generate certificate
openssl req -x509 -newkey rsa:4096 -sha256 -days 365 -nodes \
  -keyout localhost.key -out localhost.crt \
  -subj "/CN=localhost" \
  -addext "subjectAltName=DNS:localhost,IP:127.0.0.1"

# Convert to PFX for .NET
openssl pkcs12 -export -out localhost.pfx \
  -inkey localhost.key -in localhost.crt \
  -password pass:YourPassword

# Move to API project
mv localhost.pfx NERBABO.Backend/NERBABO.ApiService/
```

---

## Security Best Practices

### 1. SSL/TLS Configuration

**Strong Protocol Support:**
- Use only TLS 1.2 and TLS 1.3
- Disable older protocols (SSLv3, TLS 1.0, TLS 1.1)

**Cipher Suites:**
```nginx
ssl_ciphers 'ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256:ECDHE-ECDSA-AES256-GCM-SHA384:ECDHE-RSA-AES256-GCM-SHA384:ECDHE-ECDSA-CHACHA20-POLY1305:ECDHE-RSA-CHACHA20-POLY1305';
ssl_prefer_server_ciphers off;
```

### 2. HTTP Strict Transport Security (HSTS)

Only use with valid certificates (not self-signed):

```nginx
add_header Strict-Transport-Security "max-age=31536000; includeSubDomains; preload" always;
```

### 3. Certificate Management

- Store private keys with restricted permissions (600 or 400)
- Use separate certificates for different environments
- Implement certificate monitoring and alerts for expiration
- Keep certificates in secure locations outside web root

### 4. Regular Security Updates

```bash
# Update system packages
sudo apt update && sudo apt upgrade -y

# Update nginx
sudo apt install --only-upgrade nginx

# Update Docker and containers
docker-compose pull
docker-compose up -d --build
```

### 5. Firewall Configuration

```bash
# Ubuntu/Debian with UFW
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw enable

# CentOS/RHEL with firewalld
sudo firewall-cmd --permanent --add-service=http
sudo firewall-cmd --permanent --add-service=https
sudo firewall-cmd --reload
```

### 6. Content Security Policy

Add to nginx configuration:

```nginx
add_header Content-Security-Policy "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:; connect-src 'self'" always;
```

### 7. Rate Limiting

Protect against DDoS:

```nginx
# In nginx http block
limit_req_zone $binary_remote_addr zone=api_limit:10m rate=10r/s;
limit_req_zone $binary_remote_addr zone=login_limit:10m rate=5r/m;

# In server block
location /api/ {
    limit_req zone=api_limit burst=20 nodelay;
    # ... rest of config
}

location /api/auth/login {
    limit_req zone=login_limit burst=3 nodelay;
    # ... rest of config
}
```

---

## Troubleshooting

### Common Issues

#### 1. Certificate Chain Issues

**Error:** "SSL certificate problem: unable to get local issuer certificate"

**Solution:**
```bash
# For Let's Encrypt, ensure full chain is used
ssl_certificate /etc/letsencrypt/live/yourdomain.com/fullchain.pem;

# Verify certificate chain
openssl s_client -connect yourdomain.com:443 -showcerts
```

#### 2. Mixed Content Warnings

**Error:** "Mixed Content: The page was loaded over HTTPS, but requested an insecure resource"

**Solution:**
- Ensure all API calls use HTTPS
- Update environment variables to use `https://`
- Check browser console for specific resources
- Update Angular `environment.prod.ts`:

```typescript
export const environment = {
  production: true,
  apiUrl: 'https://yourdomain.com/api'  // Not http://
};
```

#### 3. CORS Issues with HTTPS

**Error:** "Access to fetch blocked by CORS policy"

**Solution:**
Update `.env`:
```bash
CLIENT_URL_HTTPS=https://yourdomain.com
CLIENT_URL_PROXY_HTTPS=https://yourdomain.com
```

Verify backend CORS configuration in `Program.cs`:
```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            builder.Configuration["CORS:AllowedOrigins:0"],  // HTTP
            builder.Configuration["CORS:AllowedOrigins:1"],  // HTTPS
            builder.Configuration["CORS:AllowedOrigins:2"],  // Proxy HTTP
            builder.Configuration["CORS:AllowedOrigins:3"]   // Proxy HTTPS
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});
```

#### 4. Certificate Not Trusted

**Self-Signed Certificates:**
- Import certificate to system trust store (see Option 2, Step 3)
- For development, accept the browser warning

**Let's Encrypt:**
- Verify certificate installation: `sudo certbot certificates`
- Check certificate expiry: `openssl x509 -in /etc/letsencrypt/live/yourdomain.com/cert.pem -noout -dates`

#### 5. Nginx Configuration Errors

**Test configuration:**
```bash
sudo nginx -t

# Common errors:
# - Missing semicolon
# - Invalid directive
# - Certificate file not found
```

**View detailed error logs:**
```bash
sudo tail -f /var/log/nginx/error.log
```

#### 6. Docker Container Certificate Issues

**Error:** Container can't find certificate files

**Solution:**
```bash
# Verify volume mount
docker inspect nerbabo-api | grep -A 10 Mounts

# Check file permissions inside container
docker exec nerbabo-api ls -la /app/certs

# Rebuild with proper certificates
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```

#### 7. Port Already in Use

**Error:** "bind() to 0.0.0.0:443 failed (98: Address already in use)"

**Solution:**
```bash
# Check what's using port 443
sudo lsof -i :443
sudo netstat -tulpn | grep :443

# Stop conflicting service
sudo systemctl stop apache2  # If Apache is running
```

---

## Certificate Renewal

### Let's Encrypt Auto-Renewal

Certbot automatically sets up renewal. Verify it:

```bash
# Test renewal process (dry run)
sudo certbot renew --dry-run

# Check certbot timer (systemd)
sudo systemctl status certbot.timer

# Manual renewal (if needed)
sudo certbot renew

# Post-renewal hook to reload nginx
sudo certbot renew --deploy-hook "systemctl reload nginx"
```

### Self-Signed Certificate Renewal

Create a script `renew-ssl.sh`:

```bash
#!/bin/bash

# Backup old certificates
sudo cp /etc/nginx/ssl/nerbabo.crt /etc/nginx/ssl/nerbabo.crt.$(date +%Y%m%d).bak
sudo cp /etc/nginx/ssl/nerbabo.key /etc/nginx/ssl/nerbabo.key.$(date +%Y%m%d).bak

# Generate new certificate
sudo openssl req -x509 -nodes -days 365 -newkey rsa:4096 \
  -keyout /etc/nginx/ssl/nerbabo.key \
  -out /etc/nginx/ssl/nerbabo.crt \
  -subj "/C=PT/ST=Braganca/L=Braganca/O=NERBA/OU=IT/CN=nerbabo-backoffice"

# Set permissions
sudo chmod 600 /etc/nginx/ssl/nerbabo.key
sudo chmod 644 /etc/nginx/ssl/nerbabo.crt

# Test nginx config
sudo nginx -t

# Reload nginx
if [ $? -eq 0 ]; then
    sudo systemctl reload nginx
    echo "SSL certificate renewed successfully"
else
    echo "Nginx configuration test failed. Certificate not renewed."
    # Restore backup
    sudo cp /etc/nginx/ssl/nerbabo.crt.$(date +%Y%m%d).bak /etc/nginx/ssl/nerbabo.crt
    sudo cp /etc/nginx/ssl/nerbabo.key.$(date +%Y%m%d).bak /etc/nginx/ssl/nerbabo.key
fi
```

Schedule renewal:

```bash
# Make executable
chmod +x renew-ssl.sh

# Add to crontab (renew every 6 months)
crontab -e

# Add this line:
0 0 1 */6 * /path/to/renew-ssl.sh >> /var/log/ssl-renewal.log 2>&1
```

---

## Testing HTTPS Configuration

### 1. Browser Testing

Access your application:
- `https://yourdomain.com` or `https://your-ip`
- Check for padlock icon in address bar
- Click padlock to view certificate details

### 2. SSL Labs Test

For public domains:
```
https://www.ssllabs.com/ssltest/analyze.html?d=yourdomain.com
```

Aim for A+ rating.

### 3. Command Line Testing

```bash
# Test HTTPS connection
curl -I https://yourdomain.com

# Test with verbose output
curl -v https://yourdomain.com

# Check certificate details
openssl s_client -connect yourdomain.com:443 -showcerts

# Test TLS versions
openssl s_client -connect yourdomain.com:443 -tls1_2
openssl s_client -connect yourdomain.com:443 -tls1_3

# Verify certificate expiration
echo | openssl s_client -connect yourdomain.com:443 2>/dev/null | openssl x509 -noout -dates
```

### 4. Test API Endpoints

```bash
# Test health endpoint
curl https://yourdomain.com/health

# Test API with authentication
curl -X POST https://yourdomain.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"usernameOrEmail":"admin","password":"your-password"}'
```

### 5. Browser Developer Tools

Open Developer Tools (F12):
- **Console:** Check for mixed content warnings
- **Network:** Verify all requests use HTTPS
- **Security:** View certificate and connection details

---

## Monitoring and Maintenance

### Certificate Expiration Monitoring

Create monitoring script `check-ssl-expiry.sh`:

```bash
#!/bin/bash

DOMAIN="yourdomain.com"
ALERT_DAYS=30

# Get expiry date
EXPIRY_DATE=$(echo | openssl s_client -connect $DOMAIN:443 2>/dev/null | openssl x509 -noout -enddate | cut -d= -f2)
EXPIRY_EPOCH=$(date -d "$EXPIRY_DATE" +%s)
NOW_EPOCH=$(date +%s)
DAYS_LEFT=$(( ($EXPIRY_EPOCH - $NOW_EPOCH) / 86400 ))

if [ $DAYS_LEFT -lt $ALERT_DAYS ]; then
    echo "WARNING: SSL certificate for $DOMAIN expires in $DAYS_LEFT days!"
    # Send email or notification here
else
    echo "SSL certificate for $DOMAIN is valid for $DAYS_LEFT more days"
fi
```

### Nginx Log Monitoring

```bash
# Monitor access logs for HTTPS
sudo tail -f /var/log/nginx/nerbabo_access.log | grep "HTTPS\|443"

# Monitor error logs
sudo tail -f /var/log/nginx/nerbabo_error.log

# Check SSL handshake errors
sudo grep "SSL_do_handshake" /var/log/nginx/error.log
```

### Performance Monitoring

Monitor SSL/TLS performance impact:

```bash
# Install monitoring tools
sudo apt install nginx-extras

# Enable stub_status in nginx
location /nginx_status {
    stub_status on;
    access_log off;
    allow 127.0.0.1;
    deny all;
}

# Check status
curl http://localhost/nginx_status
```

---

## Additional Resources

### Official Documentation

- **Let's Encrypt:** https://letsencrypt.org/docs/
- **Nginx SSL Configuration:** https://nginx.org/en/docs/http/configuring_https_servers.html
- **ASP.NET Core HTTPS:** https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl
- **Mozilla SSL Configuration Generator:** https://ssl-config.mozilla.org/

### SSL/TLS Testing Tools

- **SSL Labs:** https://www.ssllabs.com/ssltest/
- **Security Headers:** https://securityheaders.com/
- **Certificate Transparency:** https://crt.sh/

### Certificate Providers

- **Let's Encrypt:** Free, automated certificates
- **ZeroSSL:** Free alternative to Let's Encrypt
- **Cloudflare SSL:** Free with Cloudflare account
- **Commercial CAs:** DigiCert, GlobalSign, Sectigo

---

## Summary

| Scenario | Recommended Option | Certificate Type | Difficulty | Cost |
|----------|-------------------|------------------|------------|------|
| Production with domain | Option 1 | Let's Encrypt | Medium | Free |
| Internal network | Option 2 | Self-signed | Easy | Free |
| Development | Option 3/4 | Self-signed | Easy | Free |
| Enterprise | Option 1 | Commercial CA | Medium | Paid |

**Quick Recommendations:**

1. **For production with a domain:** Use Option 1 (Nginx + Let's Encrypt)
2. **For internal use without domain:** Use Option 2 (Nginx + Self-signed)
3. **For development:** Use Option 4 (dotnet dev-certs)
4. **For maximum control:** Use Option 3 (Docker containers with custom certs)

All options are valid and secure when properly configured. Choose based on your specific requirements and infrastructure.

---

## Support

For issues or questions:
- Check the troubleshooting section
- Review nginx error logs: `/var/log/nginx/error.log`
- Review application logs: `docker-compose logs -f`
- Open an issue: https://github.com/bernardoNerba/NERBA-Backoffice/issues

---

**Last Updated:** 2026-01-04
**Author:** NERBA IT Team
**License:** Apache License 2.0

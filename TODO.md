# NERBA Backoffice - Railway Deployment TODO

## 🚂 Railway Deployment Checklist

### 1. Pre-deployment Setup
- [ ] Create Railway account at https://railway.app
- [ ] Install Railway CLI: `npm install -g @railway/cli`
- [ ] Login to Railway: `railway login`

### 2. Database Setup
- [ ] Create new Railway project
- [ ] Add PostgreSQL plugin to Railway project
- [ ] Note PostgreSQL connection string from Railway dashboard
- [ ] Add Redis plugin (if needed for caching/sessions)
- [ ] Note Redis connection string from Railway dashboard

### 3. Backend API Deployment
- [ ] Create new Railway service for API
- [ ] Connect service to GitHub repository
- [ ] Configure build settings:
  - [ ] Set root directory to `NERBABO.Backend/NERBABO.ApiService/`
  - [ ] Confirm Dockerfile detection
- [ ] Set environment variables in Railway dashboard:
  - [ ] `ASPNETCORE_ENVIRONMENT=Production`
  - [ ] `ASPNETCORE_URLS=http://+:$PORT`
  - [ ] `ConnectionStrings__postgres=<Railway PostgreSQL URL>`
  - [ ] `ConnectionStrings__redis=<Railway Redis URL>`
  - [ ] `JWT__Key=<secure-random-key>`
  - [ ] `JWT__ExpiresInDays=7`
  - [ ] `JWT__Issuer=<API-domain-from-railway>`
  - [ ] `JWT__ClientUrl=<Frontend-domain-from-railway>`
  - [ ] `Admin__username=admin`
  - [ ] `Admin__email=admin@example.com`
  - [ ] `Admin__password=<secure-admin-password>`
  - [ ] `CORS__AllowedOrigins__0=<Frontend-domain-from-railway>`
- [ ] Deploy and verify API health endpoint

### 4. Frontend Deployment
- [ ] Create new Railway service for Frontend
- [ ] Connect service to GitHub repository
- [ ] Configure build settings:
  - [ ] Set root directory to `NERBABO.Frontend/`
  - [ ] Confirm Dockerfile detection
- [ ] Update Angular environment configuration:
  - [ ] Create/update `src/environments/environment.prod.ts`
  - [ ] Set `apiUrl` to Railway API domain
- [ ] Deploy frontend service
- [ ] Test frontend-to-API connectivity

### 5. Configuration Files
- [ ] Create `railway.toml` in project root (optional)
- [ ] Update CORS settings in backend to allow Railway frontend domain
- [ ] Configure nginx.conf for proper routing (if needed)

### 6. Domain & SSL Setup
- [ ] Configure custom domains (optional)
- [ ] Verify SSL certificates are auto-generated
- [ ] Update environment variables with final domains

### 7. Testing & Verification
- [ ] Test API endpoints via Railway domain
- [ ] Test frontend application functionality
- [ ] Verify database connectivity
- [ ] Test user authentication flow
- [ ] Test file uploads (if applicable)
- [ ] Monitor Railway deployment logs

### 8. Post-deployment
- [ ] Set up monitoring/alerting
- [ ] Configure backup strategy for database
- [ ] Document deployment process
- [ ] Update README with live URLs
- [ ] Set up CI/CD pipeline for automatic deployments

## 📝 Important Notes

### Environment Variables Needed
```bash
# Backend API
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:$PORT
ConnectionStrings__postgres=postgresql://...
ConnectionStrings__redis=redis://...
JWT__Key=your-secure-jwt-key-here
JWT__ExpiresInDays=7
JWT__Issuer=https://your-api.railway.app
JWT__ClientUrl=https://your-frontend.railway.app
Admin__username=admin
Admin__email=admin@example.com
Admin__password=SecurePassword123!
CORS__AllowedOrigins__0=https://your-frontend.railway.app
ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
```

### Frontend Environment Update
```typescript
// src/environments/environment.prod.ts
export const environment = {
  production: true,
  apiUrl: 'https://your-api.railway.app'
};
```

### Useful Railway CLI Commands
```bash
railway login                    # Login to Railway
railway link                     # Link local project to Railway
railway status                   # Check deployment status
railway logs                     # View application logs
railway shell                    # Access service shell
railway run <command>            # Run command in Railway environment
```

## 🔗 Helpful Links
- [Railway Documentation](https://docs.railway.app/)
- [Railway .NET Deployment Guide](https://docs.railway.app/guides/frameworks/dotnet)
- [Railway Angular Deployment Guide](https://docs.railway.app/guides/frameworks/angular)
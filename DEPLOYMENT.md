# Deployment Guide: Separated Frontend + Backend Architecture

This guide covers deploying the IgnaCheck application with **separated frontend and backend** for optimal performance, especially for international users (target market: Germany/Europe).

## Architecture Overview

```
┌────────────────────────────────────────────────────────────┐
│  Production Architecture (Separated)                        │
├────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────────┐         ┌─────────────────────┐     │
│  │  Angular SPA     │         │  ASP.NET Core API   │     │
│  │                  │ HTTPS   │                     │     │
│  │  Azure Static    │────────►│  Azure App Service  │     │
│  │  Web Apps        │  CORS   │                     │     │
│  │  + CDN           │         │  + PostgreSQL       │     │
│  │                  │         │  + Azure Blob       │     │
│  └──────────────────┘         └─────────────────────┘     │
│                                                             │
│  Users in Germany                                           │
│  └─► CDN Edge (Frankfurt) ─► Static files (fast!)         │
│  └─► API (West Europe) ─► Dynamic data                     │
└────────────────────────────────────────────────────────────┘
```

## Benefits of This Architecture

✅ **Global Performance** - CDN serves static files from edge locations near users
✅ **Independent Scaling** - Scale API and frontend separately
✅ **Faster Frontend Deployments** - Update UI without touching backend
✅ **Cost Optimization** - Static hosting is cheaper than running app servers
✅ **Better Caching** - Static assets cached at CDN edge
✅ **Offline Development** - Frontend dev can work with mock API

---

## Option 1: Azure Deployment (Recommended)

### Backend: Azure App Service + PostgreSQL

#### Prerequisites
- Azure subscription
- Azure CLI installed
- .NET 9.0 SDK

#### Step 1: Create Azure Resources

```bash
# Login to Azure
az login

# Create resource group
az group create \
  --name ignacheck-prod \
  --location westeurope  # Close to Germany

# Create PostgreSQL Flexible Server
az postgres flexible-server create \
  --name ignacheck-db-prod \
  --resource-group ignacheck-prod \
  --location westeurope \
  --admin-user adminuser \
  --admin-password "YOUR_SECURE_PASSWORD" \
  --sku-name Standard_B2s \
  --tier Burstable \
  --storage-size 32

# Create database
az postgres flexible-server db create \
  --resource-group ignacheck-prod \
  --server-name ignacheck-db-prod \
  --database-name IgnaCheckDb

# Create App Service Plan
az appservice plan create \
  --name ignacheck-plan-prod \
  --resource-group ignacheck-prod \
  --location westeurope \
  --sku P1V2 \
  --is-linux

# Create Web App
az webapp create \
  --name ignacheck-api-prod \
  --resource-group ignacheck-prod \
  --plan ignacheck-plan-prod \
  --runtime "DOTNET:9.0"
```

#### Step 2: Configure API App Settings

```bash
# Set connection string
az webapp config connection-string set \
  --name ignacheck-api-prod \
  --resource-group ignacheck-prod \
  --connection-string-type PostgreSQL \
  --settings IgnaCheckDb="Server=ignacheck-db-prod.postgres.database.azure.com;Database=IgnaCheckDb;Username=adminuser;Password=YOUR_PASSWORD;SslMode=Require"

# Set app settings
az webapp config appsettings set \
  --name ignacheck-api-prod \
  --resource-group ignacheck-prod \
  --settings \
    ASPNETCORE_ENVIRONMENT="Production" \
    AI__Claude__ApiKey="@Microsoft.KeyVault(SecretUri=https://your-keyvault.vault.azure.net/secrets/ClaudeApiKey)" \
    Jwt__SecretKey="@Microsoft.KeyVault(SecretUri=https://your-keyvault.vault.azure.net/secrets/JwtSecret)" \
    Cors__AllowedOrigins__0="https://app.ignacheck.ai" \
    Cors__AllowedOrigins__1="https://www.ignacheck.ai"
```

#### Step 3: Deploy API

```bash
# Build and publish
cd src/Web
dotnet publish -c Release -o ./publish

# Create deployment package
cd publish
zip -r ../deploy.zip .

# Deploy to Azure
az webapp deploy \
  --name ignacheck-api-prod \
  --resource-group ignacheck-prod \
  --src-path ../deploy.zip \
  --type zip

# Verify deployment
curl https://ignacheck-api-prod.azurewebsites.net/health
```

#### API Deployment URL
Your API will be available at: `https://ignacheck-api-prod.azurewebsites.net/api`

---

### Frontend: Azure Static Web Apps + CDN

#### Prerequisites
- Azure subscription
- Node.js 18+
- Azure CLI

#### Step 1: Build Angular for Production

```bash
cd src/Web/ClientApp

# Update environment.prod.ts with your API URL
# apiUrl: 'https://ignacheck-api-prod.azurewebsites.net/api'

# Install dependencies
npm install

# Build for production
npm run build --configuration=production

# Output will be in ClientApp/dist/
```

#### Step 2: Create Azure Static Web App

```bash
# Create Static Web App
az staticwebapp create \
  --name ignacheck-app \
  --resource-group ignacheck-prod \
  --location westeurope \
  --sku Standard  # Standard for custom domain + CDN

# Get deployment token
az staticwebapp secrets list \
  --name ignacheck-app \
  --resource-group ignacheck-prod \
  --query properties.apiKey -o tsv
```

#### Step 3: Deploy Frontend

**Option A: Using Azure CLI**
```bash
cd ClientApp/dist

az staticwebapp upload \
  --name ignacheck-app \
  --resource-group ignacheck-prod \
  --app-location .
```

**Option B: Using GitHub Actions** (Recommended for CI/CD)

Create `.github/workflows/azure-static-web-apps.yml`:

```yaml
name: Deploy Frontend to Azure Static Web Apps

on:
  push:
    branches:
      - main
    paths:
      - 'src/Web/ClientApp/**'

jobs:
  build_and_deploy:
    runs-on: ubuntu-latest
    name: Build and Deploy Frontend
    steps:
      - uses: actions/checkout@v3

      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '18'

      - name: Install dependencies
        run: |
          cd src/Web/ClientApp
          npm ci

      - name: Build Angular
        run: |
          cd src/Web/ClientApp
          npm run build -- --configuration production

      - name: Deploy to Azure Static Web Apps
        uses: Azure/static-web-apps-deploy@v1
        with:
          azure_static_web_apps_api_token: ${{ secrets.AZURE_STATIC_WEB_APPS_API_TOKEN }}
          repo_token: ${{ secrets.GITHUB_TOKEN }}
          action: "upload"
          app_location: "src/Web/ClientApp/dist"
          skip_app_build: true
```

#### Step 4: Configure Custom Domain

```bash
# Add custom domain
az staticwebapp hostname set \
  --name ignacheck-app \
  --resource-group ignacheck-prod \
  --hostname app.ignacheck.ai

# Get CNAME record
az staticwebapp hostname show \
  --name ignacheck-app \
  --resource-group ignacheck-prod \
  --hostname app.ignacheck.ai
```

Then add CNAME record to your DNS:
```
app.ignacheck.ai CNAME unique-id.azurestaticapps.net
```

#### Frontend URL
Your frontend will be available at: `https://app.ignacheck.ai`

---

## Option 2: Alternative Hosting Platforms

### Backend: Azure + Railway/Render

If you prefer simpler backend hosting:

**Railway.app:**
```bash
# Install Railway CLI
npm i -g @railway/cli

# Login
railway login

# Create project
railway init

# Deploy API
cd src/Web
railway up
```

**Render.com:**
1. Connect GitHub repo
2. Create Web Service
3. Build command: `dotnet publish -c Release`
4. Start command: `dotnet Web.dll`

### Frontend: Vercel (Excellent for Angular + CDN)

**Why Vercel:**
- ✅ Excellent global CDN (including Europe)
- ✅ Automatic SSL
- ✅ Easy GitHub integration
- ✅ Free tier available
- ✅ Optimized for Angular

**Deploy to Vercel:**

```bash
# Install Vercel CLI
npm i -g vercel

# Deploy
cd src/Web/ClientApp
vercel --prod
```

**Or using Vercel GitHub Integration:**
1. Go to vercel.com
2. Import your GitHub repo
3. Configure:
   - Framework: Angular
   - Root Directory: `src/Web/ClientApp`
   - Build Command: `npm run build -- --configuration production`
   - Output Directory: `dist`
4. Add environment variable: `API_URL=https://your-api-url.com/api`
5. Deploy!

---

## Development Workflow

### Backend Developer

```bash
# Start API locally
cd src/Web
dotnet run

# API runs on https://localhost:5001
# Swagger: https://localhost:5001/swagger

# Deploy to production
git push origin main  # CI/CD handles deployment
```

### Frontend Developer (Fully Offline)

**Option A: Use Mock API (Offline)**
```bash
cd src/Web/ClientApp

# Start mock server
npm run mock-api

# Start Angular
npm start

# Open https://localhost:44447
```

**Option B: Connect to Dev API**
```bash
# Update proxy.conf.js to point to dev API
# Or update environment.ts

npm start
```

---

## CORS Configuration

### Backend: Update appsettings.Production.json

```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://app.ignacheck.ai",
      "https://www.ignacheck.ai",
      "https://ignacheck-app.azurestaticapps.net"
    ]
  }
}
```

### Or using Azure App Settings:

```bash
az webapp config appsettings set \
  --name ignacheck-api-prod \
  --resource-group ignacheck-prod \
  --settings \
    Cors__AllowedOrigins__0="https://app.ignacheck.ai" \
    Cors__AllowedOrigins__1="https://www.ignacheck.ai"
```

---

## CI/CD Pipeline

### GitHub Actions: Backend Deployment

Create `.github/workflows/deploy-api.yml`:

```yaml
name: Deploy API to Azure

on:
  push:
    branches:
      - main
    paths:
      - 'src/**/*.cs'
      - 'src/**/*.csproj'

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --configuration Release --no-restore

      - name: Publish
        run: dotnet publish src/Web/Web.csproj -c Release -o ./publish

      - name: Deploy to Azure Web App
        uses: azure/webapps-deploy@v2
        with:
          app-name: 'ignacheck-api-prod'
          publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
          package: ./publish
```

---

## Cost Estimates (Monthly)

### Azure Deployment

| Resource | Tier | Cost |
|----------|------|------|
| App Service (API) | P1V2 | ~$80 |
| PostgreSQL Flexible | Standard_B2s | ~$30 |
| Static Web Apps | Standard | ~$9 |
| Azure Blob Storage | Standard | ~$5 |
| **Total** | | **~$124/month** |

### Vercel + Azure Hybrid

| Resource | Tier | Cost |
|----------|------|------|
| App Service (API) | P1V2 | ~$80 |
| PostgreSQL Flexible | Standard_B2s | ~$30 |
| Vercel Pro | CDN + Hosting | ~$20 |
| **Total** | | **~$130/month** |

### Budget Option

| Resource | Tier | Cost |
|----------|------|------|
| Azure App Service | B1 | ~$13 |
| PostgreSQL Flexible | B1ms | ~$12 |
| Vercel Hobby | Free tier | $0 |
| **Total** | | **~$25/month** |

---

## Performance Optimization

### For German/European Users

1. **Deploy API to West Europe region**
   ```bash
   --location westeurope
   ```

2. **Use CDN with European edge locations**
   - Azure Static Web Apps (includes CDN)
   - Vercel (excellent European CDN)
   - Cloudflare CDN

3. **Enable Compression**
   Already configured in ASP.NET Core.

4. **Cache Static Assets**
   Angular build includes cache-busting hashes.

5. **Enable HTTP/2**
   Automatically enabled on Azure App Service.

---

## Security Checklist

### API Security

- ✅ HTTPS enforced
- ✅ CORS configured with specific origins
- ✅ JWT authentication with secure secret
- ✅ Secrets stored in Azure Key Vault
- ✅ Database uses SSL/TLS
- ✅ API rate limiting (add if needed)

### Frontend Security

- ✅ Content Security Policy headers
- ✅ HTTPS only
- ✅ No sensitive data in client
- ✅ Environment variables for API URLs
- ✅ XSS protection built into Angular

---

## Monitoring & Logging

### Azure Application Insights

```bash
# Enable Application Insights
az monitor app-insights component create \
  --app ignacheck-insights \
  --location westeurope \
  --resource-group ignacheck-prod

# Get instrumentation key
az monitor app-insights component show \
  --app ignacheck-insights \
  --resource-group ignacheck-prod \
  --query instrumentationKey -o tsv

# Add to app settings
az webapp config appsettings set \
  --name ignacheck-api-prod \
  --resource-group ignacheck-prod \
  --settings APPLICATIONINSIGHTS_CONNECTION_STRING="InstrumentationKey=YOUR_KEY"
```

### Key Metrics to Monitor

- API response times
- Error rates
- Database performance
- CDN cache hit ratio
- User geographic distribution

---

## Rollback Strategy

### API Rollback

```bash
# List deployment slots
az webapp deployment list-publishing-profiles \
  --name ignacheck-api-prod \
  --resource-group ignacheck-prod

# Swap to previous version
az webapp deployment slot swap \
  --name ignacheck-api-prod \
  --resource-group ignacheck-prod \
  --slot staging \
  --target-slot production
```

### Frontend Rollback

**Vercel:**
- Go to Deployments tab
- Click "Promote to Production" on previous deployment

**Azure Static Web Apps:**
```bash
# Redeploy from specific commit
git checkout <previous-commit>
az staticwebapp upload ...
```

---

## Troubleshooting

### CORS Errors

**Problem:** Frontend can't connect to API

**Solution:**
1. Check API CORS configuration:
   ```bash
   az webapp config appsettings list --name ignacheck-api-prod --resource-group ignacheck-prod | grep CORS
   ```
2. Verify frontend URL is in allowed origins
3. Check browser console for actual error
4. Test with curl:
   ```bash
   curl -H "Origin: https://app.ignacheck.ai" -I https://ignacheck-api-prod.azurewebsites.net/api/health
   ```

### API Not Starting

**Check logs:**
```bash
az webapp log tail \
  --name ignacheck-api-prod \
  --resource-group ignacheck-prod
```

### Frontend Shows Wrong API URL

**Check environment:**
1. Verify `environment.prod.ts` has correct API URL
2. Rebuild: `npm run build -- --configuration production`
3. Redeploy

---

## Backup Strategy

### Database Backups

```bash
# Enable automated backups (included with Flexible Server)
az postgres flexible-server backup create \
  --resource-group ignacheck-prod \
  --name ignacheck-db-prod

# Export manual backup
pg_dump -h ignacheck-db-prod.postgres.database.azure.com -U adminuser -d IgnaCheckDb > backup.sql
```

### Application Code

- Automated via Git
- Tag releases: `git tag v1.0.0`
- Keep deployment artifacts

---

## Next Steps

1. ✅ Configure custom domains
2. ✅ Set up CI/CD pipelines
3. ✅ Configure monitoring and alerts
4. ✅ Set up database backups
5. ✅ Load test the application
6. ✅ Configure auto-scaling rules

---

## Support

For deployment issues:
- Azure: https://azure.microsoft.com/support
- Vercel: https://vercel.com/support
- Repository issues: Create GitHub issue

---

**Deployment checklist complete! Your application is now optimized for international users with separated, scalable architecture.** 🚀

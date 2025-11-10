# Developer Guide: Frontend + Backend Development

This project uses a **hybrid approach**: independent development, bundled deployment.

## Architecture Overview

```
Development (Separated)           Production (Bundled)
┌─────────────────────┐          ┌─────────────────────────┐
│  Frontend Dev       │          │   Single Deployment     │
│  npm start          │          │                         │
│  Port 44447         │          │  /api/*  → ASP.NET API  │
│     ↓ proxy         │          │  /*      → Angular SPA  │
│  Backend Dev        │          │                         │
│  dotnet run         │          │  Single artifact        │
│  Port 5001          │          │  wwwroot/ + API DLLs    │
└─────────────────────┘          └─────────────────────────┘
```

---

## Frontend Developer Setup

### Prerequisites
- Node.js 18+ and npm
- Git
- Code editor (VS Code recommended)

### One-Time Setup

```bash
# Clone repository
git clone <repository-url>
cd ignaCheckApi/src/Web/ClientApp

# Install dependencies
npm install
```

### Daily Development Workflow

```bash
# Navigate to Angular app
cd src/Web/ClientApp

# Start Angular dev server
npm start

# Open browser to:
# https://localhost:44447
```

**That's it!** No .NET SDK, no database, no API configuration needed.

### How It Works

- Angular runs on `https://localhost:44447`
- All API calls like `/api/projects` automatically proxy to `https://localhost:5001`
- The `proxy.conf.js` file handles routing
- Hot reload works perfectly

### API Configuration Options

**Option A: Use backend dev's local API** (default)
```javascript
// proxy.conf.js
target: 'https://localhost:5001'
```

**Option B: Use shared development server**
```javascript
// proxy.conf.js
target: 'https://ignacheck-dev.azurewebsites.net'
```

**Option C: Mock API for offline work**
```bash
npm install -D json-server
npm run mock-api  # If configured
```

### Available Commands

```bash
npm start          # Start dev server with hot reload
npm run build      # Build for production
npm test           # Run unit tests
npm run lint       # Lint code
```

### Key Files

```
ClientApp/
├── src/
│   ├── app/                 # Angular components
│   ├── api-authorization/   # Auth components
│   ├── environments/        # Environment configs
│   └── assets/              # Static assets
├── proxy.conf.js           # API proxy configuration
├── angular.json            # Angular CLI config
└── package.json            # Dependencies
```

### Making API Calls

```typescript
// services/project.service.ts
import { HttpClient } from '@angular/common/http';

export class ProjectService {
  constructor(private http: HttpClient) {}

  getProjects() {
    // Just use relative URLs - proxy handles the rest
    return this.http.get('/api/projects');
  }
}
```

### TypeScript API Client

The backend automatically generates TypeScript clients on build:

```typescript
// Auto-generated client at src/app/web-api-client.ts
import { ProjectsClient, CreateProjectCommand } from './web-api-client';

// Use the generated client
constructor(private projectsClient: ProjectsClient) {}

createProject() {
  const command = new CreateProjectCommand({
    name: 'New Project',
    description: 'Test'
  });

  this.projectsClient.create(command).subscribe(
    result => console.log('Created:', result)
  );
}
```

---

## Backend Developer Setup

### Prerequisites
- .NET 9.0 SDK
- PostgreSQL 16+ (or use SQLite for dev)
- Code editor (VS Code or Rider recommended)
- AI API key (Claude or OpenAI)

### One-Time Setup

```bash
# Clone repository
git clone <repository-url>
cd ignaCheckApi

# Restore dependencies
dotnet restore

# Configure AI provider
cd src/Web
dotnet user-secrets set "AI:Claude:ApiKey" "your-api-key-here"

# Optional: Configure PostgreSQL
dotnet user-secrets set "ConnectionStrings:IgnaCheckDb" "Server=localhost;Port=5432;Database=IgnaCheckDb;Username=admin;Password=yourpassword;"
```

### Daily Development Workflow

```bash
# Navigate to API project
cd src/Web

# Run API
dotnet run

# API available at:
# https://localhost:5001/api
# Swagger UI: https://localhost:5001/swagger
```

### Hot Reload

```bash
# Enable hot reload (auto-restart on code changes)
dotnet watch run
```

### Available Commands

```bash
dotnet run                    # Start API
dotnet watch run              # Start with hot reload
dotnet build                  # Build solution
dotnet test                   # Run tests (when added)

# Entity Framework migrations
dotnet ef migrations add <Name> --project ../Infrastructure --startup-project .
dotnet ef database update --project ../Infrastructure --startup-project .
```

### Key Directories

```
src/
├── Domain/              # Entities, enums, interfaces
├── Application/         # Use cases (CQRS commands/queries)
├── Infrastructure/      # Data access, AI services, email
└── Web/                 # API controllers, Program.cs
    ├── Controllers/     # 14 API controllers
    └── ClientApp/       # Angular app (frontend dev works here)
```

### Testing the API

**Option 1: Swagger UI** (Recommended)
```
https://localhost:5001/swagger
```

**Option 2: curl**
```bash
# Register user
curl -X POST https://localhost:5001/api/authentication/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"SecurePass123!","displayName":"Test User"}'
```

**Option 3: REST Client (VS Code extension)**
Create `.http` files with requests.

---

## Working Together

### Development Workflow

**Morning:**
1. Backend dev: `git pull && dotnet run`
2. Frontend dev: `git pull && cd ClientApp && npm start`
3. Both work independently
4. Frontend dev hits backend dev's local API (or shared dev API)

**During Day:**
- Backend dev: Makes API changes, tests with Swagger
- Frontend dev: Builds UI, sees changes immediately via proxy
- Communication: Agree on API contracts before implementing

**End of Day:**
1. Backend dev: Commits API changes, pushes to feature branch
2. Frontend dev: Commits UI changes, pushes to feature branch
3. Create PR when feature is complete

### API Contract Changes

When changing API contracts:

1. **Backend dev:**
   - Update command/query/DTO
   - Build project (generates new TypeScript client)
   - Commit the generated `web-api-client.ts` file
   - Push changes

2. **Frontend dev:**
   - Pull changes
   - Get updated TypeScript client automatically
   - Update Angular components to use new contract

### Deployment Process

When ready to deploy:

```bash
# Build and publish (combines everything)
dotnet publish -c Release -o ./publish

# Output includes:
# - API DLLs
# - Angular compiled files in wwwroot/
# - Single deployment artifact

# Deploy to Azure
az webapp deploy --resource-group ignacheck-rg --name ignacheck-api --src-path ./publish
```

---

## Common Scenarios

### Scenario 1: Frontend dev needs new API endpoint

1. **Frontend dev** creates GitHub issue: "Need API endpoint for task assignments"
2. **Backend dev** implements endpoint, commits, pushes
3. **Frontend dev** pulls changes, gets new TypeScript client, implements UI

### Scenario 2: Backend dev changes API response

1. **Backend dev** changes DTO, builds (updates TypeScript client)
2. **Backend dev** commits both API + TypeScript client changes
3. **Frontend dev** pulls, gets TypeScript errors where contract changed
4. **Frontend dev** updates components to match new contract

### Scenario 3: Working offline

**Frontend dev:**
```javascript
// Switch to mock API in proxy.conf.js
target: 'http://localhost:3000'  // json-server
```

**Backend dev:**
Just work normally - no frontend dependency

### Scenario 4: Both working on same feature

1. Create feature branch: `feature/task-assignments`
2. Backend dev: Implements API endpoint
3. Backend dev: Commits, pushes to branch
4. Frontend dev: Pulls branch, implements UI
5. Frontend dev: Commits, pushes to same branch
6. Create PR for entire feature

---

## Troubleshooting

### Frontend: "API calls returning 404"

**Check:**
1. Is backend API running? → `curl https://localhost:5001/health`
2. Is proxy configured correctly? → Check `proxy.conf.js`
3. Is the API endpoint correct? → Check Swagger at `https://localhost:5001/swagger`

### Frontend: "CORS errors"

This shouldn't happen with the proxy. If it does:
1. Make sure you're using relative URLs (`/api/projects` not `https://localhost:5001/api/projects`)
2. Check `proxy.conf.js` includes the path context

### Backend: "Angular not building on publish"

**Check:**
1. Node.js installed? → `node --version`
2. npm dependencies installed? → `cd ClientApp && npm install`
3. Check build logs for errors

### Both: "Certificate errors"

**Run:**
```bash
dotnet dev-certs https --trust
```

---

## CI/CD Pipeline

The project is configured for Azure DevOps:

```yaml
# .azdo/build.yml (simplified)
- task: DotNetCoreCLI@2
  inputs:
    command: 'publish'
    publishWebProjects: true
  # This automatically:
  # 1. Runs npm install in ClientApp/
  # 2. Runs ng build --prod
  # 3. Copies dist/ to wwwroot/
  # 4. Publishes API + wwwroot/ together
```

---

## Best Practices

### Frontend Developer

✅ **Do:**
- Use relative URLs for API calls (`/api/projects`)
- Use the auto-generated TypeScript client
- Keep components small and focused
- Write unit tests for complex logic
- Use Angular style guide conventions

❌ **Don't:**
- Hardcode API URLs
- Manually write API client code
- Modify backend code (ask backend dev)
- Commit `node_modules/`

### Backend Developer

✅ **Do:**
- Follow Clean Architecture principles
- Write FluentValidation validators for all commands
- Use CQRS pattern (MediatR)
- Return appropriate HTTP status codes
- Add XML comments for API documentation (generates Swagger)
- Commit the generated `web-api-client.ts` file

❌ **Don't:**
- Skip validation
- Return raw entities (use DTOs)
- Modify Angular code (ask frontend dev)
- Break existing API contracts without versioning

---

## Production Architecture

Despite working separately in development, **production is a single deployment**:

```
Azure App Service
├── API Process (Kestrel)
│   ├── Handles /api/* requests → API controllers
│   └── Handles /* requests → Serves wwwroot/index.html
└── wwwroot/
    ├── index.html
    ├── main.js (Angular app)
    └── assets/
```

**Benefits:**
- ✅ Single deployment artifact
- ✅ No CORS configuration needed
- ✅ Shared authentication/cookies
- ✅ Single SSL certificate
- ✅ Lower hosting costs
- ✅ Simpler operations

**Scaling:**
- Scale vertically: Increase App Service plan size
- Scale horizontally: Increase instance count
- Static files served from same process (fast in-memory)
- Can add Azure CDN later if needed

---

## Questions?

- **API Documentation:** https://localhost:5001/swagger
- **Architecture:** See `README.md`
- **Issues:** Create GitHub issue with `frontend` or `backend` label

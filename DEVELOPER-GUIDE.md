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

### Choose Your Backend Option

You have **3 options** for connecting to the API:

#### Option A: Run Full Stack Locally (Recommended for Small Teams)
**Best for:** Full independence, offline work, debugging
**Requirements:** .NET SDK, PostgreSQL/SQLite, AI API key

```bash
# One-time setup
1. Install .NET 9 SDK from https://dotnet.microsoft.com/download
2. Install PostgreSQL 16+ (or use SQLite in dev)
3. Clone repo and configure:
   cd ignaCheckApi/src/Web
   dotnet user-secrets set "AI:Claude:ApiKey" "your-key-here"

# Daily workflow
cd src/Web
dotnet run  # Start API in background

# Open new terminal
cd src/Web/ClientApp
npm install
npm start   # Start Angular dev server
```

#### Option B: Connect to Shared Dev API (No .NET Installation)
**Best for:** Larger teams, no local backend needed
**Requirements:** Internet connection, shared dev API URL

```bash
# One-time setup
cd ignaCheckApi/src/Web/ClientApp

# Update proxy.conf.js to point to shared dev API
# (Backend dev will provide the URL)
# Example: https://ignacheck-dev.azurewebsites.net

npm install
npm start
```

#### Option C: Use Mock API (Fully Offline)
**Best for:** UI-focused work, no backend available
**Requirements:** None

```bash
# One-time setup
cd ignaCheckApi/src/Web/ClientApp
npm install -D json-server

# Create mock-api/db.json with sample data
# (See Mock API section below)

# Daily workflow
npm run mock-api  # Terminal 1
npm start         # Terminal 2
```

### One-Time Setup (After Choosing Option)

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

### Setting Up Mock API (Option C)

If you chose the mock API option for offline work:

**1. Install json-server:**
```bash
cd ClientApp
npm install -D json-server
```

**2. Create mock data file:**
```bash
mkdir mock-api
cat > mock-api/db.json << 'EOF'
{
  "projects": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "GDPR Compliance Audit",
      "description": "Annual GDPR compliance review",
      "status": "Active",
      "targetDate": "2025-12-31T00:00:00Z"
    },
    {
      "id": "7b8c9d10-2345-6789-abcd-ef1234567890",
      "name": "ISO 27001 Certification",
      "description": "Initial ISO 27001 certification project",
      "status": "InProgress",
      "targetDate": "2025-06-30T00:00:00Z"
    }
  ],
  "documents": [
    {
      "id": "a1b2c3d4-5678-90ab-cdef-1234567890ab",
      "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "fileName": "privacy-policy.pdf",
      "contentType": "application/pdf",
      "fileSizeBytes": 524288,
      "uploadedAt": "2025-11-01T10:30:00Z"
    }
  ],
  "frameworks": [
    {
      "id": "f1e2d3c4-b5a6-7890-1234-567890abcdef",
      "code": "GDPR",
      "name": "General Data Protection Regulation",
      "description": "EU data protection regulation",
      "version": "2016/679"
    },
    {
      "id": "a9b8c7d6-e5f4-3210-9876-543210fedcba",
      "code": "ISO27001",
      "name": "ISO/IEC 27001:2022",
      "description": "Information security management",
      "version": "2022"
    }
  ]
}
EOF
```

**3. Update proxy.conf.js:**
```javascript
// ClientApp/proxy.conf.js
const target = process.env.USE_MOCK_API
  ? 'http://localhost:3000'  // json-server
  : 'https://localhost:5001'; // real API

const PROXY_CONFIG = [
  {
    context: ["/api"],
    target: target,
    secure: false,
    pathRewrite: process.env.USE_MOCK_API ? { '^/api': '' } : {},
  }
];

module.exports = PROXY_CONFIG;
```

**4. Add npm scripts:**
```json
// ClientApp/package.json
{
  "scripts": {
    "start": "ng serve --port 44447",
    "start:mock": "USE_MOCK_API=true npm start",
    "mock-api": "json-server --watch mock-api/db.json --port 3000 --routes mock-api/routes.json"
  }
}
```

**5. Run with mock API:**
```bash
# Terminal 1: Start mock API
npm run mock-api

# Terminal 2: Start Angular with mock API
npm run start:mock
```

### Which Backend Option Should You Choose?

| Feature | Option A: Full Stack | Option B: Shared API | Option C: Mock API |
|---------|---------------------|---------------------|-------------------|
| **Offline work** | ✅ Yes | ❌ No | ✅ Yes |
| **Setup complexity** | 🟡 Medium | 🟢 Low | 🟢 Low |
| **.NET SDK required** | ✅ Yes | ❌ No | ❌ No |
| **Database required** | ✅ Yes | ❌ No | ❌ No |
| **Real API responses** | ✅ Yes | ✅ Yes | ❌ Mock data |
| **Debug backend** | ✅ Yes | ❌ No | ❌ No |
| **Integration testing** | ✅ Full | ✅ Full | ⚠️ Limited |
| **Team coordination** | 🟢 Low | 🟢 Low | 🟡 Medium |
| **Best for** | Small teams | Large teams | UI work |

**Recommendation for 2-person team:** Start with **Option A** (Full Stack) for first 2-4 weeks, then decide if you need shared API or mocking.

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

### Development Workflow (Option A: Both Run Full Stack)

**Morning:**
1. Backend dev: `git pull && cd src/Web && dotnet run`
2. Frontend dev: `git pull && cd src/Web && dotnet run` (start API)
3. Frontend dev: Open new terminal → `cd ClientApp && npm start`
4. Both work independently

**During Day:**
- Backend dev: Makes API changes, tests with Swagger
- Frontend dev: Builds UI, stops/restarts API when pulling backend changes
- Communication: Coordinate before pushing breaking API changes

**End of Day:**
1. Backend dev: Commits API changes + generated TypeScript client
2. Frontend dev: Pulls changes, may need to restart API
3. Frontend dev: Commits UI changes
4. Create PR when feature is complete

### Development Workflow (Option B: Shared Dev API)

**Morning:**
1. Backend dev: `git pull && cd src/Web && dotnet run`
2. Frontend dev: `git pull && cd ClientApp && npm start` (no backend needed!)
3. Backend dev deploys changes to dev API periodically

**During Day:**
- Backend dev: Works on API, deploys to dev API when ready
- Frontend dev: Builds UI against stable dev API
- Communication: Backend dev announces when new API version is deployed

**Coordination:**
- Backend dev pushes commits → CI/CD auto-deploys to dev API
- Frontend dev pulls to get new TypeScript client
- Less coordination needed, more independence

### Development Workflow (Option C: Mock API)

**Morning:**
1. Backend dev: `git pull && cd src/Web && dotnet run`
2. Frontend dev: `git pull && cd ClientApp && npm run mock-api && npm start`
3. Completely independent

**During Day:**
- Backend dev: Works on API independently
- Frontend dev: Works on UI with mock data
- Communication: Agree on API contracts, frontend dev updates mocks

**Integration:**
- Periodic integration testing with real API
- Frontend dev switches from mock to real API for testing
- Higher risk of integration issues

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

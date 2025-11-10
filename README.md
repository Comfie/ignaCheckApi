# IgnaCheck.ai - AI-Powered Compliance Audit Platform

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-9.0-512BD4)](https://docs.microsoft.com/en-us/ef/core/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

IgnaCheck.ai is an AI-powered audit and compliance assistant that automatically performs gap analyses against recognized regulatory frameworks (ISO 27001, SOC 2, PCI DSS, GDPR, DORA). It ingests internal documentation and tells you exactly what's missing, why it matters, and how to fix it — in plain English, with links to the source evidence.

**Think of it as a Copilot for Compliance** — helping audit professionals and compliance teams focus on what matters, faster.

## 📋 Table of Contents

- [Why IgnaCheck?](#why-ignacheck)
- [Key Features](#key-features)
- [Architecture](#architecture)
- [Domain Model](#domain-model)
- [Technology Stack](#technology-stack)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [API Documentation](#api-documentation)
- [Development Roadmap](#development-roadmap)
- [Project Structure](#project-structure)
- [Deployment](#deployment)
- [Contributing](#contributing)
- [License](#license)

## Why IgnaCheck?

The name combines "igna" (from "ignite") with "check," reflecting the platform's dual nature:
- **Rigorous verification** - Systematic, thorough analysis across regulatory frameworks
- **Catalytic action** - Actively ignites insights, sparks remediation paths, and catalyzes continuous regulatory readiness

### The Problem We Solve

- **Manual audit prep** takes 40-120+ hours per audit cycle
- **Fintechs and mid-size institutions** dedicate 10-15% of operational time to compliance documentation
- **Errors and oversight** result in repeat audits, financial penalties, and reputational risk
- **Compliance readiness** is static and episodic, not continuous and evolving

### Our Solution

We're building an AI-powered platform that:
- 📄 **Ingests documents** - Policies, logs, reports, technical evidence (PDF, DOCX, TXT, images)
- 🤖 **AI-powered gap detection** - LLM reads documents and compares against compliance frameworks
- 🎯 **Prioritizes findings** - Flags what's compliant, partially compliant, or missing by risk/severity
- 💡 **Remediation guidance** - Human-readable explanations with recommended next steps
- 👥 **Collaboration layer** - Assign gaps, add evidence, track resolution with threaded comments
- 📊 **Audit-ready output** - Export clean, consolidated reports (PDF, Excel)

## 🚀 Key Features

### ✅ Implemented Features

#### **Authentication & Security**
- Email-based registration with verification
- JWT-based authentication with session management
- Password reset with token expiration
- Account lockout protection
- Role-based access control (Owner, Admin, Contributor, Viewer)

#### **Multi-Tenant Workspace Management**
- Organization-based tenant isolation
- Workspace settings and branding
- Team member management with role assignment
- Email-based invitations (3-day expiration)
- User profile and notification preferences

#### **Project Management**
- Complete project CRUD operations
- Project archiving and restoration
- Project-level member management
- Status tracking (Draft, Active, InProgress, Completed, Archived)
- Target date and milestone management

#### **Document Management**
- Multi-file upload with 25MB limit per file
- Storage quota enforcement per organization
- Automatic text extraction (PDF, DOCX, TXT)
- File deduplication via hash comparison
- Document categorization and tagging
- Version support for document updates

#### **Compliance Frameworks (5 Pre-Loaded)**
- **DORA** - Digital Operational Resilience Act (14 controls)
- **ISO 27001:2022** - Information Security Management (20 controls)
- **SOC 2 Type II** - Trust Service Criteria (19 controls)
- **GDPR** - General Data Protection Regulation (19 articles)
- **PCI DSS 4.0** - Payment Card Industry Standards (12 requirements)

#### **AI-Powered Audit Analysis**
- Automated document analysis against framework controls
- AI provider abstraction (Claude 3.5 Sonnet, GPT-4o)
- Automatic fallback between providers
- Compliance gap identification with confidence scoring
- Evidence extraction and relevance scoring
- Remediation guidance generation
- Weighted compliance score calculation

#### **Findings Management**
- Comprehensive finding tracking with workflow status
- Severity classification (Critical, High, Medium, Low)
- Status workflow (Open, InProgress, Resolved, Accepted, FalsePositive)
- Finding assignment with due dates
- Evidence linking with document excerpts
- Threaded comments with @mentions
- Activity tracking and audit trail

#### **Reports & Analytics**
- Compliance dashboard with overall scores
- Framework-specific detailed reports
- Executive summary with auto-generated insights
- Findings distribution by severity and status
- Compliance trend analysis over time
- Top priority gap identification
- Audit trail export (CSV)

#### **Notifications**
- In-app notification system
- Email notifications with HTML templates
- Granular user preferences per notification type
- Delivery method selection (InApp, Email, Both)
- Email frequency settings (Realtime, Daily, Weekly, Never)

#### **Search & Discovery**
- Global search across projects, documents, findings, tasks
- Result grouping by entity type
- Project-scoped search
- Multi-tenancy aware search results

#### **Administration**
- Complete audit log with filtering
- Workspace deletion with cascade cleanup
- CSV export for audit logs
- Activity tracking (30+ activity types)

## 🏗️ Architecture

This project follows **Clean Architecture** principles with strict dependency rules ensuring maintainability, testability, and separation of concerns.

```
┌─────────────────────────────────────────────┐
│              Web Layer (API)                │
│    Controllers, Authentication, Swagger     │
│         JWT, Authorization Policies         │
└────────────────┬────────────────────────────┘
                 │ Depends on
                 ▼
┌─────────────────────────────────────────────┐
│         Infrastructure Layer                │
│  EF Core, Identity, File Storage, AI,       │
│  Email, Document Parsing, Repositories      │
└────────────────┬────────────────────────────┘
                 │ Implements
                 ▼
┌─────────────────────────────────────────────┐
│         Application Layer                   │
│  CQRS (MediatR), Commands, Queries,         │
│  Validation, Behaviors, DTOs, Interfaces    │
└────────────────┬────────────────────────────┘
                 │ Uses
                 ▼
┌─────────────────────────────────────────────┐
│           Domain Layer                      │
│  Entities, Value Objects, Enums,            │
│  Domain Events, Business Rules              │
└─────────────────────────────────────────────┘
```

### Architectural Patterns

- **CQRS** - Command Query Responsibility Segregation via MediatR
- **Repository Pattern** - IApplicationDbContext abstraction
- **Unit of Work** - Entity Framework Core DbContext
- **Dependency Injection** - ASP.NET Core built-in DI container
- **Multi-Tenancy** - Organization-based isolation with ITenantEntity
- **Domain Events** - Event-driven architecture for cross-aggregate communication
- **Validation Pipeline** - FluentValidation with MediatR behaviors
- **Mapping** - AutoMapper for DTO transformations

## 📊 Domain Model

The system models compliance auditing through 18 core entities:

### Core Business Entities

#### **Organization (Workspace/Tenant)**
Multi-tenant root aggregate representing a company or team.
- Properties: Name, Slug, Domain, Industry, CompanySize
- Subscription: Trial periods, storage quotas, member limits
- Navigation: Members, Invitations, Projects

#### **Project (Compliance Engagement)**
Main aggregate for organizing compliance work.
- Properties: Name, Description, Status, TargetDate
- Lifecycle: Draft → Active → InProgress → Completed → Archived
- Contains: Documents, Findings, Tasks, ProjectMembers, ProjectFrameworks

#### **ComplianceFramework**
Regulatory frameworks with hierarchical controls.
- Properties: Code, Name, Version, Category, IssuingAuthority
- System-wide or tenant-customizable
- Contains: 100+ pre-seeded ComplianceControls

#### **ComplianceControl**
Individual requirements within a framework.
- Properties: ControlCode, Title, Description, ImplementationGuidance
- Hierarchical: Parent/sub-control relationships
- Example: "A.5.1 Policies for information security" (ISO 27001)

#### **Document**
Uploaded evidence files with AI-ready metadata.
- Properties: FileName, ContentType, FileSizeBytes, StoragePath, FileHash
- Features: Text extraction, versioning, embeddings for semantic search
- Supports: PDF, DOCX, TXT, images

#### **ComplianceFinding**
AI-detected compliance gaps with workflow tracking.
- Status: NotAssessed, Compliant, PartiallyCompliant, NonCompliant
- Risk Level: Critical, High, Medium, Low
- Workflow: Open, InProgress, Resolved, Accepted, FalsePositive
- AI Metadata: ConfidenceScore, RemediationGuidance, EstimatedEffort
- Tracking: AssignedTo, DueDate, ReviewedBy, ResolvedDate

#### **FindingEvidence**
Links findings to supporting documents with excerpts.
- Properties: DocumentId, Excerpt, PageNumber, RelevanceScore
- EvidenceType: Supporting, Contradicting, Contextual

#### **RemediationTask**
Action items to address compliance gaps.
- Properties: Title, Description, Status, Priority, AssignedTo
- Tracking: EstimatedHours, ActualHours, PercentComplete

### Supporting Entities

- **OrganizationMember** - User membership with roles
- **ProjectMember** - Project-level access control
- **Invitation** - Pending workspace invitations (3-day expiration)
- **ActivityLog** - Complete audit trail of all actions
- **Notification** - In-app notifications
- **NotificationPreference** - User notification settings
- **FindingComment** - Discussion threads with @mentions
- **TaskComment** - Collaboration on remediation tasks
- **TaskAttachment** - Evidence files attached to tasks
- **ProjectFramework** - Many-to-many join table

## 🛠️ Technology Stack

### Backend Framework
- **.NET 9.0** - Latest LTS framework
- **ASP.NET Core 9** - Web API
- **C# 13** - Programming language

### Data & Persistence
- **Entity Framework Core 9.0** - ORM with migrations
- **PostgreSQL** - Primary production database (via Npgsql.EntityFrameworkCore.PostgreSQL)
- **SQL Server** - Alternative database support
- **SQLite** - Development and testing

### Architecture & Patterns
- **MediatR 12.4** - CQRS mediator pattern
- **AutoMapper 13.0** - Object-to-object mapping
- **FluentValidation 11.11** - Input validation
- **Ardalis.GuardClauses** - Defensive programming
- **Ardalis.Specification** - Repository query specification

### AI/ML Integration
- **Anthropic.SDK** - Claude AI integration (Claude 3.5 Sonnet)
- **Azure.AI.OpenAI** - OpenAI/GPT integration (GPT-4o)
- **Planned:** pgvector extension for semantic search

### Authentication & Security
- **ASP.NET Core Identity 9.0** - User management
- **Microsoft.AspNetCore.Authentication.JwtBearer** - JWT authentication
- **System.IdentityModel.Tokens.Jwt** - Token generation and validation

### Document Processing
- **PdfPig 0.1.9** - PDF text extraction
- **DocumentFormat.OpenXml 3.2** - DOCX/Office parsing

### API Documentation
- **NSwag.AspNetCore 14.2** - OpenAPI/Swagger generation
- **Microsoft.AspNetCore.OpenApi** - OpenAPI 3.0 support

### Testing
- **NUnit 4.2** - Test framework
- **Shouldly 4.2** - Fluent assertions
- **Moq 4.20** - Mocking framework
- **Respawn 6.2** - Database cleanup between tests
- **Testcontainers 4.1** - Integration testing with Docker

### Frontend (Planned)
- **Angular** - SPA framework (SpaRoot configured in Web project)
- TypeScript client auto-generation via NSwag

### Infrastructure (Future)
- Azure Blob Storage / AWS S3 - Cloud document storage
- Redis - Distributed caching
- Hangfire - Background job processing
- SignalR - Real-time WebSocket updates

## 🎯 Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (latest version)
- [PostgreSQL 16+](https://www.postgresql.org/download/) (recommended) or SQLite for development
- [Docker](https://www.docker.com/) (optional, for containerized dependencies)
- **AI Provider API Key** - Choose one:
  - [Anthropic Claude API Key](https://console.anthropic.com/) (recommended)
  - [OpenAI API Key](https://platform.openai.com/api-keys)

### Quick Start

1. **Clone the repository:**
   ```bash
   git clone https://github.com/yourusername/ignaCheckApi.git
   cd ignaCheckApi
   ```

2. **Install dependencies:**
   ```bash
   dotnet restore
   ```

3. **Configure AI provider (Required):**

   **Option A: User Secrets (Recommended for development)**
   ```bash
   cd src/Web

   # For Claude (Recommended)
   dotnet user-secrets set "AI:Claude:ApiKey" "your-anthropic-api-key"

   # OR for OpenAI
   dotnet user-secrets set "AI:OpenAI:ApiKey" "your-openai-api-key"
   ```

   **Option B: Environment Variables**
   ```bash
   export AI__Claude__ApiKey='your-anthropic-api-key'
   # OR
   export AI__OpenAI__ApiKey='your-openai-api-key'
   ```

   **Option C: appsettings.Development.json** (Not recommended - risk of committing secrets)
   ```json
   {
     "AI": {
       "Provider": "Claude",
       "Claude": {
         "ApiKey": "your-anthropic-api-key-here"
       }
     }
   }
   ```

4. **Configure database (Optional - defaults to in-memory for dev):**

   The application uses SQLite by default in development. To use PostgreSQL:

   **Start PostgreSQL with Docker:**
   ```bash
   docker run --name ignacheck-postgres -e POSTGRES_PASSWORD=password -e POSTGRES_USER=admin -e POSTGRES_DB=IgnaCheckDb -p 5432:5432 -d postgres:16
   ```

   **Or configure existing PostgreSQL:**
   ```bash
   cd src/Web
   dotnet user-secrets set "ConnectionStrings:IgnaCheckDb" "Server=localhost;Port=5432;Database=IgnaCheckDb;Username=admin;Password=yourpassword;"
   ```

5. **Run the application:**
   ```bash
   cd src/Web
   dotnet run
   ```

6. **Access the API:**
   - **Swagger UI:** https://localhost:5001/swagger
   - **API Base:** https://localhost:5001/api/
   - **Health Check:** https://localhost:5001/health

### First-Time Setup

1. **Register a user:**
   ```bash
   POST https://localhost:5001/api/authentication/register
   {
     "email": "admin@example.com",
     "password": "SecurePassword123!",
     "displayName": "Admin User"
   }
   ```

2. **Verify email** (check console logs for verification token in dev mode)

3. **Login and get JWT token:**
   ```bash
   POST https://localhost:5001/api/authentication/login
   {
     "email": "admin@example.com",
     "password": "SecurePassword123!"
   }
   ```

4. **Create a workspace:**
   ```bash
   POST https://localhost:5001/api/workspace
   Authorization: Bearer {your-jwt-token}
   {
     "name": "My Organization",
     "industry": "Technology"
   }
   ```

5. **Create a project and start compliance checking!**

### Database Management

#### Development Mode (Auto-Reset)
By default, the database is **automatically deleted, recreated, and seeded** on startup using `ApplicationDbContextInitialiser`. This keeps the schema synchronized with domain model changes.

- ✅ No migration management needed
- ✅ Clean state on every restart
- ✅ 5 compliance frameworks with 100+ controls pre-loaded
- ⚠️ All data is lost on restart

#### Production Mode (Migrations)
For production deployments, disable auto-reset and use EF Core migrations:

1. **Create migration:**
   ```bash
   dotnet ef migrations add InitialCreate --project src/Infrastructure --startup-project src/Web
   ```

2. **Update database:**
   ```bash
   dotnet ef database update --project src/Infrastructure --startup-project src/Web
   ```

3. **Seed frameworks:**
   ```bash
   # Run FrameworkSeeder manually or via startup configuration
   ```

## ⚙️ Configuration

### AI Configuration

The system supports two AI providers with automatic fallback.

#### Recommended: Anthropic Claude

**Why Claude?**
- Superior reasoning for compliance analysis
- Better context handling for long documents
- More accurate gap identification
- Excellent at generating actionable remediation guidance

**Configuration:**
```json
{
  "AI": {
    "Provider": "Claude",
    "EnableFallback": true,
    "MaxTokens": 4096,
    "Temperature": 0.3,
    "TimeoutSeconds": 120,

    "Claude": {
      "ApiKey": "sk-ant-...",
      "Model": "claude-3-5-sonnet-20241022"
    }
  }
}
```

#### Alternative: OpenAI

**Configuration:**
```json
{
  "AI": {
    "Provider": "OpenAI",
    "EnableFallback": true,

    "OpenAI": {
      "ApiKey": "sk-...",
      "Model": "gpt-4o",
      "UseAzure": false
    }
  }
}
```

#### Configuration Options Explained

| Setting | Values | Description |
|---------|--------|-------------|
| `Provider` | "Claude", "OpenAI" | Primary AI provider |
| `EnableFallback` | true/false | Auto-switch providers on failure |
| `MaxTokens` | 1000-8000 | Maximum response length |
| `Temperature` | 0.0-1.0 | 0.3 recommended for factual compliance analysis |
| `TimeoutSeconds` | 30-300 | API request timeout |
| `CacheExtractedText` | true/false | Store document text in DB (dev: true, prod: false) |

#### Cost Estimates (per 1M tokens)

| Provider | Model | Input Cost | Output Cost |
|----------|-------|------------|-------------|
| Claude | Sonnet 3.5 | $3.00 | $15.00 |
| Claude | Opus 3 | $15.00 | $75.00 |
| Claude | Haiku 3 | $0.25 | $1.25 |
| OpenAI | GPT-4o | $2.50 | $10.00 |
| OpenAI | GPT-4 Turbo | $10.00 | $30.00 |

**Estimated Cost per Audit:**
- Small project (5 documents, 1 framework): $0.50-$2.00
- Medium project (20 documents, 3 frameworks): $5.00-$15.00
- Large project (100 documents, 5 frameworks): $25.00-$75.00

### Database Configuration

#### PostgreSQL (Recommended for Production)
```json
{
  "ConnectionStrings": {
    "IgnaCheckDb": "Server=localhost;Port=5432;Database=IgnaCheckDb;Username=admin;Password=securepassword;Include Error Detail=true"
  }
}
```

#### SQL Server
```json
{
  "ConnectionStrings": {
    "IgnaCheckDb": "Server=(localdb)\\mssqllocaldb;Database=IgnaCheckDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

#### SQLite (Development Only)
```json
{
  "ConnectionStrings": {
    "IgnaCheckDb": "Data Source=ignacheck.db"
  }
}
```

### JWT Authentication Configuration

```json
{
  "Jwt": {
    "SecretKey": "your-256-bit-secret-key-here-minimum-32-characters",
    "Issuer": "IgnaCheck.Api",
    "Audience": "IgnaCheck.Client",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

⚠️ **Security:** Generate a strong secret key (32+ characters) and store in User Secrets or Azure Key Vault.

### Email Configuration (SMTP)

```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "UseSsl": true,
    "Username": "noreply@ignacheck.ai",
    "Password": "your-app-password",
    "FromEmail": "noreply@ignacheck.ai",
    "FromName": "IgnaCheck Platform"
  }
}
```

### Storage Configuration

```json
{
  "Storage": {
    "Provider": "Local",
    "LocalPath": "./uploads",
    "MaxFileSizeBytes": 26214400,
    "AllowedExtensions": [".pdf", ".docx", ".txt", ".png", ".jpg"]
  }
}
```

**Future support:** Azure Blob Storage, AWS S3

## 📚 API Documentation

### API Overview

The API follows RESTful conventions with 14 controllers and 60+ endpoints.

**Base URL:** `https://localhost:5001/api`

**Authentication:** JWT Bearer token in `Authorization` header

**Response Format:** JSON with standardized error handling

### Core Endpoints

#### Authentication (`/api/authentication`)
```
POST   /register                    # Register new user
POST   /verify-email                # Verify email with token
POST   /login                       # Authenticate and get JWT
POST   /logout                      # Invalidate session
POST   /request-password-reset      # Request password reset
POST   /reset-password              # Reset password with token
```

#### Workspaces (`/api/workspace`)
```
GET    /my-workspaces              # List user's workspaces
POST   /                           # Create workspace
GET    /settings                   # Get workspace settings
PUT    /settings                   # Update settings
POST   /switch/{organizationId}    # Switch active workspace
```

#### Projects (`/api/projects`)
```
GET    /                           # List projects (filters: status, search, archived)
POST   /                           # Create project
GET    /{id}                       # Get project details
PUT    /{id}                       # Update project
DELETE /{id}                       # Delete project
POST   /{id}/archive               # Archive/restore project
GET    /{id}/activity              # Get activity log

# Project Members
POST   /{id}/members               # Add member
DELETE /{projectId}/members/{userId} # Remove member
PUT    /{projectId}/members/{userId}/role # Update role
```

#### Documents (`/api/documents`)
```
GET    /project/{projectId}        # List documents
POST   /project/{projectId}        # Upload single document
POST   /project/{projectId}/bulk   # Upload multiple documents
GET    /{id}                       # Get document details
GET    /{id}/download              # Download document
DELETE /{id}                       # Delete document
```

#### Frameworks (`/api/frameworks`)
```
GET    /                           # List all frameworks
GET    /{id}                       # Get framework details (with controls)
POST   /projects/{projectId}/frameworks # Assign frameworks to project
DELETE /projects/{projectId}/frameworks/{frameworkId} # Remove framework
```

#### Audit Analysis (`/api/audit`)
```
POST   /projects/{projectId}/frameworks/{frameworkId}/run
       # Run AI-powered audit check
       # Analyzes all documents against framework controls
       # Returns: Findings, evidence links, compliance scores
```

#### Findings (`/api/findings`)
```
GET    /project/{projectId}        # List findings (filters: severity, status, framework)
GET    /{id}                       # Get finding details with evidence
PUT    /{id}/status                # Update finding status
PUT    /{id}/assign                # Assign to team member
POST   /{id}/comments              # Add comment (supports @mentions)
```

#### Reports (`/api/reports`)
```
GET    /projects/{projectId}/dashboard
       # Compliance dashboard (overall score, trends, top priorities)

GET    /projects/{projectId}/frameworks/{frameworkId}
       # Framework-specific report (control-by-control analysis)

GET    /projects/{projectId}/executive-summary
       # Executive summary (auto-generated insights)

GET    /projects/{projectId}/audit-trail
       # Complete audit trail (activity log)
```

#### Search (`/api/search`)
```
GET    /?searchTerm={term}&resultTypes={types}&projectId={id}
       # Global search across projects, documents, findings, tasks
```

#### Notifications (`/api/notifications`)
```
GET    /                           # Get notifications (filters: type, read status)
POST   /mark-as-read               # Mark notifications as read
GET    /preferences                # Get notification preferences
PUT    /preferences                # Update preferences
```

#### Administration (`/api/administration`)
```
GET    /audit-logs                 # Get audit logs (Owner/Admin only)
GET    /audit-logs/export          # Export audit logs to CSV
DELETE /workspace                  # Delete workspace (Owner only)
```

### Example: Running an Audit Check

```bash
# 1. Upload documents to project
POST /api/documents/project/{projectId}
Content-Type: multipart/form-data
Authorization: Bearer {token}

# 2. Assign frameworks to project
POST /api/projects/{projectId}/frameworks
{
  "frameworkIds": ["dora-id", "iso27001-id"]
}

# 3. Run AI audit check
POST /api/audit/projects/{projectId}/frameworks/{frameworkId}/run
Authorization: Bearer {token}

Response:
{
  "projectId": "...",
  "frameworkId": "...",
  "analysisDate": "2025-11-10T10:30:00Z",
  "complianceScore": 72.5,
  "findingsCreated": 15,
  "controlsAnalyzed": 20,
  "documentsAnalyzed": 12,
  "findings": [...]
}

# 4. View compliance dashboard
GET /api/reports/projects/{projectId}/dashboard
```

### Error Handling

All endpoints return standardized error responses:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": ["The Email field is required."],
    "Password": ["Password must be at least 12 characters."]
  }
}
```

**HTTP Status Codes:**
- `200 OK` - Success
- `201 Created` - Resource created
- `204 No Content` - Success with no response body
- `400 Bad Request` - Validation error
- `401 Unauthorized` - Missing or invalid JWT token
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `409 Conflict` - Resource conflict (e.g., duplicate email)
- `500 Internal Server Error` - Unexpected server error

### Interactive API Documentation

**Swagger UI:** https://localhost:5001/swagger

Features:
- Try out API endpoints directly from browser
- View request/response schemas
- Authentication support with JWT tokens
- Auto-generated from code and XML comments

## 🗺️ Development Roadmap

### ✅ Phase 1: Foundation & Core Features (COMPLETED)

**Focus 1: Foundation & Authentication** ✅
- User registration, email verification, login/logout
- Password reset with secure token workflow
- Workspace creation and settings management
- Role-based access control (Owner, Admin, Contributor, Viewer)
- User invitations with expiration

**Focus 2: Project & Document Management** ✅
- Complete project CRUD with member management
- Document upload/download/delete with versioning
- Document parsing (PDF, DOCX, TXT)
- Activity logging (30+ activity types)
- Storage quota management

**Focus 3: Compliance Framework & AI Analysis** ✅
- 5 frameworks with 100+ controls seeded
- AI analysis infrastructure (Claude 3.5, GPT-4o)
- RunAuditCheck command (core AI orchestration)
- Finding generation with evidence linking
- Compliance scoring and remediation guidance

**Focus 4: Email Notifications & Search** ✅
- Complete email notification system
- In-app notifications with user preferences
- Global search across all entities
- @mention support in comments

**Focus 5: Workspace Administration** ✅
- Workspace deletion with cascade cleanup
- Comprehensive audit logs with filtering
- CSV export for compliance tracking

### 🚧 Phase 2: Enterprise Features (Planned)

**Remediation Tasks**
- Task CRUD with assignment and due dates
- Task comments and attachments
- Time tracking (estimated vs actual hours)
- Task dependencies and blocking relationships

**Advanced Reporting**
- PDF report generation (executive summaries, framework reports)
- Excel export for detailed analysis
- Custom report templates
- Scheduled report delivery

**Integration & Automation**
- Slack/Teams notifications
- JIRA/Azure DevOps task sync
- SSO/SAML 2.0 authentication
- Webhooks for external integrations

**Performance & Scale**
- Redis caching layer
- Background job processing (Hangfire)
- Document processing queue
- Rate limiting and throttling

### 🔮 Phase 3: Advanced AI & Analytics (Future)

**AI Enhancements**
- Semantic search with pgvector embeddings
- Multi-document synthesis
- Automated control-to-document mapping suggestions
- Predictive compliance risk scoring
- Natural language query interface

**Analytics & Intelligence**
- Compliance trend analysis over time
- Benchmark against industry standards
- Predictive gap identification
- Risk heat maps and visualizations

**Collaboration**
- Real-time WebSocket updates (SignalR)
- Collaborative document annotation
- Version control and diff viewing
- Approval workflows

## 📁 Project Structure

```
ignaCheckApi/
├── IgnaCheck.sln                          # Solution file
├── README.md                              # This file
├── appsettings.AI.json                    # AI configuration guide
├── Directory.Packages.props               # Centralized package management
│
├── src/
│   ├── Domain/                            # Enterprise business rules
│   │   ├── Entities/                      # 18 core domain entities
│   │   │   ├── Organization.cs            # Multi-tenant root aggregate
│   │   │   ├── Project.cs                 # Compliance project
│   │   │   ├── ComplianceFramework.cs     # Regulatory frameworks
│   │   │   ├── ComplianceControl.cs       # Framework requirements
│   │   │   ├── Document.cs                # Evidence files
│   │   │   ├── ComplianceFinding.cs       # Compliance gaps
│   │   │   ├── FindingEvidence.cs         # Document links
│   │   │   ├── RemediationTask.cs         # Action items
│   │   │   ├── OrganizationMember.cs      # Workspace membership
│   │   │   ├── ProjectMember.cs           # Project access
│   │   │   ├── Invitation.cs              # Pending invites
│   │   │   ├── ActivityLog.cs             # Audit trail
│   │   │   ├── Notification.cs            # User notifications
│   │   │   ├── NotificationPreference.cs  # Notification settings
│   │   │   ├── FindingComment.cs          # Finding discussions
│   │   │   ├── TaskComment.cs             # Task discussions
│   │   │   ├── TaskAttachment.cs          # Task evidence
│   │   │   └── ProjectFramework.cs        # Project-framework mapping
│   │   ├── Common/                        # Base classes and abstractions
│   │   │   ├── BaseEntity.cs              # Base entity with Guid ID
│   │   │   ├── BaseAuditableEntity.cs     # Auditable base with timestamps
│   │   │   └── ITenantEntity.cs           # Multi-tenancy interface
│   │   ├── Enums/                         # Domain enumerations
│   │   │   ├── ComplianceStatus.cs        # Compliant, PartiallyCompliant, NonCompliant
│   │   │   ├── FindingStatus.cs           # Open, InProgress, Resolved, etc.
│   │   │   ├── RiskLevel.cs               # Critical, High, Medium, Low
│   │   │   ├── ProjectStatus.cs           # Draft, Active, Completed, etc.
│   │   │   └── UserRole.cs                # Owner, Admin, Contributor, Viewer
│   │   └── Events/                        # Domain events
│   │
│   ├── Application/                       # Application business rules
│   │   ├── Common/
│   │   │   ├── Interfaces/                # Service contracts
│   │   │   │   ├── IApplicationDbContext.cs       # Repository abstraction
│   │   │   │   ├── IAIAnalysisService.cs          # AI service contract
│   │   │   │   ├── IDocumentParsingService.cs     # Document parsing
│   │   │   │   ├── IFileStorageService.cs         # File storage
│   │   │   │   ├── IEmailService.cs               # Email service
│   │   │   │   ├── INotificationService.cs        # Notifications
│   │   │   │   ├── IIdentityService.cs            # User management
│   │   │   │   ├── ITenantService.cs              # Multi-tenancy
│   │   │   │   └── IJwtTokenGenerator.cs          # JWT generation
│   │   │   ├── Behaviours/                # MediatR pipeline behaviors
│   │   │   │   ├── ValidationBehaviour.cs # FluentValidation integration
│   │   │   │   ├── AuthorizationBehaviour.cs # Permission checks
│   │   │   │   ├── LoggingBehaviour.cs    # Request/response logging
│   │   │   │   └── PerformanceBehaviour.cs # Performance monitoring
│   │   │   ├── Models/                    # Shared DTOs and models
│   │   │   └── Exceptions/                # Custom exceptions
│   │   │
│   │   ├── Authentication/                # Auth commands/queries
│   │   │   ├── Commands/
│   │   │   │   ├── Register/
│   │   │   │   ├── VerifyEmail/
│   │   │   │   ├── Login/
│   │   │   │   ├── Logout/
│   │   │   │   ├── RequestPasswordReset/
│   │   │   │   └── ResetPassword/
│   │   │
│   │   ├── Workspaces/                    # Workspace management
│   │   │   ├── Commands/CreateWorkspace/
│   │   │   ├── Commands/UpdateWorkspace/
│   │   │   ├── Commands/DeleteWorkspace/
│   │   │   └── Queries/GetMyWorkspaces/
│   │   │
│   │   ├── Projects/                      # Project management (59 use cases total)
│   │   ├── Documents/                     # Document operations
│   │   ├── Frameworks/                    # Framework selection
│   │   ├── Audit/                         # AI audit orchestration
│   │   │   └── Commands/RunAuditCheck/    # Core AI analysis command
│   │   ├── Findings/                      # Finding management
│   │   ├── Reports/                       # Report generation
│   │   ├── Notifications/                 # Notification management
│   │   ├── Search/                        # Global search
│   │   ├── Profile/                       # User profile
│   │   ├── Users/                         # User/member management
│   │   └── Administration/                # Admin operations
│   │
│   ├── Infrastructure/                    # External concerns
│   │   ├── Data/
│   │   │   ├── ApplicationDbContext.cs    # EF Core DbContext
│   │   │   ├── ApplicationDbContextInitialiser.cs # Auto-reset seeding
│   │   │   ├── FrameworkSeeder.cs         # Load 5 frameworks + 100+ controls
│   │   │   ├── Configurations/            # Entity configurations (Fluent API)
│   │   │   ├── Interceptors/              # EF Core interceptors
│   │   │   └── Migrations/                # Database migrations
│   │   ├── Identity/
│   │   │   ├── IdentityService.cs         # User CRUD
│   │   │   └── TenantService.cs           # Multi-tenancy management
│   │   ├── Services/
│   │   │   ├── AIAnalysisService.cs       # AI provider abstraction
│   │   │   ├── DocumentParsingService.cs  # PDF/DOCX parsing
│   │   │   ├── LocalFileStorageService.cs # Local file storage
│   │   │   ├── EmailService.cs            # SMTP email sender
│   │   │   ├── NotificationService.cs     # Notification orchestration
│   │   │   └── JwtTokenGenerator.cs       # JWT token generation
│   │   └── DependencyInjection.cs         # Service registration
│   │
│   └── Web/                               # API presentation layer
│       ├── Controllers/                   # 14 API controllers
│       │   ├── AuthenticationController.cs
│       │   ├── WorkspaceController.cs
│       │   ├── ProjectsController.cs
│       │   ├── DocumentsController.cs
│       │   ├── FrameworksController.cs
│       │   ├── AuditController.cs         # AI audit orchestration endpoint
│       │   ├── FindingsController.cs
│       │   ├── ReportsController.cs
│       │   ├── NotificationsController.cs
│       │   ├── SearchController.cs
│       │   ├── ProfileController.cs
│       │   ├── UsersController.cs
│       │   └── AdministrationController.cs
│       ├── Infrastructure/
│       │   ├── WebApplicationExtensions.cs
│       │   └── CustomExceptionHandler.cs
│       ├── Program.cs                     # Application entry point
│       ├── appsettings.json               # Configuration
│       ├── appsettings.Development.json   # Dev configuration
│       └── appsettings.PostgreSQL.json    # PostgreSQL configuration
│
└── tests/ (Removed in recent refactor - to be re-added)
```

## 🚀 Deployment

### Docker Deployment

#### Build Docker Image
```bash
docker build -t ignacheck-api:latest -f src/Web/Dockerfile .
```

#### Run with Docker Compose
```yaml
version: '3.8'

services:
  postgres:
    image: postgres:16
    environment:
      POSTGRES_DB: IgnaCheckDb
      POSTGRES_USER: admin
      POSTGRES_PASSWORD: securepassword
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

  api:
    image: ignacheck-api:latest
    ports:
      - "5001:5001"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__IgnaCheckDb=Server=postgres;Port=5432;Database=IgnaCheckDb;Username=admin;Password=securepassword;
      - AI__Claude__ApiKey=${CLAUDE_API_KEY}
      - Jwt__SecretKey=${JWT_SECRET}
    depends_on:
      - postgres

volumes:
  postgres_data:
```

Run: `docker-compose up -d`

### Azure Deployment

#### Azure App Service + PostgreSQL

1. **Create resources:**
   ```bash
   az group create --name ignacheck-rg --location eastus

   az postgres flexible-server create \
     --name ignacheck-db \
     --resource-group ignacheck-rg \
     --admin-user adminuser \
     --admin-password SecurePassword123!

   az webapp create \
     --name ignacheck-api \
     --resource-group ignacheck-rg \
     --plan ignacheck-plan \
     --runtime "DOTNET|9.0"
   ```

2. **Configure app settings:**
   ```bash
   az webapp config appsettings set \
     --name ignacheck-api \
     --resource-group ignacheck-rg \
     --settings \
       AI__Claude__ApiKey="@Microsoft.KeyVault(SecretUri=...)" \
       ConnectionStrings__IgnaCheckDb="Server=...;Database=IgnaCheckDb;..."
   ```

3. **Deploy:**
   ```bash
   dotnet publish src/Web/Web.csproj -c Release -o ./publish
   az webapp deploy --resource-group ignacheck-rg --name ignacheck-api --src-path ./publish
   ```

### AWS Deployment

#### ECS Fargate + RDS PostgreSQL

1. **Create RDS instance:**
   ```bash
   aws rds create-db-instance \
     --db-instance-identifier ignacheck-db \
     --db-instance-class db.t3.micro \
     --engine postgres \
     --master-username admin \
     --master-user-password SecurePassword123!
   ```

2. **Build and push Docker image:**
   ```bash
   aws ecr create-repository --repository-name ignacheck-api
   docker tag ignacheck-api:latest 123456789012.dkr.ecr.us-east-1.amazonaws.com/ignacheck-api:latest
   docker push 123456789012.dkr.ecr.us-east-1.amazonaws.com/ignacheck-api:latest
   ```

3. **Create ECS task definition and service** (use AWS Console or CDK)

### Environment Variables (Production)

```bash
# Database
ConnectionStrings__IgnaCheckDb="..."

# AI Provider
AI__Provider="Claude"
AI__Claude__ApiKey="sk-ant-..."
AI__EnableFallback=true

# JWT
Jwt__SecretKey="32-character-minimum-secret-key"
Jwt__Issuer="IgnaCheck.Api"
Jwt__Audience="IgnaCheck.Client"

# Email
Email__SmtpHost="smtp.sendgrid.net"
Email__SmtpPort=587
Email__Username="apikey"
Email__Password="SG.xxx"

# Storage
Storage__Provider="AzureBlob"  # or "AwsS3"
Storage__ConnectionString="..."
```

### Health Checks

Monitor application health:
- **Health endpoint:** `GET /health`
- **Readiness:** `GET /health/ready`
- **Liveness:** `GET /health/live`

### Performance Recommendations

- **Database:** Use connection pooling (default in EF Core)
- **AI Calls:** Implement request queuing for high-volume scenarios
- **File Storage:** Use CDN for document downloads
- **Caching:** Add Redis for frequently accessed data
- **Monitoring:** Application Insights, CloudWatch, or Datadog

## 🤝 Contributing

This project is currently in active development. Contributions are welcome once the core features are stabilized.

### Development Guidelines

1. **Architecture:** Follow Clean Architecture principles - respect dependency rules
2. **CQRS:** Use MediatR commands for writes, queries for reads
3. **Validation:** All commands must have FluentValidation validators
4. **Testing:** Write unit tests for domain logic, integration tests for use cases
5. **Code Style:** Follow Microsoft C# coding conventions
6. **Commits:** Use conventional commits (feat:, fix:, refactor:, docs:, etc.)

### Branching Strategy

- `main` - Production-ready code
- `develop` - Integration branch
- `feature/*` - Feature development
- `bugfix/*` - Bug fixes
- `release/*` - Release preparation

### Pull Request Process

1. Fork the repository
2. Create feature branch from `develop`
3. Write tests for new functionality
4. Ensure all tests pass
5. Update documentation
6. Submit PR with clear description

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

This project is built on [Jason Taylor's Clean Architecture template](https://github.com/jasontaylordev/CleanArchitecture). We're grateful for the solid foundation it provides.

Special thanks to:
- **Anthropic** for Claude AI - powering intelligent compliance analysis
- **Microsoft** for .NET and Entity Framework Core
- **PostgreSQL** community for the robust database platform

## 📞 Support

- **Documentation:** [GitHub Wiki](https://github.com/yourusername/ignaCheckApi/wiki)
- **Issues:** [GitHub Issues](https://github.com/yourusername/ignaCheckApi/issues)
- **Discussions:** [GitHub Discussions](https://github.com/yourusername/ignaCheckApi/discussions)
- **Email:** support@ignacheck.ai

---

**Built with ❤️ to make compliance auditing faster, smarter, and more accessible.**

*"We have to work twice as hard to get half of what they have. Let's build something exceptional."*

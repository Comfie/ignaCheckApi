# IgnaCheck.ai - AI-Powered Compliance Audit Platform

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

IgnaCheck.ai is an AI-powered audit and compliance assistant that automatically performs gap analyses against recognized regulatory frameworks (like ISO 27001, SOC 2, PCI DSS, GDPR, etc.). It ingests internal documentation and tells you exactly what's missing, why it matters, and how to fix it — in plain English, with links to the source evidence.

**Think of it as a Copilot for Compliance** — helping audit professionals and compliance teams focus on what matters, faster.

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
- 📄 **Ingests documents** - Policies, logs, reports, technical evidence (PDF, DOCX, CSV, etc.)
- 🤖 **AI-powered gap detection** - LLM reads documents and compares against compliance frameworks
- 🎯 **Prioritizes findings** - Flags what's compliant, partially compliant, or missing by risk/severity
- 💡 **Remediation guidance** - Human-readable explanations with recommended next steps
- 👥 **Collaboration layer** - Assign gaps, add evidence, track resolution
- 📊 **Audit-ready output** - Export clean, consolidated reports (PDF, Excel)

## Architecture

This project follows **Clean Architecture** principles with clear separation of concerns:

```
IgnaCheck/
├── src/
│   ├── Domain/              # Enterprise business rules (entities, value objects, domain events)
│   ├── Application/         # Application business rules (use cases, CQRS, behaviors)
│   ├── Infrastructure/      # External concerns (database, file storage, AI services)
│   ├── Web/                # API endpoints (minimal APIs, authentication)
│   ├── AppHost/            # .NET Aspire orchestration
│   └── ServiceDefaults/    # Shared service configuration
└── tests/
    ├── Domain.UnitTests/
    ├── Application.UnitTests/
    ├── Application.FunctionalTests/
    └── Infrastructure.IntegrationTests/
```

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (latest version)
- [PostgreSQL](https://www.postgresql.org/download/) (recommended) or SQLite for development
- [Docker](https://www.docker.com/) (optional, for containerized dependencies)

### Running the Application

1. **Clone the repository:**
   ```bash
   git clone https://github.com/ignacheck/ignaCheckApi.git
   cd ignaCheckApi
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Run the application:**
   ```bash
   cd src/Web
   dotnet run
   ```

4. **Access the API:**
   - Swagger UI: `https://localhost:5001/swagger`
   - API: `https://localhost:5001/api/`

### Database

The application currently supports:
- **PostgreSQL** (recommended for production)
- **SQLite** (for local development)
- **SQL Server**

During development, the database is automatically deleted, recreated, and seeded on startup using `ApplicationDbContextInitialiser`. This keeps the schema and sample data in sync with the domain model.

For production deployments, use EF Core migrations:
```bash
dotnet ef migrations add InitialCreate --project src/Infrastructure --startup-project src/Web
dotnet ef database update --project src/Infrastructure --startup-project src/Web
```

## MVP Development Roadmap

### Executive Summary
- **Product:** ignacheck - Multi-tenant AI-powered GRC (Governance, Risk, and Compliance) auditing platform
- **Target Users:** Enterprises requiring compliance management across DORA, ISO/IEC 27001:2022, SOC 2, GDPR, PCI-DSS
- **Core Value Proposition:** Automated compliance auditing with AI-powered document analysis, real-time gap identification, and collaborative remediation tracking

---

### ✅ Focus 1: Foundation & Authentication (COMPLETED)

**Status:** All features implemented and committed

#### Authentication
- ✅ **User Registration** - Email-based registration with email verification
  - Email/password registration with validation (min 12 chars)
  - Email verification via token (24-hour expiration)
  - Account activation workflow

- ✅ **Email Verification** - Confirm email ownership via token
  - Unique verification tokens
  - Automatic account activation upon verification

- ✅ **Login/Logout** - Secure authentication with JWT session management
  - JWT-based authentication
  - Session management with configurable expiration
  - Failed login attempt tracking and account lockout protection

- ✅ **Password Reset** - Self-service password recovery
  - Password reset via email token (1-hour expiration)
  - Password strength validation
  - Session invalidation after reset

#### Workspace Management
- ✅ **Workspace Creation** - First-time setup for organizations
  - Workspace name, company details, industry selection
  - Automatic workspace owner assignment
  - Default workspace settings

- ✅ **Workspace Settings** - Configure workspace details
  - Edit workspace metadata
  - Logo upload support
  - Timezone configuration

#### User Management
- ✅ **User Roles** - Role-based access control (RBAC)
  - Owner: Full access, billing, workspace deletion
  - Admin: User management, all projects
  - Contributor: Assigned projects only
  - Viewer: Read-only access

- ✅ **Invite Users** - Email-based team invitations
  - Role assignment during invitation
  - 3-day invitation expiration
  - Pending invitations tracking

- ✅ **User List** - View and manage workspace members
  - Searchable, filterable, sortable member list
  - Display: Name, Email, Role, Status, Last Login

- ✅ **Edit User Role** - Change user permissions
  - Role promotion/demotion with notifications
  - Protection against removing last owner

- ✅ **Remove User** - Revoke workspace access
  - Immediate access revocation
  - Automatic removal from all projects
  - Self-removal protection before ownership transfer

#### Profile Management
- ✅ **User Profile** - Personal account settings
  - Display name editing
  - Profile picture upload
  - Password change functionality
  - Email notification preferences

---

### ✅ Focus 2: Project & Document Management (COMPLETED)

**Status:** All core features implemented and committed

#### Projects
- ✅ **Create Project** - Initialize compliance projects with automatic member assignment
- ✅ **Project List** - View all accessible projects with filters (status, search, archived)
- ✅ **Project Details** - View project overview with member list and statistics
- ✅ **Update Project** - Update name, description, status, target date
- ✅ **Delete Project** - Permanently remove projects with confirmation and cascade deletion
- ✅ **Archive Project** - Archive/restore projects (owners only)
- ✅ **Project Members** - Complete member management
  - Add members with role assignment (Owner/Contributor/Viewer)
  - Remove members with last owner protection
  - Update member roles with validation

#### Documents
- ✅ **Upload Documents** - Single file upload with automatic parsing
  - 25MB file size limit per file
  - Storage quota enforcement
  - Automatic text extraction for supported formats
  - File hash computation for integrity
  - Document categorization and tagging

- ✅ **Document Parsing** - Extract text and structure
  - Text file parsing fully implemented
  - PDF and DOCX parsing interfaces ready (placeholders)
  - Structure detection support
  - Metadata extraction

- ✅ **Document List** - View all project documents with filtering
  - Filter by category
  - Search by name/description
  - Display size, version, upload date, page count

- ✅ **Download Document** - Retrieve original files with range support
- ✅ **Delete Document** - Remove documents with storage cleanup

#### Activity Log
- ✅ **Project Activity** - Complete audit trail of all actions
  - Filter by activity type (30+ activity types tracked)
  - Filter by user and date range
  - Maximum 500 records per query
  - Activity types include: project CRUD, member management, documents, findings

---

### ✅ Focus 3: Compliance Framework & AI Analysis (COMPLETED)

**Status:** All features implemented and committed.

#### Frameworks
- ✅ **Framework Library** - 5 major frameworks seeded with 100+ controls
  - DORA (Digital Operational Resilience Act) - 14 controls
  - ISO/IEC 27001:2022 - 20 key controls
  - SOC 2 Type II - 19 trust service criteria
  - GDPR - 19 key articles
  - PCI DSS 4.0 - 12 requirements
  - Control reference, title, description, implementation guidance

- ✅ **Framework Selection** - Assign/remove frameworks to/from projects
  - API: GET /api/frameworks, GET /api/frameworks/{id}
  - API: POST /api/projects/{projectId}/frameworks
  - API: DELETE /api/projects/{projectId}/frameworks/{frameworkId}

#### AI Analysis
- ✅ **AI Analysis Infrastructure** - LLM-ready service architecture
  - IAIAnalysisService with control and framework analysis methods
  - Comprehensive analysis models and DTOs
  - Evidence extraction and relevance scoring
  - Weighted compliance score calculation
  - Remediation guidance generation

- ✅ **Run Audit Check** - AI-powered document analysis orchestration
  - Automated analysis against all framework controls
  - Batch document processing with progress tracking
  - Automatic finding generation with evidence linking
  - Real-time compliance statistics
  - API: POST /api/audit/projects/{projectId}/frameworks/{frameworkId}/run

- ✅ **Document Analysis** - AI interprets and evaluates documents
  - Document content extraction and analysis
  - Control-to-document mapping
  - Confidence scoring (0.0-1.0)
  - Multi-document support per control

- ✅ **Gap Identification** - Detect compliance deficiencies
  - Severity classification (Critical, High, Medium, Low)
  - ComplianceStatus assessment (Compliant, PartiallyCompliant, NonCompliant)
  - AI-generated remediation recommendations
  - Estimated remediation effort

- ✅ **Document Mapping** - Link findings to source documents
  - FindingEvidence entity with excerpt extraction
  - Page/section referencing
  - Relevance scoring
  - Evidence type classification (Supporting, Contradicting, Contextual)

#### Findings
- ✅ **Findings List** - View all compliance gaps
  - Grouping, filtering, sorting by framework, severity, status, assigned user
  - Search by title, description, finding code
  - API: GET /api/findings/project/{projectId}

- ✅ **Finding Details** - Complete finding information
  - Control reference, severity, gap analysis, recommendations
  - Evidence documents with excerpts and relevance scores
  - Comment threads with mentions
  - API: GET /api/findings/{id}

- ✅ **Update Finding Status** - Track remediation (Open, In Progress, Resolved, Accepted, FalsePositive)
  - Resolution notes required for resolved findings
  - Automatic timestamp tracking
  - API: PUT /api/findings/{id}/status

- ✅ **Assign Finding** - Delegate responsibility to team members
  - Assign to project members with due dates
  - Owner-only permission enforcement
  - API: PUT /api/findings/{id}/assign

- ✅ **Finding Comments** - Collaborate on remediation
  - @mentions with user validation
  - Threaded discussions with parent/child relationships
  - Resolution marking support
  - API: POST /api/findings/{id}/comments

#### Reports
- ✅ **Compliance Dashboard** - High-level compliance overview
  - Overall compliance score with framework breakdown
  - Findings distribution by severity and workflow status
  - Compliance trend analysis over last 6 runs
  - Top priority findings (highest risk, unresolved)
  - API: GET /api/reports/projects/{projectId}/dashboard

- ✅ **Framework Report** - Detailed per-framework compliance
  - Per-control compliance status
  - Framework-specific compliance score
  - Findings grouped by control
  - API: GET /api/reports/projects/{projectId}/frameworks/{frameworkId}

- ✅ **Executive Summary** - Management-friendly overview
  - Auto-generated executive summary text
  - Key metrics and compliance status
  - Top risks with remediation guidance
  - AI-generated recommendations
  - Progress metrics and resolution rates
  - API: GET /api/reports/projects/{projectId}/executive-summary

- ✅ **Audit Trail Report** - Complete activity history
  - Leverages existing activity log infrastructure
  - Filter by activity type, user, date range
  - API: GET /api/reports/projects/{projectId}/audit-trail

- ✅ **Export Reports** - Download audit reports (PDF, Excel)
  - API endpoints defined with placeholder implementations
  - PDF export: /api/reports/projects/{projectId}/dashboard/export/pdf
  - Excel export: /api/reports/projects/{projectId}/frameworks/{frameworkId}/export/excel
  - Executive PDF: /api/reports/projects/{projectId}/executive-summary/export/pdf
  - Note: Full export implementation requires PDF/Excel libraries (to be added in future release)

---

### ✅ Focus 4: Email Notifications & Search (COMPLETED)

**Status:** Email-based notifications and global search implemented. Real-time collaboration features deferred to future phase.

#### Notifications
- ✅ **Email Notification Infrastructure** - Complete email notification system
  - Extended IEmailService with notification-specific methods
  - NotificationService for creating and sending notifications
  - User preference-based notification delivery
  - HTML email templates with branding
  - Notification types: FindingAssigned, TaskAssigned, MentionNotification, AuditCheckCompleted, WorkspaceInvitation

- ✅ **In-App Notifications** - Notification tracking and management
  - Notification entity for audit trail and history
  - Get notifications with filtering (type, read status, pagination)
  - Mark notifications as read (individual or bulk)
  - API: GET /api/notifications, POST /api/notifications/mark-as-read

- ✅ **Notification Preferences** - Granular user control
  - Per-notification type configuration
  - Delivery method selection (InApp, Email, Both)
  - Email frequency settings (Realtime, Daily, Weekly, Never)
  - Enable/disable toggles for each notification type
  - API: GET /api/notifications/preferences, PUT /api/notifications/preferences

#### Search
- ✅ **Global Search** - Comprehensive workspace search
  - Search across projects, documents, findings, and tasks
  - Result grouping by type with highlights
  - Filter by result types and project scope
  - Configurable max results per type
  - Respects project membership and multi-tenancy
  - API: GET /api/search?searchTerm={term}&resultTypes={types}&projectId={id}

#### Deferred Features (Future Phase)
- ⏭️ **Real-time Comments** - WebSocket updates (deferred - email-based workflow sufficient for Phase 1)
- ⏭️ **Slack/Teams Integration** - Third-party integrations (deferred to future release)
- ⏭️ **Advanced Filters** - Saved filter presets (basic filtering implemented, presets deferred)

---

### ✅ Focus 5: Workspace Administration (COMPLETED)

**Status:** Core administrative features implemented. Billing and SSO deferred to future phase.

#### Administration
- ✅ **Workspace Deletion** - Permanently delete workspace
  - Double confirmation workflow (name match + boolean flag)
  - Owner-only access control
  - Comprehensive cascade deletion:
    - All projects, documents, findings, and tasks
    - Activity logs and notifications
    - Finding comments and evidence
    - Task comments and attachments
    - Project frameworks and members
    - Organization members and invitations
  - Automatic file storage cleanup
  - API: DELETE /api/administration/workspace

- ✅ **Audit Logs** - Complete admin activity log
  - Owner and admin access only
  - Advanced filtering:
    - By activity type, user, entity type, entity ID
    - Date range filtering (start/end dates)
    - Full-text search on descriptions
  - Pagination support (up to 1000 records per query)
  - Detailed audit information:
    - Timestamp, activity type, description
    - User information (ID and name)
    - Entity details (type, ID, name)
    - Metadata for additional context
  - API: GET /api/administration/audit-logs

- ✅ **CSV Export** - Export audit logs to CSV
  - Same filtering capabilities as audit logs query
  - Up to 10,000 records per export
  - Timestamped filename for organization
  - Proper CSV escaping for data integrity
  - API: GET /api/administration/audit-logs/export

#### Deferred Features (Future Phase)
- ⏭️ **Data Export** - Full workspace data export (deferred to future release)
- ⏭️ **SSO Configuration** - SAML 2.0 integration (deferred to enterprise tier)
- ⏭️ **Billing & Subscriptions** - Payment processing (deferred to commercialization phase)

---

## Technology Stack

### Backend
- **ASP.NET Core 9** - Web framework
- **Entity Framework Core 9** - ORM
- **MediatR** - CQRS implementation
- **AutoMapper** - Object mapping
- **FluentValidation** - Input validation
- **PostgreSQL** - Primary database

### AI/ML (Planned for Focus 3)
- **Azure OpenAI** or **Anthropic Claude** - LLM for gap analysis
- **Semantic Kernel** - LLM orchestration
- **pgvector** - Vector search for embeddings

### Infrastructure (Planned)
- **Azure Blob Storage** / **AWS S3** - Document storage
- **Redis** - Caching layer
- **Hangfire** - Background job processing
- **SignalR** - Real-time updates

### Testing
- **NUnit** - Test framework
- **Shouldly** - Assertions
- **Moq** - Mocking
- **Respawn** - Database cleanup
- **Testcontainers** - Integration testing

## Contributing

This project is currently in Focus 1 completion. We welcome contributions once the foundation is stable.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

This project is built on [Jason Taylor's Clean Architecture template](https://github.com/jasontaylordev/CleanArchitecture). We're grateful for the solid foundation it provides.

---

**Remember:** We have to work twice as hard to get half of what they have. Let's build something exceptional.

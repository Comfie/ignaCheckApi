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

## Development Roadmap

### ✅ Phase 0: Foundation Setup (Current)
- Rename solution and projects to IgnaCheck
- Set up core domain entities (Organization, Project, ComplianceFramework)
- Implement multi-tenancy foundation
- Configure security and audit logging

### 🚧 Phase 1: Core Framework & Document Management (Weeks 3-6)
- Compliance framework management (ISO 27001, SOC 2, GDPR)
- Document upload and parsing (PDF, DOCX, OCR)
- Project and framework assignment
- Basic file storage integration

### 📅 Phase 2: AI Integration & Gap Analysis (Weeks 7-12)
- LLM integration (OpenAI/Claude)
- AI-powered gap detection
- Compliance findings with risk scoring
- Background job processing
- Remediation recommendations

### 📅 Phase 3: Remediation & Task Management (Weeks 13-16)
- Task creation and assignment
- Evidence attachment workflow
- Email/Slack notifications
- Task timeline and tracking

### 📅 Phase 4: Reporting & Analytics (Weeks 17-20)
- Professional audit reports (PDF/Excel)
- Interactive dashboards
- Compliance scorecards
- Trend analysis

### 📅 Phase 5: Advanced Features (Weeks 21-26)
- Cross-framework mapping
- Continuous monitoring
- External integrations (Google Drive, SharePoint, S3)
- Multi-language support
- SSO and advanced RBAC

### 📅 Phase 6: Production Readiness (Weeks 27-30)
- Performance optimization
- Security audit and penetration testing
- Monitoring and observability
- CI/CD pipelines
- Documentation

## Technology Stack

### Backend
- **ASP.NET Core 9** - Web framework
- **Entity Framework Core 9** - ORM
- **MediatR** - CQRS implementation
- **AutoMapper** - Object mapping
- **FluentValidation** - Input validation
- **PostgreSQL** - Primary database

### AI/ML (Planned)
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

This project is currently in Phase 0 (Foundation Setup). We welcome contributions once the foundation is stable.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

This project is built on [Jason Taylor's Clean Architecture template](https://github.com/jasontaylordev/CleanArchitecture). We're grateful for the solid foundation it provides.

---

**Remember:** We have to work twice as hard to get half of what they have. Let's build something exceptional.

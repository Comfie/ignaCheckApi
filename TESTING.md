# Testing Guide for IgnaCheck API

## 🎯 Quick Start

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/Domain.UnitTests/Domain.UnitTests.csproj
```

## 📁 Test Structure

```
tests/
├── Domain.UnitTests/          # Domain entity tests
│   └── Entities/
│       ├── ProjectTests.cs
│       ├── ComplianceFindingTests.cs
│       └── OrganizationTests.cs
│
├── Application.UnitTests/     # CQRS handler tests
│   ├── Common/
│   │   ├── TestBase.cs
│   │   └── MockDbSetHelper.cs
│   └── Projects/
│       └── Commands/
│           ├── CreateProjectCommandTests.cs
│           └── CreateProjectCommandValidatorTests.cs
│
├── Infrastructure.UnitTests/  # Service implementation tests
│   └── Services/
│       └── TenantServiceTests.cs
│
└── Web.IntegrationTests/      # API integration tests
    ├── Testing.cs             # Test infrastructure
    ├── BaseIntegrationTest.cs # Base class
    └── Controllers/
        └── ProjectsControllerTests.cs
```

## 🧪 Test Types

### 1. Domain Unit Tests

**Purpose**: Validate business logic and domain entities

**Example**:
```csharp
[Test]
public void Project_Should_Initialize_With_Default_Values()
{
    var project = new Project();

    project.Status.ShouldBe(ProjectStatus.Draft);
    project.ProjectFrameworks.ShouldBeEmpty();
}
```

**Run**: `dotnet test tests/Domain.UnitTests/`

### 2. Application Unit Tests

**Purpose**: Test command/query handlers with mocked dependencies

**Example**:
```csharp
[Test]
public async Task Handle_Should_CreateProject_When_ValidRequest()
{
    var command = new CreateProjectCommand { Name = "Test" };

    var result = await _handler.Handle(command, CancellationToken.None);

    result.Succeeded.ShouldBeTrue();
}
```

**Run**: `dotnet test tests/Application.UnitTests/`

### 3. Infrastructure Unit Tests

**Purpose**: Test infrastructure services (tenant, email, storage, etc.)

**Example**:
```csharp
[Test]
public void GetCurrentTenantId_Should_ReturnOrganizationId_When_ClaimExists()
{
    var result = _tenantService.GetCurrentTenantId();

    result.ShouldBe(organizationId);
}
```

**Run**: `dotnet test tests/Infrastructure.UnitTests/`

### 4. Integration Tests

**Purpose**: End-to-end API testing with real database

**Example**:
```csharp
[Test]
public async Task CreateProject_Should_ReturnSuccess_When_ValidRequest()
{
    var client = CreateClient();
    var response = await client.PostAsJsonAsync("/api/projects", command);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
}
```

**Run**: `dotnet test tests/Web.IntegrationTests/`

**Note**: Requires Docker for PostgreSQL via Testcontainers

## 🔧 Test Tools & Libraries

| Tool | Purpose | Documentation |
|------|---------|---------------|
| **NUnit** | Testing framework | [docs.nunit.org](https://docs.nunit.org/) |
| **Shouldly** | Readable assertions | [shouldly.io](https://shouldly.io/) |
| **Moq** | Mocking framework | [github.com/moq](https://github.com/moq/moq4) |
| **FluentValidation** | Validation testing | [docs.fluentvalidation.net](https://docs.fluentvalidation.net/) |
| **Testcontainers** | Docker-based testing | [dotnet.testcontainers.org](https://dotnet.testcontainers.org/) |
| **Respawn** | Database cleanup | [github.com/jbogard/Respawn](https://github.com/jbogard/Respawn) |

## 🚀 CI/CD Integration

### GitHub Actions

Automated testing runs on:
- ✅ Push to `main`, `develop`, or `claude/**` branches
- ✅ Pull requests
- ✅ Manual workflow dispatch

**Workflow**: `.github/workflows/dotnet-tests.yml`

**Features**:
- Runs all test projects
- Generates code coverage reports
- Cross-platform testing (Ubuntu, Windows, macOS)
- PostgreSQL integration for database tests
- Test result reporting

### Local CI Simulation

```bash
# Run tests like CI
dotnet test --collect:"XPlat Code Coverage" --logger "trx"

# Check code formatting
dotnet format --verify-no-changes

# Build with warnings as errors
dotnet build -warnaserror
```

## 📊 Code Coverage

### Generate Coverage Report

```bash
# Install report generator (once)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate HTML report
reportgenerator \
  -reports:"**/coverage.cobertura.xml" \
  -targetdir:"coveragereport" \
  -reporttypes:Html

# Open report
open coveragereport/index.html
```

### Coverage Targets

| Layer | Target | Current |
|-------|--------|---------|
| Domain | 90%+ | TBD |
| Application | 85%+ | TBD |
| Infrastructure | 80%+ | TBD |
| Overall | 80%+ | TBD |

## ✅ Best Practices

### 1. Test Naming

```csharp
// ❌ Bad
[Test]
public void Test1() { }

// ✅ Good
[Test]
public void CreateProject_Should_ReturnSuccess_When_ValidRequest() { }
```

Pattern: `MethodName_Should_ExpectedBehavior_When_Condition`

### 2. Arrange-Act-Assert Pattern

```csharp
[Test]
public void Example_Test()
{
    // Arrange - Set up test data and dependencies
    var entity = new Entity();

    // Act - Execute the method under test
    var result = entity.DoSomething();

    // Assert - Verify the expected outcome
    result.ShouldBe(expectedValue);
}
```

### 3. Test Independence

```csharp
// ✅ Each test is independent
[Test]
public void Test_One()
{
    await Testing.ResetState(); // Clean slate
    // ... test logic
}

[Test]
public void Test_Two()
{
    await Testing.ResetState(); // Clean slate
    // ... test logic
}
```

### 4. Descriptive Assertions

```csharp
// ❌ Less readable
Assert.True(result.Succeeded);
Assert.Equal("Test", result.Data.Name);

// ✅ More readable with Shouldly
result.Succeeded.ShouldBeTrue();
result.Data.Name.ShouldBe("Test");
```

### 5. Use TestCase for Multiple Scenarios

```csharp
[Test]
[TestCase(ProjectStatus.Active)]
[TestCase(ProjectStatus.Draft)]
[TestCase(ProjectStatus.Completed)]
public void Should_Support_All_Status_Values(ProjectStatus status)
{
    var project = new Project { Status = status };

    project.Status.ShouldBe(status);
}
```

## 🐛 Debugging Tests

### Visual Studio
1. Set breakpoint in test
2. Right-click test → **Debug Test**

### VS Code
1. Install C# Dev Kit extension
2. Set breakpoint
3. Click **Debug Test** in Test Explorer

### Command Line
```bash
# Run with detailed logging
dotnet test --logger "console;verbosity=detailed"

# Run specific test
dotnet test --filter "FullyQualifiedName=IgnaCheck.Domain.UnitTests.Entities.ProjectTests.Project_Should_Initialize_With_Default_Values"
```

## 🔍 Common Issues

### Issue: Integration tests fail with database error

**Solution**: Ensure Docker is running. Testcontainers needs Docker to start PostgreSQL.

```bash
# Check Docker status
docker ps

# Start Docker (if not running)
# Linux: sudo systemctl start docker
# macOS/Windows: Start Docker Desktop
```

### Issue: "Package restore failed"

**Solution**: Clear NuGet cache
```bash
dotnet nuget locals all --clear
dotnet restore
```

### Issue: Tests pass locally but fail in CI

**Solution**: Check for:
- Timing dependencies (use `Task.Delay` or `Polly` for retries)
- File system paths (use `Path.Combine` for cross-platform)
- Database state (ensure proper cleanup between tests)

## 📚 Writing New Tests

### Domain Entity Test Template

```csharp
using IgnaCheck.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace IgnaCheck.Domain.UnitTests.Entities;

[TestFixture]
public class MyEntityTests
{
    [Test]
    public void MyEntity_Should_Initialize_With_Default_Values()
    {
        var entity = new MyEntity();

        entity.Property.ShouldBe(expectedValue);
    }

    [Test]
    public void MyEntity_Should_SetProperties_Correctly()
    {
        var entity = new MyEntity
        {
            Property = "value"
        };

        entity.Property.ShouldBe("value");
    }
}
```

### Command Handler Test Template

```csharp
using IgnaCheck.Application.MyFeature.Commands;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace IgnaCheck.Application.UnitTests.MyFeature.Commands;

[TestFixture]
public class MyCommandHandlerTests
{
    private Mock<IApplicationDbContext> _mockDbContext;
    private MyCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _mockDbContext = new Mock<IApplicationDbContext>();
        _handler = new MyCommandHandler(_mockDbContext.Object);
    }

    [Test]
    public async Task Handle_Should_Succeed_When_ValidRequest()
    {
        var command = new MyCommand { /* ... */ };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
    }
}
```

### Integration Test Template

```csharp
using NUnit.Framework;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace IgnaCheck.Web.IntegrationTests.Controllers;

[TestFixture]
public class MyControllerTests : BaseIntegrationTest
{
    [Test]
    public async Task Endpoint_Should_ReturnSuccess()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/myendpoint");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
```

## 📖 Additional Resources

- [Full Test Documentation](tests/README.md)
- [NUnit Documentation](https://docs.nunit.org/)
- [Clean Architecture Testing](https://github.com/jasontaylordev/CleanArchitecture)
- [Integration Testing Best Practices](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)

## 🎯 Testing Checklist

Before submitting a PR:

- [ ] All tests pass locally
- [ ] New features have test coverage
- [ ] Integration tests included for API changes
- [ ] Validators have comprehensive test cases
- [ ] Code coverage meets targets (80%+)
- [ ] Tests follow naming conventions
- [ ] No test dependencies (tests run independently)
- [ ] CI/CD workflow passes

---

**Questions?** Check [tests/README.md](tests/README.md) for detailed documentation.

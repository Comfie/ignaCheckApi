# IgnaCheck API Test Suite

Comprehensive test suite for the IgnaCheck compliance management API, including unit tests, integration tests, and automated CI/CD testing.

## 📋 Table of Contents

- [Overview](#overview)
- [Test Projects](#test-projects)
- [Getting Started](#getting-started)
- [Running Tests](#running-tests)
- [Test Coverage](#test-coverage)
- [Writing Tests](#writing-tests)
- [CI/CD Integration](#cicd-integration)
- [Troubleshooting](#troubleshooting)

## 🎯 Overview

The test suite follows Clean Architecture principles and provides comprehensive coverage across all layers:

- **Domain.UnitTests**: Tests for domain entities, value objects, and business rules
- **Application.UnitTests**: Tests for CQRS command/query handlers and validators
- **Infrastructure.UnitTests**: Tests for external services and infrastructure concerns
- **Web.IntegrationTests**: End-to-end API integration tests with real database

### Test Statistics

| Test Project | Test Count | Coverage Target |
|-------------|------------|-----------------|
| Domain.UnitTests | ~50+ | 90%+ |
| Application.UnitTests | ~100+ | 85%+ |
| Infrastructure.UnitTests | ~30+ | 80%+ |
| Web.IntegrationTests | ~50+ | N/A |

## 🏗️ Test Projects

### 1. Domain.UnitTests

Tests domain entities and business logic without any infrastructure dependencies.

**Framework**: NUnit, Shouldly, Moq

**What's Tested**:
- Entity initialization and default values
- Property setters and getters
- Collection management
- Enum value validations
- Business rule enforcement

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

### 2. Application.UnitTests

Tests CQRS command and query handlers using mocked dependencies.

**Framework**: NUnit, Shouldly, Moq, MediatR

**What's Tested**:
- Command handler business logic
- Query handler data retrieval
- Validation rules
- Authorization checks
- Error handling

**Example**:
```csharp
[Test]
public async Task Handle_Should_CreateProject_When_ValidRequest()
{
    // Arrange
    var command = new CreateProjectCommand { Name = "Test" };

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Succeeded.ShouldBeTrue();
}
```

### 3. Infrastructure.UnitTests

Tests infrastructure services like tenant management, file storage, email, etc.

**Framework**: NUnit, Shouldly, Moq

**What's Tested**:
- Service implementations
- External integrations (mocked)
- Data access patterns
- Configuration handling

### 4. Web.IntegrationTests

End-to-end API tests with real HTTP requests and database.

**Framework**: NUnit, Shouldly, Microsoft.AspNetCore.Mvc.Testing, Testcontainers

**What's Tested**:
- Complete HTTP request/response cycle
- Authentication and authorization
- API endpoints
- Database transactions
- Error responses

**Example**:
```csharp
[Test]
public async Task CreateProject_Should_ReturnSuccess_When_ValidRequest()
{
    var client = CreateClient();
    var command = new CreateProjectCommand { Name = "SOC 2 Audit" };

    var response = await client.PostAsJsonAsync("/api/projects", command);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
}
```

## 🚀 Getting Started

### Prerequisites

- .NET 9.0 SDK or later
- Docker (for integration tests with PostgreSQL)
- Visual Studio 2022 / VS Code / Rider (recommended)

### Initial Setup

1. **Clone the repository**:
   ```bash
   git clone <repository-url>
   cd ignaCheckApi
   ```

2. **Restore NuGet packages**:
   ```bash
   dotnet restore IgnaCheck.sln
   ```

3. **Build the solution**:
   ```bash
   dotnet build IgnaCheck.sln
   ```

## 🧪 Running Tests

### Run All Tests

```bash
# Run all test projects
dotnet test IgnaCheck.sln

# Run with detailed output
dotnet test IgnaCheck.sln --verbosity detailed
```

### Run Specific Test Projects

```bash
# Domain tests only
dotnet test tests/Domain.UnitTests/Domain.UnitTests.csproj

# Application tests only
dotnet test tests/Application.UnitTests/Application.UnitTests.csproj

# Infrastructure tests only
dotnet test tests/Infrastructure.UnitTests/Infrastructure.UnitTests.csproj

# Integration tests only (requires Docker)
dotnet test tests/Web.IntegrationTests/Web.IntegrationTests.csproj
```

### Run Specific Tests

```bash
# Run tests by name filter
dotnet test --filter "FullyQualifiedName~ProjectTests"

# Run tests by category
dotnet test --filter "Category=Unit"

# Run a single test
dotnet test --filter "FullyQualifiedName=IgnaCheck.Domain.UnitTests.Entities.ProjectTests.Project_Should_Initialize_With_Default_Values"
```

### Run Tests in Parallel

```bash
# Run tests in parallel for faster execution
dotnet test --parallel
```

## 📊 Test Coverage

### Generate Coverage Reports

```bash
# Install coverage tool (once)
dotnet tool install -g dotnet-coverage

# Run tests with coverage
dotnet test IgnaCheck.sln --collect:"XPlat Code Coverage"

# Generate HTML report
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html

# Open report
open coveragereport/index.html  # macOS
xdg-open coveragereport/index.html  # Linux
start coveragereport/index.html  # Windows
```

### Coverage Goals

- **Domain Layer**: 90%+ (high business logic coverage)
- **Application Layer**: 85%+ (comprehensive handler testing)
- **Infrastructure Layer**: 80%+ (service implementation coverage)
- **Overall Project**: 80%+

## ✍️ Writing Tests

### Unit Test Template

```csharp
using NUnit.Framework;
using Shouldly;
using Moq;

namespace IgnaCheck.Domain.UnitTests.Entities;

[TestFixture]
public class MyEntityTests
{
    [SetUp]
    public void SetUp()
    {
        // Initialize test dependencies
    }

    [Test]
    public void MyMethod_Should_ReturnExpectedResult_When_ValidInput()
    {
        // Arrange
        var entity = new MyEntity();

        // Act
        var result = entity.MyMethod();

        // Assert
        result.ShouldBe(expectedValue);
    }

    [Test]
    [TestCase(1, "result1")]
    [TestCase(2, "result2")]
    public void MyMethod_Should_HandleMultipleCases(int input, string expected)
    {
        // Arrange & Act
        var result = MyEntity.Calculate(input);

        // Assert
        result.ShouldBe(expected);
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
    private const string ApiUrl = "/api/myendpoint";

    [Test]
    public async Task Endpoint_Should_ReturnSuccess_When_ValidRequest()
    {
        // Arrange
        var client = CreateClient();
        var request = new MyRequest { Value = "test" };

        // Act
        var response = await client.PostAsJsonAsync(ApiUrl, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Result<MyResponse>>();
        result.Succeeded.ShouldBeTrue();
    }
}
```

### Best Practices

1. **Naming Convention**: `MethodName_Should_ExpectedBehavior_When_Condition`
2. **Arrange-Act-Assert**: Structure all tests with clear AAA pattern
3. **One Assertion Per Test**: Focus on testing one thing at a time
4. **Independent Tests**: Tests should not depend on each other
5. **Mock External Dependencies**: Use Moq for mocking interfaces
6. **Descriptive Assertions**: Use Shouldly for readable assertions

## 🔄 CI/CD Integration

### GitHub Actions

The project includes a comprehensive GitHub Actions workflow that runs automatically on:

- **Push** to `main`, `develop`, or `claude/**` branches
- **Pull Requests** to `main` or `develop`
- **Manual trigger** via workflow_dispatch

#### Workflow Features

✅ **Automated Testing**: Runs all test projects in parallel
✅ **Code Coverage**: Generates and uploads coverage reports
✅ **Code Quality**: Validates formatting and build warnings
✅ **Cross-Platform**: Tests on Ubuntu, Windows, and macOS
✅ **PostgreSQL**: Spins up real database for integration tests
✅ **Test Reports**: Publishes detailed test results

#### Workflow File

Location: `.github/workflows/dotnet-tests.yml`

#### Viewing Test Results

1. Navigate to **Actions** tab in GitHub
2. Select the workflow run
3. View test results and coverage reports in artifacts

### Running Tests Locally (CI Mode)

Simulate CI environment locally:

```bash
# Run all tests with coverage
dotnet test IgnaCheck.sln --collect:"XPlat Code Coverage" --logger "trx"

# Format check (like CI)
dotnet format IgnaCheck.sln --verify-no-changes

# Build with warnings as errors (like CI)
dotnet build IgnaCheck.sln -warnaserror
```

## 🐛 Troubleshooting

### Common Issues

#### Issue: "Docker daemon not running" during integration tests

**Solution**: Start Docker Desktop or Docker service
```bash
# Linux
sudo systemctl start docker

# macOS/Windows
# Start Docker Desktop application
```

#### Issue: "Package restore failed"

**Solution**: Clear NuGet cache
```bash
dotnet nuget locals all --clear
dotnet restore IgnaCheck.sln
```

#### Issue: "Tests fail with database connection error"

**Solution**: Testcontainers will automatically start PostgreSQL. Ensure Docker is running.

#### Issue: "Coverage report not generating"

**Solution**: Install reportgenerator tool
```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

### Debug Tests

#### Visual Studio
1. Set breakpoint in test
2. Right-click test → **Debug Test**

#### VS Code
1. Install C# extension
2. Set breakpoint
3. Use Test Explorer to debug

#### Command Line
```bash
# Attach debugger
dotnet test --logger "console;verbosity=detailed"
```

## 📚 Additional Resources

- [NUnit Documentation](https://docs.nunit.org/)
- [Shouldly Assertions](https://github.com/shouldly/shouldly)
- [Moq Quick Start](https://github.com/moq/moq4)
- [ASP.NET Core Testing](https://learn.microsoft.com/en-us/aspnet/core/test/)
- [Testcontainers .NET](https://dotnet.testcontainers.org/)

## 🤝 Contributing

When adding new features:

1. ✅ Write tests FIRST (TDD approach recommended)
2. ✅ Maintain test coverage above 80%
3. ✅ Follow naming conventions
4. ✅ Update this README if adding new test patterns
5. ✅ Ensure all tests pass before submitting PR

## 📞 Support

For questions or issues:
- Create an issue in the repository
- Contact the development team
- Review existing tests for examples

---

**Happy Testing! 🎉**

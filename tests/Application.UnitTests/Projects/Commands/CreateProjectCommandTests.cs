using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Application.Common.Models;
using IgnaCheck.Application.Projects.Commands.CreateProject;
using IgnaCheck.Domain.Entities;
using IgnaCheck.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace IgnaCheck.Application.UnitTests.Projects.Commands;

[TestFixture]
public class CreateProjectCommandTests
{
    private Mock<IApplicationDbContext> _mockDbContext;
    private Mock<IUser> _mockCurrentUser;
    private Mock<ITenantService> _mockTenantService;
    private Mock<IIdentityService> _mockIdentityService;
    private Mock<DbSet<Organization>> _mockOrganizationsDbSet;
    private Mock<DbSet<Project>> _mockProjectsDbSet;
    private Mock<DbSet<ProjectMember>> _mockProjectMembersDbSet;
    private Mock<DbSet<ActivityLog>> _mockActivityLogsDbSet;
    private CreateProjectCommandHandler _handler;
    private List<Organization> _organizations;
    private List<Project> _projects;

    [SetUp]
    public void SetUp()
    {
        _mockDbContext = new Mock<IApplicationDbContext>();
        _mockCurrentUser = new Mock<IUser>();
        _mockTenantService = new Mock<ITenantService>();
        _mockIdentityService = new Mock<IIdentityService>();

        _organizations = new List<Organization>();
        _projects = new List<Project>();

        _mockOrganizationsDbSet = CreateMockDbSet(_organizations);
        _mockProjectsDbSet = CreateMockDbSet(_projects);
        _mockProjectMembersDbSet = new Mock<DbSet<ProjectMember>>();
        _mockActivityLogsDbSet = new Mock<DbSet<ActivityLog>>();

        _mockDbContext.Setup(x => x.Organizations).Returns(_mockOrganizationsDbSet.Object);
        _mockDbContext.Setup(x => x.Projects).Returns(_mockProjectsDbSet.Object);
        _mockDbContext.Setup(x => x.ProjectMembers).Returns(_mockProjectMembersDbSet.Object);
        _mockDbContext.Setup(x => x.ActivityLogs).Returns(_mockActivityLogsDbSet.Object);
        _mockDbContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _handler = new CreateProjectCommandHandler(
            _mockDbContext.Object,
            _mockCurrentUser.Object,
            _mockTenantService.Object,
            _mockIdentityService.Object);
    }

    [Test]
    public async Task Handle_Should_CreateProject_When_ValidRequest()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var organizationId = Guid.NewGuid();
        var organization = new Organization
        {
            Id = organizationId,
            Name = "Test Org",
            IsActive = true
        };
        _organizations.Add(organization);

        var user = new ApplicationUserDto
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        _mockCurrentUser.Setup(x => x.Id).Returns(userId);
        _mockTenantService.Setup(x => x.GetCurrentTenantId()).Returns(organizationId);
        _mockIdentityService.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(user);

        var command = new CreateProjectCommand
        {
            Name = "SOC 2 Audit",
            Description = "Annual SOC 2 Type II audit",
            TargetDate = DateTime.UtcNow.AddMonths(3)
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Name.ShouldBe("SOC 2 Audit");
        result.Data.Description.ShouldBe("Annual SOC 2 Type II audit");
        result.Data.Status.ShouldBe(ProjectStatus.Draft);
        result.Data.Id.ShouldNotBe(Guid.Empty);

        _mockProjectsDbSet.Verify(x => x.Add(It.IsAny<Project>()), Times.Once);
        _mockProjectMembersDbSet.Verify(x => x.Add(It.IsAny<ProjectMember>()), Times.Once);
        _mockActivityLogsDbSet.Verify(x => x.Add(It.IsAny<ActivityLog>()), Times.Once);
        _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_Should_ReturnFailure_When_UserNotAuthenticated()
    {
        // Arrange
        _mockCurrentUser.Setup(x => x.Id).Returns((string)null!);

        var command = new CreateProjectCommand { Name = "Test Project" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain("User must be authenticated to create a project.");
    }

    [Test]
    public async Task Handle_Should_ReturnFailure_When_NoWorkspaceSelected()
    {
        // Arrange
        _mockCurrentUser.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());
        _mockTenantService.Setup(x => x.GetCurrentTenantId()).Returns((Guid?)null);

        var command = new CreateProjectCommand { Name = "Test Project" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain("No workspace selected. Please select a workspace first.");
    }

    [Test]
    public async Task Handle_Should_ReturnFailure_When_OrganizationNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var organizationId = Guid.NewGuid();

        _mockCurrentUser.Setup(x => x.Id).Returns(userId);
        _mockTenantService.Setup(x => x.GetCurrentTenantId()).Returns(organizationId);

        var command = new CreateProjectCommand { Name = "Test Project" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain("Workspace not found.");
    }

    [Test]
    public async Task Handle_Should_ReturnFailure_When_OrganizationNotActive()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var organizationId = Guid.NewGuid();
        var organization = new Organization
        {
            Id = organizationId,
            Name = "Test Org",
            IsActive = false
        };
        _organizations.Add(organization);

        _mockCurrentUser.Setup(x => x.Id).Returns(userId);
        _mockTenantService.Setup(x => x.GetCurrentTenantId()).Returns(organizationId);

        var command = new CreateProjectCommand { Name = "Test Project" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain("Workspace is not active.");
    }

    [Test]
    public async Task Handle_Should_ReturnFailure_When_ProjectLimitReached()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var organizationId = Guid.NewGuid();
        var organization = new Organization
        {
            Id = organizationId,
            Name = "Test Org",
            IsActive = true,
            MaxProjects = 5,
            SubscriptionTier = "Starter"
        };
        _organizations.Add(organization);

        // Add 5 existing projects
        for (int i = 0; i < 5; i++)
        {
            _projects.Add(new Project
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                Name = $"Project {i + 1}"
            });
        }

        _mockCurrentUser.Setup(x => x.Id).Returns(userId);
        _mockTenantService.Setup(x => x.GetCurrentTenantId()).Returns(organizationId);

        var command = new CreateProjectCommand { Name = "Test Project" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Project limit reached"));
    }

    [Test]
    public async Task Handle_Should_TrimWhitespace_From_ProjectName()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var organizationId = Guid.NewGuid();
        var organization = new Organization
        {
            Id = organizationId,
            Name = "Test Org",
            IsActive = true
        };
        _organizations.Add(organization);

        var user = new ApplicationUserDto
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        _mockCurrentUser.Setup(x => x.Id).Returns(userId);
        _mockTenantService.Setup(x => x.GetCurrentTenantId()).Returns(organizationId);
        _mockIdentityService.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(user);

        var command = new CreateProjectCommand
        {
            Name = "  SOC 2 Audit  ",
            Description = "  Test Description  "
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeTrue();
        result.Data.Name.ShouldBe("SOC 2 Audit");
        result.Data.Description.ShouldBe("Test Description");
    }

    private Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var mockSet = new Mock<DbSet<T>>();

        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

        mockSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));

        mockSet.As<IQueryable<T>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));

        return mockSet;
    }
}

// Helper classes for async testing
internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object Execute(System.Linq.Expressions.Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default)
    {
        var resultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethod(
                name: nameof(IQueryProvider.Execute),
                genericParameterCount: 1,
                types: new[] { typeof(System.Linq.Expressions.Expression) })!
            .MakeGenericMethod(resultType)
            .Invoke(this, new[] { expression });

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(resultType)
            .Invoke(null, new[] { executionResult })!;
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(System.Linq.Expressions.Expression expression)
        : base(expression)
    {
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public ValueTask<bool> MoveNextAsync()
    {
        return new ValueTask<bool>(_inner.MoveNext());
    }

    public T Current => _inner.Current;

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return new ValueTask();
    }
}


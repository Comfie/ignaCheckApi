using IgnaCheck.Application.Common.EventHandlers;
using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Entities;
using IgnaCheck.Domain.Enums;
using IgnaCheck.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IgnaCheck.Application.UnitTests.EventHandlers;

/// <summary>
/// Unit tests for EntityCreatedEventHandler to verify automatic audit logging on creation.
/// </summary>
public class EntityCreatedEventHandlerTests
{
    private Mock<IApplicationDbContext> _mockContext = null!;
    private Mock<IUser> _mockCurrentUser = null!;
    private Mock<ITenantService> _mockTenantService = null!;
    private Mock<IIdentityService> _mockIdentityService = null!;
    private Mock<ILogger<EntityCreatedEventHandler>> _mockLogger = null!;
    private EntityCreatedEventHandler _handler = null!;
    private List<ActivityLog> _activityLogs = null!;

    [SetUp]
    public void Setup()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockCurrentUser = new Mock<IUser>();
        _mockTenantService = new Mock<ITenantService>();
        _mockIdentityService = new Mock<IIdentityService>();
        _mockLogger = new Mock<ILogger<EntityCreatedEventHandler>>();

        _activityLogs = new List<ActivityLog>();

        var mockActivityLogSet = CreateMockDbSet(_activityLogs);
        _mockContext.Setup(x => x.ActivityLogs).Returns(mockActivityLogSet.Object);

        _handler = new EntityCreatedEventHandler(
            _mockContext.Object,
            _mockCurrentUser.Object,
            _mockTenantService.Object,
            _mockIdentityService.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task Handle_WhenProjectCreated_ShouldCreateActivityLog()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var userId = "user-123";
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "New Project",
            OrganizationId = organizationId,
            Created = DateTime.UtcNow,
            CreatedBy = userId
        };

        var createdEvent = new EntityCreatedEvent(project);

        _mockCurrentUser.Setup(x => x.Id).Returns(userId);
        _mockTenantService.Setup(x => x.GetCurrentTenantId()).Returns(organizationId);
        _mockIdentityService.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(new IgnaCheck.Application.Common.Models.ApplicationUserDto
            {
                Id = userId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com"
            });

        // Act
        await _handler.Handle(createdEvent, CancellationToken.None);

        // Assert
        Assert.That(_activityLogs, Has.Count.EqualTo(1));
        var log = _activityLogs.First();

        Assert.That(log.ActivityType, Is.EqualTo(ActivityType.ProjectCreated));
        Assert.That(log.EntityType, Is.EqualTo("Project"));
        Assert.That(log.EntityId, Is.EqualTo(project.Id));
        Assert.That(log.EntityName, Is.EqualTo("New Project"));
        Assert.That(log.UserId, Is.EqualTo(userId));
        Assert.That(log.UserName, Is.EqualTo("John Doe"));
        Assert.That(log.Description, Does.Contain("Created project 'New Project'"));
    }

    [Test]
    public async Task Handle_WhenDocumentCreated_ShouldCreateActivityLog()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var userId = "user-123";
        var document = new Document
        {
            Id = Guid.NewGuid(),
            FileName = "report.pdf",
            ProjectId = projectId,
            Created = DateTime.UtcNow,
            CreatedBy = userId
        };

        var createdEvent = new EntityCreatedEvent(document);

        _mockCurrentUser.Setup(x => x.Id).Returns(userId);
        _mockTenantService.Setup(x => x.GetCurrentTenantId()).Returns(organizationId);
        _mockIdentityService.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(new IgnaCheck.Application.Common.Models.ApplicationUserDto
            {
                Id = userId,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com"
            });

        // Act
        await _handler.Handle(createdEvent, CancellationToken.None);

        // Assert
        Assert.That(_activityLogs, Has.Count.EqualTo(1));
        var log = _activityLogs.First();

        Assert.That(log.ActivityType, Is.EqualTo(ActivityType.DocumentUploaded));
        Assert.That(log.EntityType, Is.EqualTo("Document"));
        Assert.That(log.ProjectId, Is.EqualTo(projectId));
        Assert.That(log.Description, Does.Contain("Created document 'report.pdf'"));
    }

    [Test]
    public async Task Handle_WhenOrganizationCreated_ShouldCreateActivityLog()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var userId = "user-123";
        var organization = new Organization
        {
            Id = organizationId,
            Name = "New Workspace",
            Created = DateTime.UtcNow,
            CreatedBy = userId
        };

        var createdEvent = new EntityCreatedEvent(organization);

        _mockCurrentUser.Setup(x => x.Id).Returns(userId);
        _mockTenantService.Setup(x => x.GetCurrentTenantId()).Returns(organizationId);
        _mockIdentityService.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(new IgnaCheck.Application.Common.Models.ApplicationUserDto
            {
                Id = userId,
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@example.com"
            });

        // Act
        await _handler.Handle(createdEvent, CancellationToken.None);

        // Assert
        Assert.That(_activityLogs, Has.Count.EqualTo(1));
        var log = _activityLogs.First();

        Assert.That(log.ActivityType, Is.EqualTo(ActivityType.WorkspaceCreated));
        Assert.That(log.EntityName, Is.EqualTo("New Workspace"));
    }

    [Test]
    public async Task Handle_WhenOrganizationCreated_WithoutSetup_ShouldNotCreateLog()
    {
        // Arrange - Organization created without proper mocks
        _mockCurrentUser.Setup(x => x.Id).Returns("user-123");
        _mockTenantService.Setup(x => x.GetCurrentTenantId()).Returns((Guid?)null); // No tenant context

        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Test Org"
        };

        var createdEvent = new EntityCreatedEvent(organization);

        // Act
        await _handler.Handle(createdEvent, CancellationToken.None);

        // Assert - No log should be created without tenant context
        Assert.That(_activityLogs, Is.Empty);
    }

    [Test]
    public async Task Handle_MetadataContainsCreationInfo()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var userId = "user-123";
        var createdAt = DateTime.UtcNow;
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            OrganizationId = organizationId,
            Created = createdAt,
            CreatedBy = userId
        };

        var createdEvent = new EntityCreatedEvent(project);

        _mockCurrentUser.Setup(x => x.Id).Returns(userId);
        _mockTenantService.Setup(x => x.GetCurrentTenantId()).Returns(organizationId);
        _mockIdentityService.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(new IgnaCheck.Application.Common.Models.ApplicationUserDto
            {
                Id = userId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com"
            });

        // Act
        await _handler.Handle(createdEvent, CancellationToken.None);

        // Assert
        var log = _activityLogs.First();
        Assert.That(log.Metadata, Is.Not.Null);
        Assert.That(log.Metadata, Does.Contain("CreatedAt"));
        Assert.That(log.Metadata, Does.Contain("CreatedBy"));
        Assert.That(log.Metadata, Does.Contain(userId));
    }

    [Test]
    public async Task Handle_WhenNoOrganizationContext_ShouldNotCreateLog()
    {
        // Arrange
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            OrganizationId = Guid.NewGuid()
        };

        var createdEvent = new EntityCreatedEvent(project);

        _mockCurrentUser.Setup(x => x.Id).Returns("user-123");
        _mockTenantService.Setup(x => x.GetCurrentTenantId()).Returns((Guid?)null);

        // Act
        await _handler.Handle(createdEvent, CancellationToken.None);

        // Assert
        Assert.That(_activityLogs, Is.Empty);
    }

    [Test]
    public async Task Handle_WhenExceptionOccurs_ShouldNotThrow()
    {
        // Arrange
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            OrganizationId = Guid.NewGuid()
        };

        var createdEvent = new EntityCreatedEvent(project);

        _mockCurrentUser.Setup(x => x.Id).Returns("user-123");
        _mockTenantService.Setup(x => x.GetCurrentTenantId()).Returns(Guid.NewGuid());
        _mockIdentityService.Setup(x => x.GetUserByIdAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert - Should not throw
        await _handler.Handle(createdEvent, CancellationToken.None);

        // Verify error was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var mockSet = new Mock<DbSet<T>>();

        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

        mockSet.Setup(m => m.Add(It.IsAny<T>())).Callback<T>(data.Add);

        return mockSet;
    }
}

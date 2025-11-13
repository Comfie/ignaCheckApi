using IgnaCheck.Application.Common.EventHandlers;
using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Entities;
using IgnaCheck.Domain.Enums;
using IgnaCheck.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Application.UnitTests.EventHandlers;

/// <summary>
/// Unit tests for EntityDeletedEventHandler to verify automatic audit logging on deletion.
/// </summary>
public class EntityDeletedEventHandlerTests
{
    private Mock<IApplicationDbContext> _mockContext = null!;
    private Mock<IUser> _mockCurrentUser = null!;
    private Mock<ITenantService> _mockTenantService = null!;
    private Mock<IIdentityService> _mockIdentityService = null!;
    private Mock<ILogger<EntityDeletedEventHandler>> _mockLogger = null!;
    private EntityDeletedEventHandler _handler = null!;
    private List<ActivityLog> _activityLogs = null!;

    [SetUp]
    public void Setup()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockCurrentUser = new Mock<IUser>();
        _mockTenantService = new Mock<ITenantService>();
        _mockIdentityService = new Mock<IIdentityService>();
        _mockLogger = new Mock<ILogger<EntityDeletedEventHandler>>();

        _activityLogs = new List<ActivityLog>();

        var mockActivityLogSet = CreateMockDbSet(_activityLogs);
        _mockContext.Setup(x => x.ActivityLogs).Returns(mockActivityLogSet.Object);

        _handler = new EntityDeletedEventHandler(
            _mockContext.Object,
            _mockCurrentUser.Object,
            _mockTenantService.Object,
            _mockIdentityService.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task Handle_WhenProjectDeleted_ShouldCreateActivityLog()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var userId = "user-123";
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            OrganizationId = organizationId,
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow,
            DeletedBy = userId
        };

        var deletedEvent = new EntityDeletedEvent(project);

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
        await _handler.Handle(deletedEvent, CancellationToken.None);

        // Assert
        Assert.That(_activityLogs, Has.Count.EqualTo(1));
        var log = _activityLogs.First();

        Assert.That(log.ActivityType, Is.EqualTo(ActivityType.ProjectDeleted));
        Assert.That(log.EntityType, Is.EqualTo("Project"));
        Assert.That(log.EntityId, Is.EqualTo(project.Id));
        Assert.That(log.EntityName, Is.EqualTo("Test Project"));
        Assert.That(log.UserId, Is.EqualTo(userId));
        Assert.That(log.UserName, Is.EqualTo("John Doe"));
        Assert.That(log.UserEmail, Is.EqualTo("john.doe@example.com"));
        Assert.That(log.Description, Does.Contain("Deleted project 'Test Project'"));
    }

    [Test]
    public async Task Handle_WhenDocumentDeleted_ShouldCreateActivityLog()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var userId = "user-123";
        var document = new Document
        {
            Id = Guid.NewGuid(),
            FileName = "test-doc.pdf",
            ProjectId = projectId,
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow,
            DeletedBy = userId
        };

        var deletedEvent = new EntityDeletedEvent(document);

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
        await _handler.Handle(deletedEvent, CancellationToken.None);

        // Assert
        Assert.That(_activityLogs, Has.Count.EqualTo(1));
        var log = _activityLogs.First();

        Assert.That(log.ActivityType, Is.EqualTo(ActivityType.DocumentDeleted));
        Assert.That(log.EntityType, Is.EqualTo("Document"));
        Assert.That(log.EntityId, Is.EqualTo(document.Id));
        Assert.That(log.EntityName, Is.EqualTo("test-doc.pdf"));
        Assert.That(log.ProjectId, Is.EqualTo(projectId));
        Assert.That(log.Description, Does.Contain("Deleted document 'test-doc.pdf'"));
    }

    [Test]
    public async Task Handle_WhenOrganizationDeleted_ShouldCreateActivityLog()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var userId = "user-123";
        var organization = new Organization
        {
            Id = organizationId,
            Name = "Test Workspace",
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow,
            DeletedBy = userId
        };

        var deletedEvent = new EntityDeletedEvent(organization);

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
        await _handler.Handle(deletedEvent, CancellationToken.None);

        // Assert
        Assert.That(_activityLogs, Has.Count.EqualTo(1));
        var log = _activityLogs.First();

        Assert.That(log.ActivityType, Is.EqualTo(ActivityType.WorkspaceDeleted));
        Assert.That(log.EntityType, Is.EqualTo("Organization"));
        Assert.That(log.EntityName, Is.EqualTo("Test Workspace"));
    }

    [Test]
    public async Task Handle_WhenNoOrganizationContext_ShouldNotCreateLog()
    {
        // Arrange
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            OrganizationId = Guid.NewGuid(),
            IsDeleted = true
        };

        var deletedEvent = new EntityDeletedEvent(project);

        _mockCurrentUser.Setup(x => x.Id).Returns("user-123");
        _mockTenantService.Setup(x => x.GetCurrentTenantId()).Returns((Guid?)null); // No organization context

        // Act
        await _handler.Handle(deletedEvent, CancellationToken.None);

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
            OrganizationId = Guid.NewGuid(),
            IsDeleted = true
        };

        var deletedEvent = new EntityDeletedEvent(project);

        _mockCurrentUser.Setup(x => x.Id).Returns("user-123");
        _mockTenantService.Setup(x => x.GetCurrentTenantId()).Returns(Guid.NewGuid());
        _mockIdentityService.Setup(x => x.GetUserByIdAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert - Should not throw
        await _handler.Handle(deletedEvent, CancellationToken.None);

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

    [Test]
    public async Task Handle_MetadataContainsDeletionInfo()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var userId = "user-123";
        var deletedAt = DateTime.UtcNow;
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            OrganizationId = organizationId,
            IsDeleted = true,
            DeletedAt = deletedAt,
            DeletedBy = userId
        };

        var deletedEvent = new EntityDeletedEvent(project);

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
        await _handler.Handle(deletedEvent, CancellationToken.None);

        // Assert
        var log = _activityLogs.First();
        Assert.That(log.Metadata, Is.Not.Null);
        Assert.That(log.Metadata, Does.Contain("DeletedAt"));
        Assert.That(log.Metadata, Does.Contain("DeletedBy"));
        Assert.That(log.Metadata, Does.Contain("IsDeleted"));
        // Assert userId is in metadata
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

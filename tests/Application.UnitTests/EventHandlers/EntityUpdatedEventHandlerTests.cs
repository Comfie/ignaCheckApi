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
/// Unit tests for EntityUpdatedEventHandler to verify automatic audit logging on updates.
/// </summary>
public class EntityUpdatedEventHandlerTests
{
    private Mock<IApplicationDbContext> _mockContext = null!;
    private Mock<IUser> _mockCurrentUser = null!;
    private Mock<ITenantService> _mockTenantService = null!;
    private Mock<IIdentityService> _mockIdentityService = null!;
    private Mock<ILogger<EntityUpdatedEventHandler>> _mockLogger = null!;
    private EntityUpdatedEventHandler _handler = null!;
    private List<ActivityLog> _activityLogs = null!;

    [SetUp]
    public void Setup()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockCurrentUser = new Mock<IUser>();
        _mockTenantService = new Mock<ITenantService>();
        _mockIdentityService = new Mock<IIdentityService>();
        _mockLogger = new Mock<ILogger<EntityUpdatedEventHandler>>();

        _activityLogs = new List<ActivityLog>();

        var mockActivityLogSet = CreateMockDbSet(_activityLogs);
        _mockContext.Setup(x => x.ActivityLogs).Returns(mockActivityLogSet.Object);

        _handler = new EntityUpdatedEventHandler(
            _mockContext.Object,
            _mockCurrentUser.Object,
            _mockTenantService.Object,
            _mockIdentityService.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task Handle_WhenSignificantPropertyUpdated_ShouldCreateActivityLog()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var userId = "user-123";
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Updated Project",
            OrganizationId = organizationId
        };

        var modifiedProperties = new[] { "Name", "Description" };
        var updatedEvent = new EntityUpdatedEvent(project, modifiedProperties);

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
        await _handler.Handle(updatedEvent, CancellationToken.None);

        // Assert
        Assert.That(_activityLogs, Has.Count.EqualTo(1));
        var log = _activityLogs.First();

        Assert.That(log.ActivityType, Is.EqualTo(ActivityType.ProjectUpdated));
        Assert.That(log.EntityType, Is.EqualTo("Project"));
        Assert.That(log.EntityId, Is.EqualTo(project.Id));
        Assert.That(log.Description, Does.Contain("Updated project"));
        Assert.That(log.Description, Does.Contain("Name, Description"));
    }

    [Test]
    public async Task Handle_WhenOnlyAuditFieldsUpdated_ShouldNotCreateLog()
    {
        // Arrange
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            OrganizationId = Guid.NewGuid()
        };

        // Only audit fields changed (LastModified, LastModifiedBy)
        var modifiedProperties = new[] { "LastModified", "LastModifiedBy" };
        var updatedEvent = new EntityUpdatedEvent(project, modifiedProperties);

        _mockCurrentUser.Setup(x => x.Id).Returns("user-123");
        _mockTenantService.Setup(x => x.GetCurrentTenantId()).Returns(Guid.NewGuid());

        // Act
        await _handler.Handle(updatedEvent, CancellationToken.None);

        // Assert - No log should be created for audit-only field changes
        Assert.That(_activityLogs, Is.Empty);
    }

    [Test]
    public async Task Handle_WhenMultiplePropertiesUpdated_MetadataContainsAll()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var userId = "user-123";
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Updated Project",
            Description = "New Description",
            Status = ProjectStatus.Active,
            OrganizationId = organizationId
        };

        var modifiedProperties = new[] { "Name", "Description", "Status" };
        var updatedEvent = new EntityUpdatedEvent(project, modifiedProperties);

        _mockCurrentUser.Setup(x => x.Id).Returns(userId);
        _mockTenantService.Setup(x => x.GetCurrentTenantId()).Returns(organizationId);
        _mockIdentityService.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(new IgnaCheck.Application.Common.Models.ApplicationUserDto
            {
                Id = userId,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@example.com"
            });

        // Act
        await _handler.Handle(updatedEvent, CancellationToken.None);

        // Assert
        var log = _activityLogs.First();
        Assert.That(log.Metadata, Is.Not.Null);
        Assert.That(log.Metadata, Does.Contain("ModifiedProperties"));
        Assert.That(log.Metadata, Does.Contain("Name"));
        Assert.That(log.Metadata, Does.Contain("Description"));
        Assert.That(log.Metadata, Does.Contain("Status"));
    }

    [Test]
    public async Task Handle_WhenDocumentUpdated_ShouldSetCorrectProjectId()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var userId = "user-123";
        var document = new Document
        {
            Id = Guid.NewGuid(),
            FileName = "updated-doc.pdf",
            ProjectId = projectId,
            Category = DocumentCategory.Policy
        };

        var modifiedProperties = new[] { "Category" };
        var updatedEvent = new EntityUpdatedEvent(document, modifiedProperties);

        _mockCurrentUser.Setup(x => x.Id).Returns(userId);
        _mockTenantService.Setup(x => x.GetCurrentTenantId()).Returns(organizationId);
        _mockIdentityService.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(new IgnaCheck.Application.Common.Models.ApplicationUserDto
            {
                Id = userId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com"
            });

        // Act
        await _handler.Handle(updatedEvent, CancellationToken.None);

        // Assert
        Assert.That(_activityLogs, Has.Count.EqualTo(1));
        var log = _activityLogs.First();
        Assert.That(log.ProjectId, Is.EqualTo(projectId));
        Assert.That(log.ActivityType, Is.EqualTo(ActivityType.DocumentUpdated));
    }

    [Test]
    public async Task Handle_WhenFindingStatusChanged_ShouldLog()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var userId = "user-123";
        var finding = new ComplianceFinding
        {
            Id = Guid.NewGuid(),
            Title = "Critical Finding",
            ProjectId = projectId,
            OrganizationId = organizationId,
            WorkflowStatus = FindingWorkflowStatus.InProgress
        };

        var modifiedProperties = new[] { "WorkflowStatus" };
        var updatedEvent = new EntityUpdatedEvent(finding, modifiedProperties);

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
        await _handler.Handle(updatedEvent, CancellationToken.None);

        // Assert
        Assert.That(_activityLogs, Has.Count.EqualTo(1));
        var log = _activityLogs.First();
        Assert.That(log.ActivityType, Is.EqualTo(ActivityType.FindingUpdated));
        Assert.That(log.EntityName, Is.EqualTo("Critical Finding"));
        Assert.That(log.Description, Does.Contain("WorkflowStatus"));
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

        var modifiedProperties = new[] { "Name" };
        var updatedEvent = new EntityUpdatedEvent(project, modifiedProperties);

        _mockCurrentUser.Setup(x => x.Id).Returns("user-123");
        _mockTenantService.Setup(x => x.GetCurrentTenantId()).Returns((Guid?)null);

        // Act
        await _handler.Handle(updatedEvent, CancellationToken.None);

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

        var modifiedProperties = new[] { "Name" };
        var updatedEvent = new EntityUpdatedEvent(project, modifiedProperties);

        _mockCurrentUser.Setup(x => x.Id).Returns("user-123");
        _mockTenantService.Setup(x => x.GetCurrentTenantId()).Returns(Guid.NewGuid());
        _mockIdentityService.Setup(x => x.GetUserByIdAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert - Should not throw
        await _handler.Handle(updatedEvent, CancellationToken.None);

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
    public async Task Handle_WhenTooManyPropertiesChanged_DescriptionDoesNotListAll()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var userId = "user-123";
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            OrganizationId = organizationId
        };

        // More than 3 properties changed
        var modifiedProperties = new[] { "Name", "Description", "Status", "TargetDate" };
        var updatedEvent = new EntityUpdatedEvent(project, modifiedProperties);

        _mockCurrentUser.Setup(x => x.Id).Returns(userId);
        _mockTenantService.Setup(x => x.GetCurrentTenantId()).Returns(organizationId);
        _mockIdentityService.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(new IgnaCheck.Application.Common.Models.ApplicationUserDto
            {
                Id = userId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com"
            });

        // Act
        await _handler.Handle(updatedEvent, CancellationToken.None);

        // Assert
        var log = _activityLogs.First();
        // Description should not list individual properties when there are too many
        Assert.That(log.Description, Does.Not.Contain("Name, Description, Status, TargetDate"));
        Assert.That(log.Description, Does.Contain("Updated project"));
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

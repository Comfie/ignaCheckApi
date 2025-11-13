# Soft Delete Feature - Unit Testing Guide

## Overview

This document explains the unit tests created for the soft delete and automatic audit logging feature. The tests ensure that entities are properly soft-deleted (marked as deleted rather than physically removed) and that all changes are automatically logged for audit purposes.

## Test Architecture

The soft delete feature involves multiple layers of the application:

1. **Infrastructure Layer**: Interceptors that handle the soft delete logic
2. **Application Layer**: Event handlers that create audit logs
3. **Domain Layer**: Events that are raised when entities are created, updated, or deleted

### Testing Strategy

- **Unit Tests**: Test individual components in isolation using mocks
- **Integration Tests**: Test the entire API workflow with a real database
- **In-Memory Database**: Used for infrastructure tests to verify EF Core behavior

## Test Files Overview

### 1. SoftDeleteInterceptorTests.cs

**Location**: `tests/Infrastructure.UnitTests/Interceptors/SoftDeleteInterceptorTests.cs`

**Purpose**: Tests the `SoftDeleteInterceptor` that converts hard deletes (EF Core `Remove()`) into soft deletes (setting `IsDeleted = true`).

#### Key Tests

##### Test: Convert Hard Delete to Soft Delete
```csharp
[Test]
public async Task SavingChangesAsync_WhenEntityDeleted_ShouldConvertToSoftDelete()
```

**What it tests**:
- When `context.Projects.Remove(project)` is called
- The interceptor intercepts the delete operation
- Instead of physically deleting, it sets:
  - `IsDeleted = true`
  - `DeletedAt = DateTime.UtcNow`
  - `DeletedBy = current user ID`

**Verification**:
```csharp
var deletedProject = await context.Projects
    .IgnoreQueryFilters()  // Bypasses soft delete filter to see deleted records
    .FirstOrDefaultAsync(p => p.Id == project.Id);

Assert.That(deletedProject, Is.Not.Null);  // Still exists in database
Assert.That(deletedProject.IsDeleted, Is.True);
Assert.That(deletedProject.DeletedBy, Is.EqualTo("test-user-123"));
```

##### Test: Raise Domain Event on Delete
```csharp
[Test]
public async Task SavingChangesAsync_WhenEntityDeleted_ShouldRaiseEntityDeletedEvent()
```

**What it tests**:
- When an entity is soft-deleted
- An `EntityDeletedEvent` domain event is raised
- This event will trigger the `EntityDeletedEventHandler` to create an audit log

##### Test: Multiple Entity Deletion
```csharp
[Test]
public async Task SavingChangesAsync_WhenMultipleEntitiesDeleted_ShouldSoftDeleteAll()
```

**What it tests**:
- Bulk delete operations work correctly
- All entities in a `RemoveRange()` are soft-deleted
- Each entity gets its own deletion metadata

##### Test: Normal Updates Not Affected
```csharp
[Test]
public async Task SavingChangesAsync_WhenNormalUpdate_ShouldNotAffectIsDeleted()
```

**What it tests**:
- Regular update operations (`entity.Name = "New Name"`) don't trigger soft delete
- `IsDeleted` remains `false` for normal updates

##### Test: UTC Timestamp
```csharp
[Test]
public async Task SavingChangesAsync_DeletedAtTimestamp_ShouldBeUtc()
```

**What it tests**:
- `DeletedAt` timestamp is always stored in UTC
- Ensures consistency across time zones

#### Testing Approach

**Setup**:
```csharp
[SetUp]
public void Setup()
{
    _mockUser = new Mock<IUser>();
    _mockUser.Setup(x => x.Id).Returns("test-user-123");
    _timeProvider = TimeProvider.System;
    _interceptor = new SoftDeleteInterceptor(_mockUser.Object, _timeProvider);
}
```

**Test Context**:
- Uses EF Core `InMemoryDatabase` for fast, isolated tests
- Interceptor is added to the DbContext via `.AddInterceptors(_interceptor)`
- Each test gets a fresh database with unique name: `Guid.NewGuid().ToString()`

---

### 2. SoftDeleteQueryFilterTests.cs

**Location**: `tests/Infrastructure.UnitTests/Data/SoftDeleteQueryFilterTests.cs`

**Purpose**: Tests that global query filters automatically hide soft-deleted entities from normal queries.

#### Key Tests

##### Test: Deleted Entities Not Returned
```csharp
[Test]
public async Task Query_WithoutIgnoreQueryFilters_ShouldNotReturnDeletedEntities()
```

**What it tests**:
- Normal queries (`context.Projects.ToListAsync()`) automatically exclude soft-deleted entities
- The global query filter `!p.IsDeleted` is applied automatically

**Example**:
```csharp
// Setup: Create 2 projects, delete 1
var activeProject = new Project { Name = "Active" };
var deletedProject = new Project { Name = "Deleted" };
context.Projects.AddRange(activeProject, deletedProject);
await context.SaveChangesAsync();

context.Projects.Remove(deletedProject);
await context.SaveChangesAsync();

// Normal query only returns active projects
var projects = await context.Projects.ToListAsync();
Assert.That(projects, Has.Count.EqualTo(1));
Assert.That(projects.First().Name, Is.EqualTo("Active"));
```

##### Test: IgnoreQueryFilters Shows Deleted
```csharp
[Test]
public async Task Query_WithIgnoreQueryFilters_ShouldReturnDeletedEntities()
```

**What it tests**:
- Using `.IgnoreQueryFilters()` bypasses the soft delete filter
- Allows admin features to view/restore deleted entities

```csharp
var allProjects = await context.Projects
    .IgnoreQueryFilters()
    .ToListAsync();

Assert.That(allProjects.Count, Is.EqualTo(2));  // Both active and deleted
```

##### Test: Query Only Deleted
```csharp
[Test]
public async Task Query_OnlyDeletedEntities_ShouldReturnOnlyDeleted()
```

**What it tests**:
- Can explicitly query for deleted entities
- Useful for "trash bin" or "recently deleted" features

```csharp
var deletedProjects = await context.Projects
    .IgnoreQueryFilters()
    .Where(p => p.IsDeleted)
    .ToListAsync();
```

##### Test: Count Excludes Deleted
```csharp
[Test]
public async Task Count_WithoutIgnoreQueryFilters_ShouldExcludeDeletedEntities()
```

**What it tests**:
- Aggregate operations like `Count()` respect soft delete filter
- Ensures accurate counts in the UI

```csharp
// 5 projects created, 2 deleted
var activeCount = await context.Projects.CountAsync();  // Returns 3
var totalCount = await context.Projects.IgnoreQueryFilters().CountAsync();  // Returns 5
```

##### Test: FindById Returns Null for Deleted
```csharp
[Test]
public async Task FirstOrDefault_OnDeletedEntity_ShouldReturnNull()
```

**What it tests**:
- Querying by ID for a deleted entity returns null
- Protects against accessing deleted resources

##### Test: Restore Deleted Entity
```csharp
[Test]
public async Task RestoreDeletedEntity_ShouldMakeItVisibleAgain()
```

**What it tests**:
- Entities can be "undeleted" by setting `IsDeleted = false`
- Demonstrates how to implement a restore feature

```csharp
deletedProject.IsDeleted = false;
deletedProject.DeletedAt = null;
deletedProject.DeletedBy = null;
await context.SaveChangesAsync();

// Now visible in normal queries
var restoredProject = await context.Projects
    .FirstOrDefaultAsync(p => p.Id == projectId);
Assert.That(restoredProject, Is.Not.Null);
```

#### Query Filter Configuration

The query filter is configured in `DbContext.OnModelCreating`:

```csharp
modelBuilder.Entity<Project>()
    .HasQueryFilter(p => !p.IsDeleted);
```

This applies to ALL queries automatically unless bypassed with `IgnoreQueryFilters()`.

---

### 3. EntityDeletedEventHandlerTests.cs

**Location**: `tests/Application.UnitTests/EventHandlers/EntityDeletedEventHandlerTests.cs`

**Purpose**: Tests the automatic creation of audit logs when entities are deleted.

#### Key Tests

##### Test: Create Activity Log on Deletion
```csharp
[Test]
public async Task Handle_WhenProjectDeleted_ShouldCreateActivityLog()
```

**What it tests**:
- When `EntityDeletedEvent` is raised
- `EntityDeletedEventHandler` creates an `ActivityLog` entry
- The log contains correct metadata

**Activity Log Fields Verified**:
```csharp
Assert.That(log.ActivityType, Is.EqualTo(ActivityType.ProjectDeleted));
Assert.That(log.EntityType, Is.EqualTo("Project"));
Assert.That(log.EntityId, Is.EqualTo(project.Id));
Assert.That(log.EntityName, Is.EqualTo("Test Project"));
Assert.That(log.UserId, Is.EqualTo(userId));
Assert.That(log.UserName, Is.EqualTo("John Doe"));
Assert.That(log.Description, Does.Contain("Deleted project"));
```

##### Test: Different Entity Types
```csharp
[Test]
public async Task Handle_WhenDocumentDeleted_ShouldCreateActivityLog()

[Test]
public async Task Handle_WhenOrganizationDeleted_ShouldCreateActivityLog()
```

**What it tests**:
- Each entity type gets appropriate `ActivityType`:
  - Project → `ProjectDeleted`
  - Document → `DocumentDeleted`
  - Organization → `WorkspaceDeleted`
- Document deletions link to parent project via `ProjectId`

##### Test: No Organization Context = No Log
```csharp
[Test]
public async Task Handle_WhenNoOrganizationContext_ShouldNotCreateLog()
```

**What it tests**:
- Multi-tenancy enforcement
- If `TenantService.GetCurrentTenantId()` returns null, no log is created
- Prevents orphaned logs from background jobs or system operations

##### Test: Exception Handling
```csharp
[Test]
public async Task Handle_WhenExceptionOccurs_ShouldNotThrow()
```

**What it tests**:
- Event handlers never throw exceptions (would break the save operation)
- Errors are logged but don't fail the entire transaction

**Verification**:
```csharp
_mockIdentityService.Setup(x => x.GetUserByIdAsync(It.IsAny<string>()))
    .ThrowsAsync(new Exception("Database error"));

await _handler.Handle(updatedEvent, CancellationToken.None);  // Should not throw

// Verify error was logged
_mockLogger.Verify(
    x => x.Log(LogLevel.Error, ...),
    Times.Once);
```

##### Test: Metadata Contains Deletion Info
```csharp
[Test]
public async Task Handle_MetadataContainsDeletionInfo()
```

**What it tests**:
- Activity log metadata includes deletion details
- JSON metadata contains: `DeletedAt`, `DeletedBy`, `IsDeleted`

#### Testing Approach

**Mock Setup**:
```csharp
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
}
```

**DbSet Mock**:
- Uses `Mock<DbSet<T>>` to simulate EF Core DbSet
- Tracks items added via `Add()` in a `List<T>`
- Allows assertions on what was added

---

### 4. EntityCreatedEventHandlerTests.cs

**Location**: `tests/Application.UnitTests/EventHandlers/EntityCreatedEventHandlerTests.cs`

**Purpose**: Tests automatic audit logging when entities are created.

#### Key Tests

##### Test: Log Project Creation
```csharp
[Test]
public async Task Handle_WhenProjectCreated_ShouldCreateActivityLog()
```

**What it tests**:
- Creating a project generates `ActivityType.ProjectCreated` log
- Log includes creator's name and email
- Description: "Created project 'Project Name'"

##### Test: Log Document Upload
```csharp
[Test]
public async Task Handle_WhenDocumentCreated_ShouldCreateActivityLog()
```

**What it tests**:
- Document creation → `ActivityType.DocumentUploaded`
- Links to parent project via `ProjectId`
- Shows document filename in log

##### Test: Log Organization Creation
```csharp
[Test]
public async Task Handle_WhenOrganizationCreated_ShouldCreateActivityLog()
```

**What it tests**:
- Organization creation → `ActivityType.WorkspaceCreated`
- First entry in a new organization's audit trail

##### Test: Creation Metadata
```csharp
[Test]
public async Task Handle_MetadataContainsCreationInfo()
```

**What it tests**:
- Metadata includes `CreatedAt` and `CreatedBy`
- Captures timestamp and user ID in JSON format

---

### 5. EntityUpdatedEventHandlerTests.cs

**Location**: `tests/Application.UnitTests/EventHandlers/EntityUpdatedEventHandlerTests.cs`

**Purpose**: Tests automatic audit logging when entities are updated.

#### Key Tests

##### Test: Log Significant Updates
```csharp
[Test]
public async Task Handle_WhenSignificantPropertyUpdated_ShouldCreateActivityLog()
```

**What it tests**:
- Only "significant" property changes are logged
- Audit fields (`LastModified`, `LastModifiedBy`) don't trigger logs
- Description lists changed properties: "Updated project (Name, Description)"

**Significant vs Audit-Only**:
```csharp
// These trigger logs:
var modifiedProperties = new[] { "Name", "Description", "Status" };

// These DON'T trigger logs:
var auditOnlyProperties = new[] { "LastModified", "LastModifiedBy" };
```

##### Test: Filter Audit-Only Changes
```csharp
[Test]
public async Task Handle_WhenOnlyAuditFieldsUpdated_ShouldNotCreateLog()
```

**What it tests**:
- If only `LastModified` and `LastModifiedBy` changed, no log is created
- Prevents infinite loops and noise in audit trail

##### Test: Multiple Properties Changed
```csharp
[Test]
public async Task Handle_WhenMultiplePropertiesUpdated_MetadataContainsAll()
```

**What it tests**:
- All changed properties are included in metadata
- Metadata JSON: `{ "ModifiedProperties": ["Name", "Description", "Status"] }`

##### Test: Too Many Properties Changed
```csharp
[Test]
public async Task Handle_WhenTooManyPropertiesChanged_DescriptionDoesNotListAll()
```

**What it tests**:
- If more than 3 properties changed, description is simplified
- Instead of "Name, Description, Status, TargetDate", shows "Updated project"
- Keeps descriptions concise

##### Test: Document Update Links to Project
```csharp
[Test]
public async Task Handle_WhenDocumentUpdated_ShouldSetCorrectProjectId()
```

**What it tests**:
- Document updates link to parent project
- Allows filtering project activity by documents

##### Test: Finding Status Changes
```csharp
[Test]
public async Task Handle_WhenFindingStatusChanged_ShouldLog()
```

**What it tests**:
- Compliance finding updates are logged
- Status transitions are tracked (Draft → InProgress → Resolved)

---

## Key Testing Concepts

### 1. Mocking with Moq

**Purpose**: Isolate the code under test by replacing dependencies with controllable fakes.

```csharp
var _mockCurrentUser = new Mock<IUser>();
_mockCurrentUser.Setup(x => x.Id).Returns("test-user-123");
```

**Benefits**:
- Tests run fast (no real database)
- Full control over dependency behavior
- Can simulate error conditions

### 2. In-Memory Database

**Purpose**: Test EF Core behavior (interceptors, query filters) with a real DbContext.

```csharp
var options = new DbContextOptionsBuilder<TestDbContext>()
    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
    .AddInterceptors(_interceptor)
    .Options;
```

**When to Use**:
- Testing interceptors (SoftDeleteInterceptor)
- Testing query filters
- Testing EF Core configuration

**When NOT to Use**:
- Testing business logic (use mocks instead)
- Integration tests (use real database)

### 3. NUnit Assertions

**Constraint Model**:
```csharp
Assert.That(actual, Is.EqualTo(expected));
Assert.That(collection, Has.Count.EqualTo(1));
Assert.That(collection, Is.Empty);
Assert.That(str, Does.Contain("text"));
```

### 4. Async Testing

All tests are async because they interact with EF Core:

```csharp
[Test]
public async Task TestName()
{
    await context.SaveChangesAsync();
    var result = await context.Projects.ToListAsync();
}
```

### 5. Test Isolation

Each test:
- Has its own `[SetUp]` to initialize fresh mocks
- Uses unique database name for in-memory tests
- Doesn't depend on other tests' state

---

## Test Coverage Summary

### Infrastructure Layer (18 tests)

**SoftDeleteInterceptor** (5 tests):
- ✅ Converts hard delete to soft delete
- ✅ Sets deletion metadata (IsDeleted, DeletedAt, DeletedBy)
- ✅ Raises EntityDeletedEvent
- ✅ Handles multiple entity deletion
- ✅ Doesn't affect normal updates

**Query Filters** (13 tests):
- ✅ Hides deleted entities by default
- ✅ IgnoreQueryFilters shows deleted entities
- ✅ Count/Any/FirstOrDefault respect filters
- ✅ Can query only deleted entities
- ✅ Can restore deleted entities

### Application Layer (25 tests)

**EntityDeletedEventHandler** (7 tests):
- ✅ Creates activity log for Project/Document/Organization deletion
- ✅ Captures deletion metadata
- ✅ Respects tenant context
- ✅ Handles exceptions gracefully

**EntityCreatedEventHandler** (8 tests):
- ✅ Logs entity creation
- ✅ Captures creator information
- ✅ Different activity types per entity

**EntityUpdatedEventHandler** (10 tests):
- ✅ Logs significant property changes
- ✅ Filters audit-only field changes
- ✅ Tracks modified properties in metadata
- ✅ Links to parent entities (Project)

---

## Running the Tests

### Run All Tests
```bash
dotnet test
```

### Run Specific Test File
```bash
dotnet test --filter "FullyQualifiedName~SoftDeleteInterceptorTests"
```

### Run Single Test
```bash
dotnet test --filter "FullyQualifiedName~SavingChangesAsync_WhenEntityDeleted_ShouldConvertToSoftDelete"
```

### Run with Detailed Output
```bash
dotnet test --logger "console;verbosity=detailed"
```

---

## Test Results

All 116 tests passing:
- **Domain.UnitTests**: 43 tests ✅
- **Application.UnitTests**: 46 tests ✅ (includes 25 event handler tests)
- **Infrastructure.UnitTests**: 18 tests ✅ (includes 18 soft delete tests)
- **Web.IntegrationTests**: 9 tests ✅

---

## Future Test Enhancements

### Potential Additional Tests

1. **Concurrent Deletion**
   - Test multiple users deleting the same entity
   - Verify optimistic concurrency handling

2. **Cascade Soft Delete**
   - Test parent entity deletion soft-deletes children
   - Verify referential integrity with soft delete

3. **Performance Tests**
   - Measure query filter impact on large datasets
   - Benchmark soft delete vs hard delete

4. **Restore Functionality**
   - Test entity restoration with related entities
   - Verify audit log on restore

5. **Permanent Delete**
   - Test admin feature to permanently remove soft-deleted entities
   - Verify cascade deletion of related audit logs

---

## Troubleshooting

### Tests Failing After Schema Changes

**Symptom**: Tests fail after adding/removing properties from entities.

**Solution**: Update test data setup to include new required properties.

### In-Memory Database Limitations

**Known Issues**:
- Some SQL functions not supported (LIKE, date functions)
- Case sensitivity differs from real database
- Some constraints not enforced

**Solution**: Use integration tests with real database for complex queries.

### Mock Setup Errors

**Symptom**: `NullReferenceException` or "Setup not configured".

**Solution**: Ensure all dependencies used by the handler are mocked:
```csharp
_mockCurrentUser.Setup(x => x.Id).Returns("test-user-123");
_mockTenantService.Setup(x => x.GetCurrentTenantId()).Returns(orgId);
_mockIdentityService.Setup(x => x.GetUserByIdAsync(It.IsAny<string>()))
    .ReturnsAsync(new ApplicationUserDto { ... });
```

---

## Best Practices

### ✅ DO

- Write one assertion per test concept
- Use descriptive test names following pattern: `Method_Scenario_ExpectedResult`
- Test both success and failure paths
- Mock external dependencies
- Use `[SetUp]` for common initialization
- Clean up resources in `[TearDown]` if needed

### ❌ DON'T

- Test multiple unrelated things in one test
- Depend on test execution order
- Use real external services (databases, APIs) in unit tests
- Share state between tests
- Catch exceptions unless testing exception handling

---

## Related Documentation

- [Soft Delete Implementation Guide](./SOFT_DELETE_IMPLEMENTATION.md)
- [Audit Logging Design](./AUDIT_LOGGING.md)
- [Testing Strategy](./TESTING_STRATEGY.md)

---

**Last Updated**: November 13, 2024
**Test Coverage**: 116 tests covering soft delete, query filters, and audit logging

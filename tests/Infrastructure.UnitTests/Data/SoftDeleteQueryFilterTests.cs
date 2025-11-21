using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Entities;
using IgnaCheck.Infrastructure.Data.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Infrastructure.UnitTests.Data;

/// <summary>
/// Tests for soft delete query filters to ensure deleted items are properly hidden.
/// </summary>
public class SoftDeleteQueryFilterTests
{
    private Mock<IUser> _mockUser = null!;
    private Mock<ILogger<SoftDeleteInterceptor>> _mockLogger = null!;
    private TimeProvider _timeProvider = null!;
    private SoftDeleteInterceptor _interceptor = null!;

    [SetUp]
    public void Setup()
    {
        _mockUser = new Mock<IUser>();
        _mockUser.Setup(x => x.Id).Returns("test-user");
        _mockLogger = new Mock<ILogger<SoftDeleteInterceptor>>();
        _timeProvider = TimeProvider.System;
        _interceptor = new SoftDeleteInterceptor(_mockUser.Object, _timeProvider, _mockLogger.Object);
    }

    [Test]
    public async Task Query_WithoutIgnoreQueryFilters_ShouldNotReturnDeletedEntities()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<Interceptors.TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(_interceptor)
            .Options;

        await using var context = new Interceptors.TestDbContext(options);

        var orgId = Guid.NewGuid();
        var activeProject = new Project { Id = Guid.NewGuid(), Name = "Active Project", OrganizationId = orgId };
        var deletedProject = new Project { Id = Guid.NewGuid(), Name = "Deleted Project", OrganizationId = orgId };

        context.Projects.AddRange(activeProject, deletedProject);
        await context.SaveChangesAsync();

        // Delete one project
        context.Projects.Remove(deletedProject);
        await context.SaveChangesAsync();

        // Act - Query without IgnoreQueryFilters
        var projects = await context.Projects.ToListAsync();

        // Assert
        Assert.That(projects, Has.Count.EqualTo(1));
        Assert.That(projects.First().Name, Is.EqualTo("Active Project"));
        Assert.That(projects.Any(p => p.Name == "Deleted Project"), Is.False);
    }

    [Test]
    public async Task Query_WithIgnoreQueryFilters_ShouldReturnDeletedEntities()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<Interceptors.TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(_interceptor)
            .Options;

        await using var context = new Interceptors.TestDbContext(options);

        var orgId = Guid.NewGuid();
        var activeProject = new Project { Id = Guid.NewGuid(), Name = "Active Project", OrganizationId = orgId };
        var deletedProject = new Project { Id = Guid.NewGuid(), Name = "Deleted Project", OrganizationId = orgId };

        context.Projects.AddRange(activeProject, deletedProject);
        await context.SaveChangesAsync();

        // Delete one project
        context.Projects.Remove(deletedProject);
        await context.SaveChangesAsync();

        // Act - Query WITH IgnoreQueryFilters
        var allProjects = await context.Projects
            .IgnoreQueryFilters()
            .ToListAsync();

        // Assert
        Assert.That(allProjects.Count, Is.EqualTo(2));
        Assert.That(allProjects.Any(p => p.Name == "Active Project" && !p.IsDeleted), Is.True);
        Assert.That(allProjects.Any(p => p.Name == "Deleted Project" && p.IsDeleted), Is.True);
    }

    [Test]
    public async Task Query_OnlyDeletedEntities_ShouldReturnOnlyDeleted()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<Interceptors.TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(_interceptor)
            .Options;

        await using var context = new Interceptors.TestDbContext(options);

        var orgId = Guid.NewGuid();
        var activeProject = new Project { Id = Guid.NewGuid(), Name = "Active Project", OrganizationId = orgId };
        var deletedProject1 = new Project { Id = Guid.NewGuid(), Name = "Deleted Project 1", OrganizationId = orgId };
        var deletedProject2 = new Project { Id = Guid.NewGuid(), Name = "Deleted Project 2", OrganizationId = orgId };

        context.Projects.AddRange(activeProject, deletedProject1, deletedProject2);
        await context.SaveChangesAsync();

        // Delete two projects
        context.Projects.RemoveRange(deletedProject1, deletedProject2);
        await context.SaveChangesAsync();

        // Act - Query only deleted entities
        var deletedProjects = await context.Projects
            .IgnoreQueryFilters()
            .Where(p => p.IsDeleted)
            .ToListAsync();

        // Assert
        Assert.That(deletedProjects.Count, Is.EqualTo(2));
        Assert.That(deletedProjects.All(p => p.IsDeleted), Is.True);
        Assert.That(deletedProjects.Any(p => p.Name == "Active Project"), Is.False);
    }

    [Test]
    public async Task Count_WithoutIgnoreQueryFilters_ShouldExcludeDeletedEntities()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<Interceptors.TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(_interceptor)
            .Options;

        await using var context = new Interceptors.TestDbContext(options);

        var orgId = Guid.NewGuid();
        for (int i = 0; i < 5; i++)
        {
            context.Projects.Add(new Project
            {
                Id = Guid.NewGuid(),
                Name = $"Project {i}",
                OrganizationId = orgId
            });
        }
        await context.SaveChangesAsync();

        // Delete 2 projects
        var projectsToDelete = await context.Projects.Take(2).ToListAsync();
        context.Projects.RemoveRange(projectsToDelete);
        await context.SaveChangesAsync();

        // Act
        var activeCount = await context.Projects.CountAsync();
        var totalCount = await context.Projects.IgnoreQueryFilters().CountAsync();

        // Assert
        Assert.That(activeCount, Is.EqualTo(3));
        Assert.That(totalCount, Is.EqualTo(5));
    }

    [Test]
    public async Task FirstOrDefault_OnDeletedEntity_ShouldReturnNull()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<Interceptors.TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(_interceptor)
            .Options;

        await using var context = new Interceptors.TestDbContext(options);

        var projectId = Guid.NewGuid();
        var project = new Project
        {
            Id = projectId,
            Name = "Test Project",
            OrganizationId = Guid.NewGuid()
        };

        context.Projects.Add(project);
        await context.SaveChangesAsync();

        // Delete the project
        context.Projects.Remove(project);
        await context.SaveChangesAsync();

        // Act - Try to find by ID without IgnoreQueryFilters
        var foundProject = await context.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId);

        // Assert
        Assert.That(foundProject, Is.Null);
    }

    [Test]
    public async Task FirstOrDefault_OnDeletedEntity_WithIgnoreQueryFilters_ShouldReturnEntity()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<Interceptors.TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(_interceptor)
            .Options;

        await using var context = new Interceptors.TestDbContext(options);

        var projectId = Guid.NewGuid();
        var project = new Project
        {
            Id = projectId,
            Name = "Test Project",
            OrganizationId = Guid.NewGuid()
        };

        context.Projects.Add(project);
        await context.SaveChangesAsync();

        // Delete the project
        context.Projects.Remove(project);
        await context.SaveChangesAsync();

        // Act - Try to find by ID WITH IgnoreQueryFilters
        var foundProject = await context.Projects
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == projectId);

        // Assert
        Assert.That(foundProject, Is.Not.Null);
        Assert.That(foundProject.IsDeleted, Is.True);
        Assert.That(foundProject.Name, Is.EqualTo("Test Project"));
    }

    [Test]
    public async Task RestoreDeletedEntity_ShouldMakeItVisibleAgain()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<Interceptors.TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(_interceptor)
            .Options;

        await using var context = new Interceptors.TestDbContext(options);

        var projectId = Guid.NewGuid();
        var project = new Project
        {
            Id = projectId,
            Name = "Test Project",
            OrganizationId = Guid.NewGuid()
        };

        context.Projects.Add(project);
        await context.SaveChangesAsync();

        // Delete the project
        context.Projects.Remove(project);
        await context.SaveChangesAsync();

        // Act - Restore the project
        var deletedProject = await context.Projects
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == projectId);

        Assert.That(deletedProject, Is.Not.Null);
        deletedProject.IsDeleted = false;
        deletedProject.DeletedAt = null;
        deletedProject.DeletedBy = null;
        await context.SaveChangesAsync();

        // Assert - Should be visible in normal queries now
        var restoredProject = await context.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId);

        Assert.That(restoredProject, Is.Not.Null);
        Assert.That(restoredProject.IsDeleted, Is.False);
        Assert.That(restoredProject.Name, Is.EqualTo("Test Project"));
    }

    [Test]
    public async Task Any_WithoutIgnoreQueryFilters_ShouldNotFindDeletedEntities()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<Interceptors.TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(_interceptor)
            .Options;

        await using var context = new Interceptors.TestDbContext(options);

        var projectId = Guid.NewGuid();
        var project = new Project
        {
            Id = projectId,
            Name = "Test Project",
            OrganizationId = Guid.NewGuid()
        };

        context.Projects.Add(project);
        await context.SaveChangesAsync();

        // Delete the project
        context.Projects.Remove(project);
        await context.SaveChangesAsync();

        // Act
        var exists = await context.Projects.AnyAsync(p => p.Id == projectId);

        // Assert
        Assert.That(exists, Is.False);
    }
}

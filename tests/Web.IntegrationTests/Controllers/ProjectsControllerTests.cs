using System.Net;
using System.Net.Http.Json;
using IgnaCheck.Application.Projects.Commands.CreateProject;
using IgnaCheck.Domain.Entities;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Web.IntegrationTests.Controllers;

[TestFixture]
public class ProjectsControllerTests : BaseIntegrationTest
{
    private const string ApiUrl = "/api/projects";

    [Test]
    public async Task CreateProject_Should_ReturnSuccess_When_ValidRequest()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var organization = new Organization
        {
            Id = organizationId,
            Name = "Test Organization",
            IsActive = true
        };
        await AddAsync(organization);
        SetCurrentOrganization(organizationId);

        var client = CreateClient();

        var command = new CreateProjectCommand
        {
            Name = "SOC 2 Type II Audit",
            Description = "Annual compliance audit for SOC 2 Type II",
            TargetDate = DateTime.UtcNow.AddMonths(3)
        };

        // Act
        var response = await client.PostAsJsonAsync(ApiUrl, command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Result<CreateProjectResponse>>();
        result.ShouldNotBeNull();
        result.Succeeded.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Name.ShouldBe("SOC 2 Type II Audit");
        result.Data.Description.ShouldBe("Annual compliance audit for SOC 2 Type II");
        result.Data.Status.ShouldBe(ProjectStatus.Draft);
        result.Data.Id.ShouldNotBe(Guid.Empty);

        // Verify project was actually created in database
        var project = await FindAsync<Project>(result.Data.Id);
        project.ShouldNotBeNull();
        project.Name.ShouldBe("SOC 2 Type II Audit");
    }

    [Test]
    public async Task CreateProject_Should_ReturnBadRequest_When_NameIsEmpty()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var organization = new Organization
        {
            Id = organizationId,
            Name = "Test Organization",
            IsActive = true
        };
        await AddAsync(organization);
        SetCurrentOrganization(organizationId);

        var client = CreateClient();

        var command = new CreateProjectCommand
        {
            Name = "",
            Description = "Test description"
        };

        // Act
        var response = await client.PostAsJsonAsync(ApiUrl, command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task CreateProject_Should_ReturnBadRequest_When_NameTooShort()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var organization = new Organization
        {
            Id = organizationId,
            Name = "Test Organization",
            IsActive = true
        };
        await AddAsync(organization);
        SetCurrentOrganization(organizationId);

        var client = CreateClient();

        var command = new CreateProjectCommand
        {
            Name = "AB",
            Description = "Test description"
        };

        // Act
        var response = await client.PostAsJsonAsync(ApiUrl, command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task GetProjects_Should_ReturnEmptyList_When_NoProjects()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var organization = new Organization
        {
            Id = organizationId,
            Name = "Test Organization",
            IsActive = true
        };
        await AddAsync(organization);
        SetCurrentOrganization(organizationId);

        var client = CreateClient();

        // Act
        var response = await client.GetAsync(ApiUrl);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Result<List<ProjectDto>>>();
        result.ShouldNotBeNull();
        result.Succeeded.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Count.ShouldBe(0);
    }

    [Test]
    public async Task GetProjects_Should_ReturnProjects_When_ProjectsExist()
    {
        // Arrange
        // Create test data
        var organizationId = Guid.NewGuid();
        var organization = new Organization
        {
            Id = organizationId,
            Name = "Test Organization",
            IsActive = true
        };
        await AddAsync(organization);
        SetCurrentOrganization(organizationId);

        var client = CreateClient();

        var project1 = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Project 1",
            Description = "Description 1",
            Status = ProjectStatus.Active,
            OrganizationId = organizationId
        };
        var project2 = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Project 2",
            Description = "Description 2",
            Status = ProjectStatus.Draft,
            OrganizationId = organizationId
        };
        await AddAsync(project1);
        await AddAsync(project2);

        // Act
        var response = await client.GetAsync(ApiUrl);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Result<List<ProjectDto>>>();
        result.ShouldNotBeNull();
        result.Succeeded.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Count.ShouldBeGreaterThanOrEqualTo(2);
    }

    [Test]
    public async Task GetProjectDetails_Should_ReturnProject_When_ProjectExists()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var organization = new Organization
        {
            Id = organizationId,
            Name = "Test Organization",
            IsActive = true
        };
        await AddAsync(organization);
        SetCurrentOrganization(organizationId);

        var client = CreateClient();

        var projectId = Guid.NewGuid();
        var project = new Project
        {
            Id = projectId,
            Name = "Test Project",
            Description = "Test Description",
            Status = ProjectStatus.Active,
            OrganizationId = organizationId
        };
        await AddAsync(project);

        // Act
        var response = await client.GetAsync($"{ApiUrl}/{projectId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Result<ProjectDetailsDto>>();
        result.ShouldNotBeNull();
        result.Succeeded.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Id.ShouldBe(projectId);
        result.Data.Name.ShouldBe("Test Project");
    }

    [Test]
    public async Task GetProjectDetails_Should_ReturnNotFound_When_ProjectDoesNotExist()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var organization = new Organization
        {
            Id = organizationId,
            Name = "Test Organization",
            IsActive = true
        };
        await AddAsync(organization);
        SetCurrentOrganization(organizationId);

        var client = CreateClient();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"{ApiUrl}/{nonExistentId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetProjects_Should_FilterByStatus_When_StatusProvided()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var organization = new Organization
        {
            Id = organizationId,
            Name = "Test Organization",
            IsActive = true
        };
        await AddAsync(organization);
        SetCurrentOrganization(organizationId);

        var client = CreateClient();

        var activeProject = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Active Project",
            Status = ProjectStatus.Active,
            OrganizationId = organizationId
        };
        var draftProject = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Draft Project",
            Status = ProjectStatus.Draft,
            OrganizationId = organizationId
        };
        await AddAsync(activeProject);
        await AddAsync(draftProject);

        // Act
        var response = await client.GetAsync($"{ApiUrl}?status={ProjectStatus.Active}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Result<List<ProjectDto>>>();
        result.ShouldNotBeNull();
        result.Succeeded.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.All(p => p.Status == ProjectStatus.Active).ShouldBeTrue();
    }

    [Test]
    public async Task GetProjects_Should_SearchByTerm_When_SearchTermProvided()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var organization = new Organization
        {
            Id = organizationId,
            Name = "Test Organization",
            IsActive = true
        };
        await AddAsync(organization);
        SetCurrentOrganization(organizationId);

        var client = CreateClient();

        var project1 = new Project
        {
            Id = Guid.NewGuid(),
            Name = "SOC 2 Audit",
            Description = "Security audit",
            Status = ProjectStatus.Active,
            OrganizationId = organizationId
        };
        var project2 = new Project
        {
            Id = Guid.NewGuid(),
            Name = "ISO 27001",
            Description = "Compliance project",
            Status = ProjectStatus.Active,
            OrganizationId = organizationId
        };
        await AddAsync(project1);
        await AddAsync(project2);

        // Act
        var response = await client.GetAsync($"{ApiUrl}?searchTerm=SOC");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Result<List<ProjectDto>>>();
        result.ShouldNotBeNull();
        result.Succeeded.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Any(p => p.Name.Contains("SOC")).ShouldBeTrue();
    }
}

// DTOs for testing
public class Result<T>
{
    public bool Succeeded { get; set; }
    public T Data { get; set; } = default!;
    public string[] Errors { get; set; } = Array.Empty<string>();
}

public class ProjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectStatus Status { get; set; }
    public DateTime? TargetDate { get; set; }
    public int MemberCount { get; set; }
    public int FindingCount { get; set; }
}

public class ProjectDetailsDto : ProjectDto
{
    public List<ProjectFrameworkDto> Frameworks { get; set; } = new();
    public List<ProjectMemberDto> Members { get; set; } = new();
}

public class ProjectFrameworkDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ProjectMemberDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public ProjectRole Role { get; set; }
}

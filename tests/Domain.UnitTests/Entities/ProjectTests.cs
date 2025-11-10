using IgnaCheck.Domain.Entities;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Domain.UnitTests.Entities;

[TestFixture]
public class ProjectTests
{
    [Test]
    public void Project_Should_Initialize_With_Default_Values()
    {
        // Arrange & Act
        var project = new Project();

        // Assert
        project.Name.ShouldBe(string.Empty);
        project.Status.ShouldBe(ProjectStatus.Draft);
        project.Description.ShouldBeNull();
        project.TargetDate.ShouldBeNull();
        project.ProjectFrameworks.ShouldNotBeNull();
        project.ProjectFrameworks.ShouldBeEmpty();
        project.Documents.ShouldNotBeNull();
        project.Documents.ShouldBeEmpty();
        project.Findings.ShouldNotBeNull();
        project.Findings.ShouldBeEmpty();
        project.RemediationTasks.ShouldNotBeNull();
        project.RemediationTasks.ShouldBeEmpty();
        project.ProjectMembers.ShouldNotBeNull();
        project.ProjectMembers.ShouldBeEmpty();
    }

    [Test]
    public void Project_Should_Set_Properties_Correctly()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var targetDate = DateTime.UtcNow.AddMonths(3);

        // Act
        var project = new Project
        {
            Name = "SOC 2 Type II Audit",
            Description = "Annual SOC 2 Type II compliance audit",
            Status = ProjectStatus.Active,
            TargetDate = targetDate,
            OrganizationId = organizationId
        };

        // Assert
        project.Name.ShouldBe("SOC 2 Type II Audit");
        project.Description.ShouldBe("Annual SOC 2 Type II compliance audit");
        project.Status.ShouldBe(ProjectStatus.Active);
        project.TargetDate.ShouldBe(targetDate);
        project.OrganizationId.ShouldBe(organizationId);
    }

    [Test]
    public void Project_Should_Allow_Adding_ProjectFrameworks()
    {
        // Arrange
        var project = new Project { Name = "Test Project" };
        var frameworkId = Guid.NewGuid();
        var projectFramework = new ProjectFramework
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            FrameworkId = frameworkId
        };

        // Act
        project.ProjectFrameworks.Add(projectFramework);

        // Assert
        project.ProjectFrameworks.Count.ShouldBe(1);
        project.ProjectFrameworks.First().FrameworkId.ShouldBe(frameworkId);
    }

    [Test]
    public void Project_Should_Allow_Adding_Documents()
    {
        // Arrange
        var project = new Project { Name = "Test Project" };
        var document = new Document
        {
            Id = Guid.NewGuid(),
            FileName = "policy.pdf",
            ProjectId = project.Id
        };

        // Act
        project.Documents.Add(document);

        // Assert
        project.Documents.Count.ShouldBe(1);
        project.Documents.First().FileName.ShouldBe("policy.pdf");
    }

    [Test]
    public void Project_Should_Allow_Adding_Findings()
    {
        // Arrange
        var project = new Project { Name = "Test Project" };
        var finding = new ComplianceFinding
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            Title = "Missing encryption policy"
        };

        // Act
        project.Findings.Add(finding);

        // Assert
        project.Findings.Count.ShouldBe(1);
        project.Findings.First().Title.ShouldBe("Missing encryption policy");
    }

    [Test]
    public void Project_Should_Allow_Adding_ProjectMembers()
    {
        // Arrange
        var project = new Project { Name = "Test Project" };
        var userId = Guid.NewGuid().ToString();
        var projectMember = new ProjectMember
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            UserId = userId,
            Role = ProjectRole.Contributor
        };

        // Act
        project.ProjectMembers.Add(projectMember);

        // Assert
        project.ProjectMembers.Count.ShouldBe(1);
        project.ProjectMembers.First().UserId.ShouldBe(userId);
        project.ProjectMembers.First().Role.ShouldBe(ProjectRole.Contributor);
    }

    [Test]
    [TestCase(ProjectStatus.Draft)]
    [TestCase(ProjectStatus.Active)]
    [TestCase(ProjectStatus.Completed)]
    [TestCase(ProjectStatus.Archived)]
    [TestCase(ProjectStatus.OnHold)]
    public void Project_Should_Support_All_Status_Values(ProjectStatus status)
    {
        // Arrange & Act
        var project = new Project { Status = status };

        // Assert
        project.Status.ShouldBe(status);
    }
}

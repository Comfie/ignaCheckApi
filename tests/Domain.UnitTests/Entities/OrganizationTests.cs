using IgnaCheck.Domain.Entities;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Domain.UnitTests.Entities;

[TestFixture]
public class OrganizationTests
{
    [Test]
    public void Organization_Should_Initialize_With_Default_Values()
    {
        // Arrange & Act
        var organization = new Organization();

        // Assert
        organization.Name.ShouldBe(string.Empty);
        organization.IsActive.ShouldBe(true);
        organization.SubscriptionTier.ShouldBe(SubscriptionTier.Free);
        organization.Projects.ShouldNotBeNull();
        organization.Projects.ShouldBeEmpty();
        organization.Members.ShouldNotBeNull();
        organization.Members.ShouldBeEmpty();
    }

    [Test]
    public void Organization_Should_Set_Properties_Correctly()
    {
        // Arrange
        var createdDate = DateTime.UtcNow;

        // Act
        var organization = new Organization
        {
            Name = "Acme Corporation",
            IsActive = true,
            SubscriptionTier = SubscriptionTier.Professional,
            MaxProjects = 50,
            MaxMembers = 25,
            SubscriptionStartDate = createdDate,
            SubscriptionEndDate = createdDate.AddYears(1)
        };

        // Assert
        organization.Name.ShouldBe("Acme Corporation");
        organization.IsActive.ShouldBe(true);
        organization.SubscriptionTier.ShouldBe(SubscriptionTier.Professional);
        organization.MaxProjects.ShouldBe(50);
        organization.MaxMembers.ShouldBe(25);
        organization.SubscriptionStartDate.ShouldBe(createdDate);
        organization.SubscriptionEndDate.ShouldBe(createdDate.AddYears(1));
    }

    [Test]
    [TestCase(SubscriptionTier.Free)]
    [TestCase(SubscriptionTier.Starter)]
    [TestCase(SubscriptionTier.Professional)]
    [TestCase(SubscriptionTier.Enterprise)]
    public void Organization_Should_Support_All_SubscriptionTier_Values(SubscriptionTier tier)
    {
        // Arrange & Act
        var organization = new Organization { SubscriptionTier = tier };

        // Assert
        organization.SubscriptionTier.ShouldBe(tier);
    }

    [Test]
    public void Organization_Should_Allow_Adding_Projects()
    {
        // Arrange
        var organization = new Organization { Name = "Test Org" };
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            OrganizationId = organization.Id
        };

        // Act
        organization.Projects.Add(project);

        // Assert
        organization.Projects.Count.ShouldBe(1);
        organization.Projects.First().Name.ShouldBe("Test Project");
    }

    [Test]
    public void Organization_Should_Allow_Adding_Members()
    {
        // Arrange
        var organization = new Organization { Name = "Test Org" };
        var userId = Guid.NewGuid().ToString();
        var member = new OrganizationMember
        {
            Id = Guid.NewGuid(),
            OrganizationId = organization.Id,
            UserId = userId,
            Role = OrganizationRole.Member
        };

        // Act
        organization.Members.Add(member);

        // Assert
        organization.Members.Count.ShouldBe(1);
        organization.Members.First().UserId.ShouldBe(userId);
        organization.Members.First().Role.ShouldBe(OrganizationRole.Member);
    }

    [Test]
    public void Organization_Should_Track_Subscription_Dates()
    {
        // Arrange
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddMonths(12);

        // Act
        var organization = new Organization
        {
            Name = "Test Org",
            SubscriptionStartDate = startDate,
            SubscriptionEndDate = endDate
        };

        // Assert
        organization.SubscriptionStartDate.ShouldBe(startDate);
        organization.SubscriptionEndDate.ShouldBe(endDate);
    }

    [Test]
    public void Organization_Should_Support_Resource_Limits()
    {
        // Arrange & Act
        var organization = new Organization
        {
            Name = "Limited Org",
            MaxProjects = 10,
            MaxMembers = 5,
            MaxStorage = 5368709120 // 5 GB in bytes
        };

        // Assert
        organization.MaxProjects.ShouldBe(10);
        organization.MaxMembers.ShouldBe(5);
        organization.MaxStorage.ShouldBe(5368709120);
    }

    [Test]
    public void Organization_Can_Be_Deactivated()
    {
        // Arrange
        var organization = new Organization
        {
            Name = "Test Org",
            IsActive = true
        };

        // Act
        organization.IsActive = false;

        // Assert
        organization.IsActive.ShouldBe(false);
    }
}

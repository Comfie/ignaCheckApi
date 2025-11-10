using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Infrastructure.Identity;
using IgnaCheck.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IgnaCheck.Infrastructure.UnitTests.Services;

[TestFixture]
public class TenantServiceTests
{
    private Mock<IUser> _mockCurrentUser;
    private Mock<IApplicationDbContext> _mockDbContext;
    private Mock<DbSet<Organization>> _mockOrganizationsDbSet;
    private List<Organization> _organizations;
    private TenantService _tenantService;

    [SetUp]
    public void SetUp()
    {
        _mockCurrentUser = new Mock<IUser>();
        _mockDbContext = new Mock<IApplicationDbContext>();
        _organizations = new List<Organization>();

        _mockOrganizationsDbSet = CreateMockDbSet(_organizations);
        _mockDbContext.Setup(x => x.Organizations).Returns(_mockOrganizationsDbSet.Object);

        _tenantService = new TenantService(_mockCurrentUser.Object, _mockDbContext.Object);
    }

    [Test]
    public void GetCurrentTenantId_Should_ReturnOrganizationId_When_UserHasOrganization()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        _mockCurrentUser.Setup(x => x.OrganizationId).Returns(organizationId);

        // Act
        var result = _tenantService.GetCurrentTenantId();

        // Assert
        result.ShouldBe(organizationId);
    }

    [Test]
    public void GetCurrentTenantId_Should_ReturnNull_When_UserHasNoOrganization()
    {
        // Arrange
        _mockCurrentUser.Setup(x => x.OrganizationId).Returns((Guid?)null);

        // Act
        var result = _tenantService.GetCurrentTenantId();

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task GetCurrentTenantAsync_Should_ReturnOrganization_When_TenantIdExists()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var organization = new Organization
        {
            Id = organizationId,
            Name = "Test Org",
            IsActive = true
        };
        _organizations.Add(organization);

        _mockCurrentUser.Setup(x => x.OrganizationId).Returns(organizationId);

        // Act
        var result = await _tenantService.GetCurrentTenantAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(organizationId);
        result.Name.ShouldBe("Test Org");
    }

    [Test]
    public async Task GetCurrentTenantAsync_Should_ReturnNull_When_TenantIdIsNull()
    {
        // Arrange
        _mockCurrentUser.Setup(x => x.OrganizationId).Returns((Guid?)null);

        // Act
        var result = await _tenantService.GetCurrentTenantAsync();

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public void SetCurrentTenantId_Should_OverrideTenantId()
    {
        // Arrange
        var overrideTenantId = Guid.NewGuid();

        // Act
        _tenantService.SetCurrentTenantId(overrideTenantId);
        var result = _tenantService.GetCurrentTenantId();

        // Assert
        result.ShouldBe(overrideTenantId);
    }

    private Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var mockSet = new Mock<DbSet<T>>();

        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

        return mockSet;
    }
}

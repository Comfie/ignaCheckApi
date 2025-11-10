using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace IgnaCheck.Infrastructure.UnitTests.Services;

[TestFixture]
public class TenantServiceTests
{
    private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private Mock<HttpContext> _mockHttpContext;
    private TenantService _tenantService;

    [SetUp]
    public void SetUp()
    {
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockHttpContext = new Mock<HttpContext>();
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);

        _tenantService = new TenantService(_mockHttpContextAccessor.Object);
    }

    [Test]
    public void GetCurrentTenantId_Should_ReturnOrganizationId_When_ClaimExists()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new Claim("organization_id", organizationId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _mockHttpContext.Setup(x => x.User).Returns(claimsPrincipal);

        // Act
        var result = _tenantService.GetCurrentTenantId();

        // Assert
        result.ShouldBe(organizationId);
    }

    [Test]
    public void GetCurrentTenantId_Should_ReturnNull_When_ClaimDoesNotExist()
    {
        // Arrange
        var claims = new List<Claim>();
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _mockHttpContext.Setup(x => x.User).Returns(claimsPrincipal);

        // Act
        var result = _tenantService.GetCurrentTenantId();

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public void GetCurrentTenantId_Should_ReturnNull_When_HttpContextIsNull()
    {
        // Arrange
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext)null!);
        _tenantService = new TenantService(_mockHttpContextAccessor.Object);

        // Act
        var result = _tenantService.GetCurrentTenantId();

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public void GetCurrentTenantId_Should_ReturnNull_When_ClaimValueIsInvalid()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim("organization_id", "invalid-guid")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _mockHttpContext.Setup(x => x.User).Returns(claimsPrincipal);

        // Act
        var result = _tenantService.GetCurrentTenantId();

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public void GetCurrentOrganizationId_Should_ReturnSameAsGetCurrentTenantId()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new Claim("organization_id", organizationId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _mockHttpContext.Setup(x => x.User).Returns(claimsPrincipal);

        // Act
        var tenantId = _tenantService.GetCurrentTenantId();
        var orgId = _tenantService.GetCurrentOrganizationId();

        // Assert
        tenantId.ShouldBe(orgId);
    }
}

namespace IgnaCheck.Application.UnitTests.Common;

/// <summary>
/// Base class for unit tests providing common test setup
/// </summary>
public abstract class TestBase
{
    protected Mock<IApplicationDbContext> MockDbContext { get; private set; } = null!;
    protected Mock<ICurrentUserService> MockCurrentUser { get; private set; } = null!;
    protected Mock<ITenantService> MockTenantService { get; private set; } = null!;

    [SetUp]
    public void BaseSetUp()
    {
        MockDbContext = new Mock<IApplicationDbContext>();
        MockCurrentUser = new Mock<ICurrentUserService>();
        MockTenantService = new Mock<ITenantService>();

        // Set default values
        MockCurrentUser.Setup(x => x.UserId).Returns(Guid.NewGuid().ToString());
        MockTenantService.Setup(x => x.GetCurrentOrganizationId()).Returns(Guid.NewGuid());

        OnSetUp();
    }

    /// <summary>
    /// Override this method to add additional setup logic
    /// </summary>
    protected virtual void OnSetUp()
    {
    }
}

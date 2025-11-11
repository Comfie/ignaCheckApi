using IgnaCheck.Application.Common.Interfaces;

namespace IgnaCheck.Application.UnitTests.Common;

/// <summary>
/// Base class for unit tests providing common test setup
/// </summary>
public abstract class TestBase
{
    protected Mock<IApplicationDbContext> MockDbContext { get; private set; } = null!;
    protected Mock<IUser> MockCurrentUser { get; private set; } = null!;
    protected Mock<ITenantService> MockTenantService { get; private set; } = null!;

    [SetUp]
    public void BaseSetUp()
    {
        MockDbContext = new Mock<IApplicationDbContext>();
        MockCurrentUser = new Mock<IUser>();
        MockTenantService = new Mock<ITenantService>();

        // Set default values
        MockCurrentUser.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());
        MockTenantService.Setup(x => x.GetCurrentTenantId()).Returns(Guid.NewGuid());

        OnSetUp();
    }

    /// <summary>
    /// Override this method to add additional setup logic
    /// </summary>
    protected virtual void OnSetUp()
    {
    }
}

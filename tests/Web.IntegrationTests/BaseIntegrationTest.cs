using MediatR;

namespace IgnaCheck.Web.IntegrationTests;

/// <summary>
/// Base class for integration tests that provides database cleanup between tests
/// </summary>
public abstract class BaseIntegrationTest
{
    [SetUp]
    public async Task TestSetUp()
    {
        await Testing.ResetState();
        // Clear the test organization context
        TestUser.ClearOrganizationId();
    }

    protected static Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        return Testing.SendAsync(request);
    }

    protected static Task SendAsync(IRequest request)
    {
        return Testing.SendAsync(request);
    }

    protected static Task<TEntity?> FindAsync<TEntity>(params object[] keyValues)
        where TEntity : class
    {
        return Testing.FindAsync<TEntity>(keyValues);
    }

    protected static Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        return Testing.AddAsync(entity);
    }

    protected static Task<int> CountAsync<TEntity>() where TEntity : class
    {
        return Testing.CountAsync<TEntity>();
    }

    protected static HttpClient CreateClient()
    {
        return Testing.CreateClient();
    }

    protected static void SetCurrentOrganization(Guid organizationId)
    {
        TestUser.SetOrganizationId(organizationId);
    }
}

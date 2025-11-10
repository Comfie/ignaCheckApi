using IgnaCheck.Infrastructure.Data;
using IgnaCheck.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Respawn;
using Testcontainers.PostgreSql;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace IgnaCheck.Web.IntegrationTests;

[SetUpFixture]
public partial class Testing
{
    private static WebApplicationFactory<Program> _factory = null!;
    private static IServiceScopeFactory _scopeFactory = null!;
    private static PostgreSqlContainer _dbContainer = null!;
    private static Respawner _respawner = null!;
    private static string _connectionString = null!;

    [OneTimeSetUp]
    public async Task RunBeforeAnyTests()
    {
        // Start PostgreSQL container
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("ignacheck_test")
            .WithUsername("test")
            .WithPassword("test123")
            .Build();

        await _dbContainer.StartAsync();
        _connectionString = _dbContainer.GetConnectionString();

        // Create test web application factory
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove existing DbContext registration
                    services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                    services.RemoveAll<ApplicationDbContext>();

                    // Add test database context
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseNpgsql(_connectionString);
                    });

                    // Replace authentication with test authentication that bypasses auth
                    services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();

                    // Mock the current user for tests
                    services.RemoveAll<IUser>();
                    services.AddScoped<IUser, TestUser>();

                    // Build the service provider
                    var sp = services.BuildServiceProvider();

                    // Create database and apply migrations
                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    db.Database.Migrate();
                });
            });

        _scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();

        // Initialize Respawner for database cleanup
        await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "public" },
            TablesToIgnore = new Respawn.Graph.Table[] { "__EFMigrationsHistory" }
        });
    }

    public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        return await mediator.Send(request);
    }

    public static async Task SendAsync(IRequest request)
    {
        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        await mediator.Send(request);
    }

    public static async Task<TEntity?> FindAsync<TEntity>(params object[] keyValues)
        where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await context.FindAsync<TEntity>(keyValues);
    }

    public static async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // For Organizations, set the TestUser's organization to match
        if (entity is Domain.Entities.Organization org)
        {
            TestUser.SetOrganizationId(org.Id);
        }

        // For tenant entities, temporarily set the tenant to match the entity's organization
        if (entity is Domain.Common.ITenantEntity tenantEntity && tenantEntity.OrganizationId != Guid.Empty)
        {
            var tenantService = scope.ServiceProvider.GetRequiredService<ITenantService>();
            tenantService.SetCurrentTenantId(tenantEntity.OrganizationId);

            // Also set the TestUser's organization to match
            TestUser.SetOrganizationId(tenantEntity.OrganizationId);
        }

        context.Add(entity);
        await context.SaveChangesAsync();
    }

    public static async Task<int> CountAsync<TEntity>() where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await context.Set<TEntity>().CountAsync();
    }

    public static HttpClient CreateClient()
    {
        return _factory.CreateClient();
    }

    public static async Task ResetState()
    {
        try
        {
            await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            await _respawner.ResetAsync(connection);
        }
        catch (Exception)
        {
            // Ignore errors during cleanup
        }
    }

    [OneTimeTearDown]
    public async Task RunAfterAnyTests()
    {
        await _dbContainer.DisposeAsync();
        await _factory.DisposeAsync();
    }
}

/// <summary>
/// Fake policy evaluator that bypasses authentication/authorization for integration tests
/// </summary>
public class FakePolicyEvaluator : IPolicyEvaluator
{
    public Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
    {
        var principal = new System.Security.Claims.ClaimsPrincipal();

        var identity = new System.Security.Claims.ClaimsIdentity(new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "test-user-id"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "Test User"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "test@test.com"),
        }, "Test");

        principal.AddIdentity(identity);

        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, "Test")));
    }

    public Task<PolicyAuthorizationResult> AuthorizeAsync(
        AuthorizationPolicy policy,
        AuthenticateResult authenticationResult,
        HttpContext context,
        object? resource)
    {
        return Task.FromResult(PolicyAuthorizationResult.Success());
    }
}

/// <summary>
/// Test user implementation for integration tests
/// </summary>
public class TestUser : IUser
{
    private static Guid? _currentOrganizationId;

    public string? Id => "test-user-id";
    public string? Email => "test@test.com";
    public Guid? OrganizationId => _currentOrganizationId;
    public string? OrganizationRole => "Admin";

    public static void SetOrganizationId(Guid? organizationId)
    {
        _currentOrganizationId = organizationId;
    }

    public static void ClearOrganizationId()
    {
        _currentOrganizationId = null;
    }
}

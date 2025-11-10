using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace IgnaCheck.Infrastructure.Data;

/// <summary>
/// Design-time factory for creating ApplicationDbContext during migrations.
/// This factory is only used by EF Core tools (migrations, scaffolding, etc.)
/// and allows the context to be created without runtime dependencies like ITenantService.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Determine the environment (default to Development for migrations)
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        // Build configuration to read connection string
        var webProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "../Web");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(webProjectPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddJsonFile("appsettings.PostgreSQL.json", optional: true) // PostgreSQL-specific config
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("IgnaCheckDb");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string 'IgnaCheckDb' not found. Checked in: {webProjectPath}");
        }

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Use PostgreSQL (matching the production configuration in DependencyInjection.cs)
        optionsBuilder.UseNpgsql(connectionString);

        // Create context without ITenantService (pass null)
        // The global query filter in ApplicationDbContext is designed to handle null tenant service
        return new ApplicationDbContext(optionsBuilder.Options, tenantService: null);
    }
}

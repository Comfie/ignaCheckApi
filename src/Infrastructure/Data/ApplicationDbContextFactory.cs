using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace IgnaCheck.Infrastructure.Data;

/// <summary>
/// Design-time factory for ApplicationDbContext.
/// This is used by EF Core tools (migrations, etc.) to create the DbContext at design time.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Find the Web project directory (where appsettings.json files are located)
        var currentDir = Directory.GetCurrentDirectory();
        var webProjectPath = Path.Combine(currentDir, "..", "Web");

        // If we're already in Infrastructure directory, go up and find Web
        if (currentDir.EndsWith("Infrastructure"))
        {
            webProjectPath = Path.Combine(currentDir, "..", "Web");
        }
        else if (currentDir.EndsWith("src"))
        {
            webProjectPath = Path.Combine(currentDir, "Web");
        }
        else
        {
            // Assume we're at solution root
            webProjectPath = Path.Combine(currentDir, "src", "Web");
        }

        // Build configuration
        var configBasePath = Directory.Exists(webProjectPath) ? webProjectPath : currentDir;
        var configuration = new ConfigurationBuilder()
            .SetBasePath(configBasePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile("appsettings.PostgreSQL.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Get connection string
        var connectionString = configuration.GetConnectionString("IgnaCheckDb")
                               ?? "Data Source=ignacheck.db"; // Fallback to SQLite

        // Create DbContextOptions
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Determine database provider based on connection string
        if (connectionString.Contains("Host=") || connectionString.Contains("Server=") && connectionString.Contains("Port="))
        {
            // PostgreSQL
            optionsBuilder.UseNpgsql(connectionString,
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
        }
        else if (connectionString.Contains("Data Source=") && connectionString.EndsWith(".db"))
        {
            // SQLite
            optionsBuilder.UseSqlite(connectionString,
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
        }
        else
        {
            // SQL Server
            optionsBuilder.UseSqlServer(connectionString,
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
        }

        // Use the parameterless constructor (without ITenantService)
        // Multi-tenancy filters won't be active at design time, which is fine for migrations
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}

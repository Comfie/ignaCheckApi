using IgnaCheck.Infrastructure;
using IgnaCheck.Infrastructure.Data;
using IgnaCheck.Web;
using Serilog;
using Serilog.Events;

// Configure Serilog early - before building the host
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .Enrich.WithProcessId()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/ignacheck-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting IgnaCheck API application");

    var builder = WebApplication.CreateBuilder(args);

    // Replace default logging with Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentName()
        .Enrich.WithThreadId()
        .Enrich.WithProcessId()
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}")
        .WriteTo.File(
            path: "logs/ignacheck-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}"));

    // Add services to the container.
    builder.AddKeyVaultIfConfigured();
    builder.AddApplicationServices();
    builder.AddInfrastructureServices();
    builder.AddWebServices();

    var app = builder.Build();

    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("IgnaCheck API starting up...");
    logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);

    // Add Serilog request logging - must be early in pipeline
    app.UseSerilogRequestLogging(options =>
    {
        // Customize the message template
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

        // Emit debug-level events instead of info for requests with 2xx status codes
        options.GetLevel = (httpContext, elapsed, ex) => ex != null
            ? LogEventLevel.Error
            : httpContext.Response.StatusCode > 499
                ? LogEventLevel.Error
                : httpContext.Response.StatusCode > 399
                    ? LogEventLevel.Warning
                    : LogEventLevel.Information;

        // Attach additional properties to the request completion event
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? "unknown");
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme ?? "http");
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString() ?? "unknown");
            diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");

            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value ?? "unknown");
                diagnosticContext.Set("UserName", httpContext.User.Identity.Name ?? "unknown");
            }
        };
    });

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        // Skip database initialization if running under NSwag (for faster OpenAPI generation)
        var skipDbInit = Environment.GetEnvironmentVariable("SKIP_DB_INIT") == "true"
                         || Environment.GetEnvironmentVariable("NSWAG_EXECUTOR_MODE") == "true";

        if (!skipDbInit)
        {
            logger.LogInformation("Initializing database...");
            await app.InitialiseDatabaseAsync();
            logger.LogInformation("Database initialization completed.");
        }
        else
        {
            logger.LogInformation("Database initialization skipped (SKIP_DB_INIT or NSWAG_EXECUTOR_MODE set)");
        }
    }
    else
    {
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHealthChecks("/health");
    app.UseHttpsRedirection();
    app.UseStaticFiles();

    // Enable CORS for separated frontend deployment
    app.UseCors("AllowFrontend");

    app.UseSwaggerUi(settings =>
    {
        settings.Path = "/api";
        settings.DocumentPath = "/api/specification.json";
    });

    app.UseExceptionHandler(options => { });

    // Add authentication and authorization
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapEndpoints();

    logger.LogInformation("Application configured. Starting web server...");
    logger.LogInformation("API running in standalone mode (frontend deployed separately)");

    app.Run();
    Log.Information("IgnaCheck API application stopped gracefully");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }

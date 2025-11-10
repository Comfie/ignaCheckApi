using IgnaCheck.Infrastructure;
using IgnaCheck.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddKeyVaultIfConfigured();
builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("IgnaCheck API starting up...");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);

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

app.UseSwaggerUi(settings =>
{
    settings.Path = "/api";
    settings.DocumentPath = "/api/specification.json";
});

app.MapFallbackToFile("index.html");

app.UseExceptionHandler(options => { });

// Add authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapEndpoints();

logger.LogInformation("Application configured. Starting web server...");
app.Run();

public partial class Program { }

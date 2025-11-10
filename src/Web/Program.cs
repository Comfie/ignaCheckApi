using IgnaCheck.Infrastructure;
using IgnaCheck.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddKeyVaultIfConfigured();
builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Skip database initialization if running under NSwag (for faster OpenAPI generation)
    var skipDbInit = Environment.GetEnvironmentVariable("SKIP_DB_INIT") == "true"
                     || Environment.GetEnvironmentVariable("NSWAG_EXECUTOR_MODE") == "true";

    if (!skipDbInit)
    {
        await app.InitialiseDatabaseAsync();
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

app.MapRazorPages();

app.MapFallbackToFile("index.html");

app.UseExceptionHandler(options => { });

// Add authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapEndpoints();

app.Run();

public partial class Program { }

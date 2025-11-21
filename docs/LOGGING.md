# IgnaCheck API - Logging Guide

## Overview

IgnaCheck API uses **Serilog** for structured logging across all layers of the application. This provides rich, queryable logs that help with debugging, monitoring, and auditing.

## Why Serilog?

- **Structured Logging**: Logs are written as structured data, not just plain text
- **Multiple Sinks**: Write to console, files, databases, cloud services simultaneously
- **Performance**: Minimal overhead with async logging
- **Enrichers**: Automatically add contextual information (machine name, thread ID, etc.)
- **Flexible Configuration**: Configure via code or appsettings.json

---

## Configuration

### Packages Installed

```xml
<PackageReference Include="Serilog.AspNetCore" />
<PackageReference Include="Serilog.Sinks.Console" />
<PackageReference Include="Serilog.Sinks.File" />
<PackageReference Include="Serilog.Sinks.Seq" />
<PackageReference Include="Serilog.Enrichers.Environment" />
<PackageReference Include="Serilog.Enrichers.Thread" />
<PackageReference Include="Serilog.Enrichers.Process" />
```

### Program.cs Setup

Serilog is configured in two phases:

#### 1. Bootstrap Logger (Early Initialization)

```csharp
// Configure Serilog early - before building the host
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console()
    .WriteTo.File("logs/ignacheck-.log", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();
```

**Purpose**: Captures startup errors before the host is built.

#### 2. Full Logger Configuration

```csharp
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)  // Read from appsettings.json
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/ignacheck-.log", rollingInterval: RollingInterval.Day));
```

**Purpose**: Full configuration with access to appsettings.json and dependency injection.

### Exception Handling

```csharp
try
{
    Log.Information("Starting IgnaCheck API application");
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
    Log.CloseAndFlush();  // Ensure all logs are written before exit
}
```

---

## Configuration Files

### appsettings.json (Base Configuration)

```json
{
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File", "Serilog.Sinks.Seq" ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/ignacheck-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ],
    "Enrich": [ "FromLogContext", "WithMachineName", "WithEnvironmentName" ]
  }
}
```

### appsettings.Development.json

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft.EntityFrameworkCore": "Information",
        "IgnaCheck": "Debug"
      }
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "logs/ignacheck-dev-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      }
    ]
  }
}
```

**Development Features**:
- `Debug` level logging for application code
- SQL queries visible (`EF Core = Information`)
- Shorter log retention (7 days)

### appsettings.Production.json

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning",
      "Override": {
        "IgnaCheck": "Information"
      }
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "/app/logs/ignacheck-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 90,
          "fileSizeLimitBytes": 104857600,
          "rollOnFileSizeLimit": true
        }
      }
    ]
  }
}
```

**Production Features**:
- `Warning` level by default (less noise)
- Application logs at `Information` level
- Longer retention (90 days)
- File size limits (100MB per file)

---

## HTTP Request Logging

All HTTP requests are automatically logged with rich context:

```csharp
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

    // Dynamic log levels based on status code
    options.GetLevel = (httpContext, elapsed, ex) => ex != null
        ? LogEventLevel.Error          // Exceptions = Error
        : httpContext.Response.StatusCode > 499
            ? LogEventLevel.Error       // 5xx = Error
            : httpContext.Response.StatusCode > 399
                ? LogEventLevel.Warning // 4xx = Warning
                : LogEventLevel.Information; // 2xx/3xx = Information

    // Enrich with additional context
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? "unknown");
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString() ?? "unknown");
        diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");

        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value ?? "unknown");
            diagnosticContext.Set("UserName", httpContext.User.Identity.Name ?? "unknown");
        }
    };
});
```

### Example Request Log

```
[14:23:45 INF] HTTP GET /api/v1/projects responded 200 in 23.4567 ms
RequestHost: localhost:5000
UserAgent: Mozilla/5.0...
RemoteIP: 127.0.0.1
UserId: user-123
UserName: john.doe@example.com
```

---

## Logging in Application Code

### 1. Inject ILogger

```csharp
public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateProjectCommandHandler> _logger;

    public CreateProjectCommandHandler(
        IApplicationDbContext context,
        ILogger<CreateProjectCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }
}
```

### 2. Use Structured Logging

```csharp
// ❌ BAD: String concatenation
_logger.LogInformation("Creating project with name " + command.Name);

// ✅ GOOD: Structured logging with properties
_logger.LogInformation("Creating project {ProjectName} for organization {OrganizationId}",
    command.Name,
    organizationId);
```

**Why?** Properties are indexed and queryable in log analysis tools.

### 3. Log Levels

```csharp
// TRACE - Very detailed, typically only in development
_logger.LogTrace("Entering CreateProject handler with {ProjectId}", projectId);

// DEBUG - Diagnostic information useful for debugging
_logger.LogDebug("Retrieved {Count} existing projects from database", existingProjects.Count);

// INFORMATION - General flow of the application
_logger.LogInformation("Project {ProjectId} created successfully by user {UserId}",
    project.Id,
    userId);

// WARNING - Unexpected but not necessarily error conditions
_logger.LogWarning("Project name '{ProjectName}' already exists, using generated name",
    command.Name);

// ERROR - Errors and exceptions that can be handled
_logger.LogError(ex, "Failed to create project {ProjectName}", command.Name);

// CRITICAL - Serious failures requiring immediate attention
_logger.LogCritical(ex, "Database connection lost - cannot create project");
```

### 4. Exception Logging

```csharp
try
{
    await _context.SaveChangesAsync(cancellationToken);
}
catch (DbUpdateException ex)
{
    _logger.LogError(ex,
        "Database error while saving project {ProjectId} for organization {OrganizationId}",
        project.Id,
        organizationId);
    throw;
}
```

**Note**: Exception is the first parameter, then message template.

---

## Logging Examples by Layer

### Infrastructure Layer: SoftDeleteInterceptor

```csharp
public class SoftDeleteInterceptor : SaveChangesInterceptor
{
    private readonly ILogger<SoftDeleteInterceptor> _logger;

    private void HandleSoftDelete(DbContext? context)
    {
        foreach (var entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State == EntityState.Deleted)
            {
                var entityType = entry.Entity.GetType().Name;
                var entityId = entry.Entity.Id;

                _logger.LogDebug(
                    "Converting hard delete to soft delete for {EntityType} with ID {EntityId}",
                    entityType,
                    entityId);

                // ... perform soft delete ...

                _logger.LogInformation(
                    "Soft deleted {EntityType} with ID {EntityId} by user {UserId}",
                    entityType,
                    entityId,
                    _user.Id);
            }
        }
    }
}
```

### Application Layer: Event Handler

```csharp
public class EntityDeletedEventHandler : INotificationHandler<EntityDeletedEvent>
{
    private readonly ILogger<EntityDeletedEventHandler> _logger;

    public async Task Handle(EntityDeletedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // Create activity log
            _context.ActivityLogs.Add(activityLog);

            _logger.LogInformation(
                "Logged deletion of {EntityType} {EntityId} by user {UserId}",
                notification.EntityType,
                notification.EntityId,
                userId);
        }
        catch (Exception ex)
        {
            // Never throw from event handlers - log and continue
            _logger.LogError(ex,
                "Error logging entity deletion for {EntityType} {EntityId}",
                notification.EntityType,
                notification.EntityId);
        }
    }
}
```

### Application Layer: Command Handler

```csharp
public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Result>
{
    public async Task<Result> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating project '{ProjectName}' for organization {OrganizationId}",
            request.Name,
            organizationId);

        try
        {
            var project = new Project
            {
                Name = request.Name,
                Description = request.Description,
                OrganizationId = organizationId.Value
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully created project {ProjectId} '{ProjectName}'",
                project.Id,
                project.Name);

            return Result.Success(project.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create project '{ProjectName}' for organization {OrganizationId}",
                request.Name,
                organizationId);

            return Result.Failure(new[] { "Failed to create project." });
        }
    }
}
```

---

## Log File Location

### Development
```
/logs/ignacheck-dev-20250114.log
/logs/ignacheck-dev-20250113.log
...
```

### Production
```
/app/logs/ignacheck-20250114.log
/app/logs/ignacheck-20250113.log
...
```

---

## Log Output Templates

### Console Template
```
[14:23:45 INF] IgnaCheck.Application.Projects.Commands.CreateProject.CreateProjectCommandHandler
Creating project 'SOC 2 Audit' for organization 12345678-1234-1234-1234-123456789012
```

### File Template
```
2025-01-14 14:23:45.123 +00:00 [INF] IgnaCheck.Application.Projects.Commands.CreateProject.CreateProjectCommandHandler - Creating project 'SOC 2 Audit' for organization 12345678-1234-1234-1234-123456789012
```

---

## Querying Logs

### Find All Errors Today
```bash
grep "\[ERR\]" logs/ignacheck-20250114.log
```

### Find Logs for Specific User
```bash
grep "UserId.*user-123" logs/ignacheck-20250114.log
```

### Find Slow Requests (>1000ms)
```bash
grep "responded.*in [0-9][0-9][0-9][0-9]" logs/ignacheck-20250114.log
```

### Monitor Live Logs
```bash
tail -f logs/ignacheck-dev-20250114.log
```

---

## Advanced: Seq Integration (Optional)

[Seq](https://datalust.co/seq) is a powerful log viewer for structured logs.

### 1. Run Seq Locally

```bash
docker run -d --restart unless-stopped --name seq -e ACCEPT_EULA=Y -p 5341:80 datalust/seq:latest
```

### 2. Add Seq Sink to appsettings.Development.json

```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://localhost:5341"
        }
      }
    ]
  }
}
```

### 3. View Logs

Open `http://localhost:5341` in your browser to query and visualize logs.

---

## Best Practices

### ✅ DO

1. **Use Structured Logging**
   ```csharp
   _logger.LogInformation("User {UserId} deleted project {ProjectId}", userId, projectId);
   ```

2. **Log at Appropriate Levels**
   - Debug: Diagnostic info
   - Information: Normal flow
   - Warning: Unexpected but handled
   - Error: Failures
   - Critical: System failures

3. **Include Context**
   ```csharp
   _logger.LogError(ex, "Failed to process order {OrderId} for customer {CustomerId}",
       orderId, customerId);
   ```

4. **Log Business Events**
   ```csharp
   _logger.LogInformation("Invoice {InvoiceId} paid by customer {CustomerId}", invoiceId, customerId);
   ```

5. **Use LoggerMessage for Hot Paths** (Performance)
   ```csharp
   private static readonly Action<ILogger, string, Exception?> _projectCreated =
       LoggerMessage.Define<string>(
           LogEventLevel.Information,
           new EventId(1, nameof(ProjectCreated)),
           "Project {ProjectName} created successfully");

   _projectCreated(_logger, project.Name, null);
   ```

### ❌ DON'T

1. **Don't Log Sensitive Data**
   ```csharp
   // ❌ BAD
   _logger.LogInformation("User logged in with password {Password}", password);

   // ✅ GOOD
   _logger.LogInformation("User {UserId} logged in successfully", userId);
   ```

2. **Don't Use String Interpolation**
   ```csharp
   // ❌ BAD - Always evaluated
   _logger.LogDebug($"Processing {items.Count} items");

   // ✅ GOOD - Only evaluated if Debug is enabled
   _logger.LogDebug("Processing {Count} items", items.Count);
   ```

3. **Don't Log in Loops (Use Aggregates)**
   ```csharp
   // ❌ BAD
   foreach (var item in items)
   {
       _logger.LogInformation("Processing item {ItemId}", item.Id);
   }

   // ✅ GOOD
   _logger.LogInformation("Processing {Count} items", items.Count);
   ```

4. **Don't Throw After Logging in Event Handlers**
   ```csharp
   // ❌ BAD - Breaks the request
   catch (Exception ex)
   {
       _logger.LogError(ex, "Failed to create audit log");
       throw;
   }

   // ✅ GOOD - Log and continue
   catch (Exception ex)
   {
       _logger.LogError(ex, "Failed to create audit log");
       // Don't throw - audit logging failures shouldn't break requests
   }
   ```

---

## Monitoring and Alerts

### Production Monitoring

Monitor these log patterns:

1. **Error Rate**
   - Alert if error rate > 1% of total requests
   - Track `[ERR]` and `[FTL]` entries

2. **Slow Requests**
   - Alert if request duration > 5000ms
   - Track "responded {StatusCode} in {Elapsed}" logs

3. **Failed Logins**
   - Track "Login failed" patterns
   - Alert on multiple failures from same IP

4. **Database Errors**
   - Track "DbUpdateException", "SqlException"
   - Alert on connection pool exhaustion

### Example Alert Queries (for Seq)

```sql
-- Errors in last 5 minutes
@Level = 'Error' AND @Timestamp > Now()-5m

-- Slow requests
Elapsed > 5000

-- Failed authentication
@Message LIKE '%authentication failed%'

-- Soft delete operations
EntityType IS NOT NULL AND IsDeleted = true
```

---

## Troubleshooting

### Logs Not Appearing

1. **Check Log Level**
   - Ensure appsettings.json has correct `MinimumLevel`
   - Development should be `Debug` or `Information`

2. **Check File Permissions**
   ```bash
   chmod 755 logs/
   ```

3. **Verify Serilog Initialization**
   ```csharp
   Log.Information("Test log message");
   ```

### Too Many Logs

1. **Increase Minimum Level for Noisy Namespaces**
   ```json
   {
     "Serilog": {
       "MinimumLevel": {
         "Override": {
           "Microsoft.EntityFrameworkCore.Database.Command": "Warning"
         }
       }
     }
   }
   ```

2. **Reduce Request Logging**
   ```csharp
   options.GetLevel = (ctx, elapsed, ex) =>
       ctx.Response.StatusCode == 200 ? LogEventLevel.Debug : LogEventLevel.Information;
   ```

### Performance Issues

1. **Use Async Sinks**
   ```json
   {
     "WriteTo": [
       {
         "Name": "Async",
         "Args": {
           "configure": [
             { "Name": "File", "Args": { "path": "logs/log.txt" } }
           ]
         }
       }
     ]
   }
   ```

2. **Limit Log Retention**
   ```json
   "retainedFileCountLimit": 7  // Keep only 7 days
   ```

---

## Summary

IgnaCheck API now has comprehensive structured logging:

✅ **Serilog configured** with console and file sinks
✅ **HTTP request logging** with rich context
✅ **Environment-specific configuration** (Dev/Prod)
✅ **Logging in all layers** (Infrastructure, Application, Domain)
✅ **Best practices documented** with examples
✅ **Production-ready** with log rotation and retention

All changes are logged with structured data, making debugging and monitoring significantly easier!

---

**Last Updated**: November 14, 2025
**Serilog Version**: 9.0.0
**Configuration Files**: appsettings.json, appsettings.Development.json, appsettings.Production.json

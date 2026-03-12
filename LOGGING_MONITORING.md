# FinShark Logging & Monitoring Guide

Complete guide to structured logging with Serilog and application monitoring.

## Overview

FinShark uses **Serilog** for structured logging across all layers with:
- Structured JSON events
- Role-based log levels
- File and console sinks
- Automatic correlation tracking
- Performance metrics

---

## Serilog Configuration

### Base Configuration (appsettings.json)

```json
{
  "Serilog": {
    "MinimumLevel": "Information",
    "Override": {
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss.fff}] [{Level:u3}] {SourceContext:l}: {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "bin/Debug/logs/finshark-.txt",
          "rollingInterval": "Day",
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
        }
      }
    ]
  }
}
```

### Setup in Program.cs

```csharp
using Serilog;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Configure Serilog from configuration file
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "FinShark.API")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .CreateLogger();

try
{
    Log.Information("FinShark application starting up");
    
    builder.Host.UseSerilog(); // Use Serilog for host logging
    
    var app = builder.Build();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
```

---

## Log Levels

| Level | Use Case | Example |
|-------|----------|---------|
| **Trace** | Very detailed diagnostic info | Variable values, loop iterations |
| **Debug** | Diagnostic info for developers | Method entry/exit, variable state |
| **Information** | General app flow | User actions, important events |
| **Warning** | Potentially problematic | Deprecated API, unusual state |
| **Error** | Recoverable errors | Validation failure, transient issue |
| **Fatal** | Critical failures | App can't continue |

---

## Logging in Code

### Basic Logging

```csharp
using Serilog;
using Microsoft.Extensions.Logging;

public class StocksController : ControllerBase
{
    private readonly ILogger<StocksController> _logger;
    private readonly IMediator _mediator;

    public StocksController(ILogger<StocksController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateStock([FromBody] CreateStockRequestDto request)
    {
        _logger.LogInformation("Stock creation requested for symbol: {Symbol}", request.Symbol);
        
        var command = new CreateStockCommand(request.Symbol);
        var result = await _mediator.Send(command);
        
        _logger.LogInformation("Stock created with ID: {StockId}", result);
        
        return CreatedAtAction(nameof(GetStockById), new { id = result }, result);
    }
}
```

### Structured Logging with Properties

```csharp
_logger.LogInformation(
    "Stock updated. Id: {StockId}, Symbol: {Symbol}, Price: {Price}, User: {UserId}",
    stock.Id,
    stock.Symbol,
    stock.CurrentPrice,
    userId
);
```

Output:
```json
{
  "Timestamp": "2026-03-12T12:34:56.789Z",
  "Level": "Information",
  "Message": "Stock updated. Id: 1, Symbol: AAPL, Price: 250.50, User: user123",
  "StockId": 1,
  "Symbol": "AAPL",
  "Price": 250.50,
  "UserId": "user123",
  "SourceContext": "FinShark.API.Controllers.StocksController"
}
```

---

## Exception Logging

### Log Exceptions Properly

```csharp
try
{
    var stock = await _stockRepository.GetByIdAsync(id);
    if (stock == null)
        throw new StockNotFoundException($"Stock with ID {id} not found");
}
catch (Exception ex)
{
    // Log with full context
    _logger.LogError(ex, 
        "Error retrieving stock. StockId: {StockId}, User: {UserId}",
        id,
        userId);
    
    throw; // Re-throw to be handled by exception middleware
}
```

### Exception Middleware Logging

```csharp
// FinShark.API/Middleware/ExceptionMiddleware.cs
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log exception with request context
            _logger.LogError(ex,
                "Unhandled exception. Method: {Method}, Path: {Path}, User: {User}",
                context.Request.Method,
                context.Request.Path,
                context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "Anonymous");

            var response = new ApiResponse<object>
            {
                Success = false,
                Errors = new[] { "An unexpected error occurred" }
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
```

---

## Performance Logging

### Log Method Execution Time

```csharp
public class PerformanceLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceLoggingBehavior<TRequest, TResponse>> _logger;

    public PerformanceLoggingBehavior(ILogger<PerformanceLoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            return await next();
        }
        finally
        {
            stopwatch.Stop();
            
            var requestName = typeof(TRequest).Name;
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            if (elapsedMilliseconds > 500) // Log if slow
            {
                _logger.LogWarning(
                    "Slow request detected. Request: {RequestName}, Duration: {Duration}ms",
                    requestName,
                    elapsedMilliseconds);
            }
            else
            {
                _logger.LogDebug(
                    "Request completed. Request: {RequestName}, Duration: {Duration}ms",
                    requestName,
                    elapsedMilliseconds);
            }
        }
    }
}

// Register in Program.cs
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceLoggingBehavior<,>));
});
```

---

## Correlation IDs

Track requests across services:

```csharp
// Middleware to add correlation ID
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue("X-Correlation-ID", out var value)
            ? value.ToString()
            : Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers.Add("X-Correlation-ID", correlationId);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}

// In Program.cs
app.UseMiddleware<CorrelationIdMiddleware>();
```

All logs now include `CorrelationId`:
```json
{
  "Timestamp": "2026-03-12T12:34:56.789Z",
  "Level": "Information",
  "Message": "Stock created successfully",
  "CorrelationId": "550e8400-e29b-41d4-a716-446655440000"
}
```

---

## Log Aggregation

### Development - Local Files

Logs stored in: `bin/Debug/logs/`

```powershell
# View latest logs
Get-Content "bin/Debug/logs/*.txt" -Tail 100

# Search logs
Select-String "Error" "bin/Debug/logs/*.txt"
```

### Production - Centralized

Integrate with logging services:

```python
# ElasticSearch + Kibana
# Splunk
# Azure Monitor
# DataDog
# New Relic
```

Example Serilog configuration for Elasticsearch:

```json
{
  "WriteTo": [
    {
      "Name": "Elasticsearch",
      "Args": {
        "nodeUris": "http://elasticsearch:9200",
        "indexFormat": "finshark-{0:yyyy.MM.dd}",
        "autoRegisterTemplate": true
      }
    }
  ]
}
```

---

## Database Query Logging

### Enable EF Core Logging

```csharp
// Development
services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
        .LogTo(Console.WriteLine, LogLevel.Information) // Console output
        .EnableSensitiveDataLogging(); // Log parameter values
});
```

**Output**:
```
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (42ms) [Parameters=[@__symbol_0='?'], CommandType='Text', CommandTimeout='30']
      SELECT TOP(1) [s].[Id], [s].[Symbol], [s].[CompanyName], [s].[CurrentPrice], [s].[Industry], [s].[MarketCap], [s].[CreatedAt], [s].[UpdatedAt]
      FROM [Stocks] AS [s]
      WHERE [s].[Symbol] = @__symbol_0
```

### Log Slow Queries

```csharp
services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
        .LogTo(category => category == "Microsoft.EntityFrameworkCore.Database.Command" 
                ? LogLevel.Warning 
                : LogLevel.None)
        .ConfigureWarnings(warnings => 
            warnings.Throw(RelationalEventId.CommandExecutionStarting));
});
```

---

## Health Check Logging

Monitor application health:

```csharp
// Add health checks
services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

// In Program.cs
app.MapHealthChecks("/health");
```

Monitor health:
```powershell
curl https://localhost:5001/health

# Output:
# Healthy (200)

curl https://localhost:5001/health/detailed

# Output:
# {
#   "status": "Healthy",
#   "checks": {
#     "AppDbContext": {
#       "status": "Healthy",
#       "description": "Database connection successful"
#     }
#   }
# }
```

---

## Metrics & Instrumentation

### Add Application Insights

```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

### Log Custom Metrics

```csharp
var stopwatch = Stopwatch.StartNew();

var result = await _stockRepository.GetAllAsync();

stopwatch.Stop();
_logger.LogInformation(
    "Query completed. Duration: {Duration}ms, ResultCount: {Count}",
    stopwatch.ElapsedMilliseconds,
    result.Count());
```

---

## Log Retention

### Development
- Daily rolling files
- 7-day retention

### Production
- Daily rolling files
- 90-day retention
- Compressed after 30 days

```json
{
  "WriteTo": [
    {
      "Name": "File",
      "Args": {
        "path": "/var/log/finshark/finshark-.txt",
        "rollingInterval": "Day",
        "retainedFileCountLimit": 90,
        "fileSizeLimitBytes": 1073741824, // 1GB per file
        "rollOnFileSizeLimit": true
      }
    }
  ]
}
```

---

## Monitoring Checklist

- [ ] All exceptions logged with context
- [ ] Slow queries logged (>500ms)
- [ ] Request/response logged
- [ ] Performance metrics tracked
- [ ] Health checks implemented
- [ ] Error alerts configured
- [ ] Log aggregation enabled
- [ ] Retention policies set

---

## Best Practices

✅ **Do This**:
- Include context in logs (IDs, user, action)
- Use structured logging with named properties
- Log at appropriate levels
- Implement correlation IDs
- Monitor performance metrics
- Alert on errors and slow operations

❌ **Never Do This**:
- Log sensitive data (passwords, tokens)
- Use string concatenation (use structured logging)
- Ignore exceptions
- Log too much (performance impact)
- Store logs on local disk in production

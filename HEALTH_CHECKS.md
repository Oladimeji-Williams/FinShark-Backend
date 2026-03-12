# FinShark Health Checks & Monitoring

Comprehensive health checks, status endpoints, and application monitoring.

## Health Checks Overview

Health checks monitor application and infrastructure health in real-time.

```
Health Check Service
    ├── Database connection check
    ├── Memory usage check
    ├── Response time check
    ├── Disk space check
    └── Custom business logic checks
```

---

## Setup

### Installation

```bash
dotnet add package AspNetCore.HealthChecks.SqlServer
dotnet add package AspNetCore.HealthChecks.UI
```

### Configuration

```csharp
// Program.cs
var builder = WebApplicationBuilder.CreateBuilder(args);

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database", 
        failureStatus: HealthStatus.Unhealthy)
    .AddCheck("live", () => HealthCheckResult.Healthy("Live check passed"), 
        tags: new[] { "live" });

// Register Health Checks UI (optional)
builder.Services.AddHealthChecksUI()
    .AddInMemoryStorage();

var app = builder.Build();

// Map health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = healthCheck => healthCheck.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = healthCheck => healthCheck.Tags.Contains("live")
});

// Optional: Map UI
app.MapHealthChecksUI();

app.Run();
```

---

## Health Check Endpoints

### 1. Liveness Probe `/health/live`

Checks if app is running.

**Response (200 OK)**:
```json
{
  "status": "Healthy"
}
```

**Use Case**:
- Kubernetes liveness probe
- Container restart triggers
- Readiness for accepting connections

```powershell
curl https://localhost:5001/health/live

# Output: 200 OK
```

### 2. Readiness Probe `/health/ready`

Checks if app is ready to serve requests.

**Response (200 OK)**:
```json
{
  "status": "Healthy",
  "checks": {
    "database": {
      "status": "Healthy",
      "duration": "00:00:0.1234567"
    }
  }
}
```

**Response (503 Service Unavailable)**:
```json
{
  "status": "Unhealthy",
  "checks": {
    "database": {
      "status": "Unhealthy",
      "description": "Connection timeout",
      "duration": "00:00:2.5000000"
    }
  }
}
```

**Use Case**:
- Kubernetes readiness probe
- Load balancer health checks
- Before routing traffic

```powershell
curl https://localhost:5001/health/ready

# Output: 200 OK or 503 Service Unavailable
```

### 3. Full Health Check `/health`

Detailed health status.

**Response (200 OK)**:
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:0.2500000",
  "entries": {
    "database": {
      "status": "Healthy",
      "description": null,
      "duration": "00:00:0.1000000",
      "data": {}
    },
    "live": {
      "status": "Healthy",
      "description": "Live check passed",
      "duration": "00:00:0.0000100",
      "data": {}
    }
  }
}
```

```powershell
curl https://localhost:5001/health

# Full health details
```

---

## Built-in Health Checks

### Database Check

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(
        name: "database",
        failureStatus: HealthStatus.Unhealthy,
        timeout: TimeSpan.FromSeconds(5));
```

Verifies:
- Database connection
- Context can query
- Command execution

### Custom Health Checks

```csharp
public class StockServiceHealthCheck : IHealthCheck
{
    private readonly IStockRepository _repository;

    public StockServiceHealthCheck(IStockRepository repository)
    {
        _repository = repository;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check basic stock operation
            var stockCount = await _repository.GetCountAsync(cancellationToken);
            
            if (stockCount >= 0)
            {
                return HealthCheckResult.Healthy(
                    $"Database contains {stockCount} stocks");
            }

            return HealthCheckResult.Unhealthy("Unable to count stocks");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                $"Stock service unhealthy: {ex.Message}");
        }
    }
}

// Register
builder.Services.AddHealthChecks()
    .AddCheck<StockServiceHealthCheck>("stocks");
```

---

## Advanced Checks

### Memory Health Check

```csharp
public class MemoryHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var totalMemory = GC.GetTotalMemory(false);
        const long warningThreshold = 500_000_000; // 500 MB
        const long criticalThreshold = 1_000_000_000; // 1 GB

        if (totalMemory > criticalThreshold)
            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"Memory usage critical: {totalMemory / 1_000_000}MB"));

        if (totalMemory > warningThreshold)
            return Task.FromResult(HealthCheckResult.Degraded(
                $"Memory usage high: {totalMemory / 1_000_000}MB"));

        return Task.FromResult(HealthCheckResult.Healthy(
            $"Memory usage normal: {totalMemory / 1_000_000}MB"));
    }
}
```

### API Response Time Check

```csharp
public class ResponseTimeHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ResponseTimeHealthCheck(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var stopwatch = Stopwatch.StartNew();
            
            var response = await client.GetAsync(
                "https://localhost:5001/api/stocks",
                cancellationToken);
            
            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds > 2000)
                return HealthCheckResult.Degraded(
                    $"API slow: {stopwatch.ElapsedMilliseconds}ms");

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy(
                    $"API responsive: {stopwatch.ElapsedMilliseconds}ms")
                : HealthCheckResult.Unhealthy("API returned error");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                $"API unreachable: {ex.Message}");
        }
    }
}
```

---

## Health Check UI

### Visual Dashboard

```csharp
// Program.cs
builder.Services.AddHealthChecksUI()
    .AddInMemoryStorage();

app.MapHealthChecksUI(options => 
{
    options.UIPath = "/health-ui";
    options.ApiPath = "/health-ui-api";
});
```

**Access Dashboard**:
- URL: `https://localhost:5001/health-ui`
- Shows: All health checks with status
- Real-time: Updates every 10 seconds

### Configuration

```csharp
builder.Services.Configure<HealthCheckOptions>(options =>
{
    options.DetailedOutput = true;
    options.Predicate = _ => true;
    options.AllowCachingResponses = false;
});
```

---

## Kubernetes Integration

### Liveness Probe

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: finshark-api
spec:
  containers:
  - name: api
    image: finshark:latest
    livenessProbe:
      httpGet:
        path: /health/live
        port: 5001
        scheme: HTTPS
      initialDelaySeconds: 10
      periodSeconds: 10
      timeoutSeconds: 5
      failureThreshold: 3
```

### Readiness Probe

```yaml
readinessProbe:
  httpGet:
    path: /health/ready
    port: 5001
    scheme: HTTPS
  initialDelaySeconds: 5
  periodSeconds: 5
  timeoutSeconds: 3
  failureThreshold: 3
```

### Complete Deployment

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: finshark-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: finshark-api
  template:
    metadata:
      labels:
        app: finshark-api
    spec:
      containers:
      - name: api
        image: finshark:1.0.0
        ports:
        - containerPort: 5001
        livenessProbe:
          httpGet:
            path: /health/live
            port: 5001
          initialDelaySeconds: 10
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 5001
          initialDelaySeconds: 5
          periodSeconds: 5
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
```

---

## Monitoring & Alerting

### Application Insights

```csharp
builder.Services.AddApplicationInsightsTelemetry();

app.UseApplicationInsightRequestTelemetry();
app.UseApplicationInsightExceptionTelemetry();
```

### Log Health Check Results

```csharp
public class HealthCheckLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HealthCheckLoggingMiddleware> _logger;

    public HealthCheckLoggingMiddleware(RequestDelegate next, ILogger<HealthCheckLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/health"))
        {
            var stopwatch = Stopwatch.StartNew();
            await _next(context);
            stopwatch.Stop();

            _logger.LogInformation(
                "Health check completed. Status: {StatusCode}, Duration: {Duration}ms",
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
        else
        {
            await _next(context);
        }
    }
}

// Register
app.UseMiddleware<HealthCheckLoggingMiddleware>();
```

---

## Docker Health Check

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

COPY bin/Release/net10.0 .

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f https://localhost:5001/health/live || exit 1

EXPOSE 5000 5001

ENTRYPOINT ["dotnet", "FinShark.API.dll"]
```

---

## Testing Health Checks

```csharp
[Fact]
public async Task HealthCheck_WhenDatabaseConnected_ReturnsHealthy()
{
    // Arrange
    var healthCheck = new DbContextCheck<AppDbContext>();
    var context = new HealthCheckContext { Registration = new HealthCheckRegistration("test", healthCheck, null, null) };

    // Act
    var result = await healthCheck.CheckHealthAsync(context);

    // Assert
    Assert.Equal(HealthStatus.Healthy, result.Status);
}

[Fact]
public async Task HealthCheck_WhenDatabaseDisconnected_ReturnsUnhealthy()
{
    // Arrange
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer("Server=invalid;")
        .Options;

    var healthCheck = new DbContextCheck<AppDbContext>(options);
    var context = new HealthCheckContext { Registration = new HealthCheckRegistration("test", healthCheck, null, null) };

    // Act
    var result = await healthCheck.CheckHealthAsync(context);

    // Assert
    Assert.Equal(HealthStatus.Unhealthy, result.Status);
}
```

---

## Monitoring Best Practices

✅ **Do This**:
- Monitor database connectivity
- Track response times
- Alert on failures
- Log health changes
- Test probes regularly
- Include meaningful data

❌ **Never Do This**:
- Block on I/O in health checks
- Make health checks slow
- Ignore warnings
- Disable health checks in production
- Check too frequently
- Log sensitive data

---

## Response Status Codes

| Status | HTTP Code | Meaning |
|--------|-----------|---------|
| **Healthy** | 200 | Fully operational |
| **Degraded** | 200 | Operational but degraded |
| **Unhealthy** | 503 | Not operational |

---

## Useful Commands

```bash
# Check local health
curl https://localhost:5001/health -k

# Check liveness
curl https://localhost:5001/health/live -k

# Check readiness
curl https://localhost:5001/health/ready -k

# Check with PowerShell
$ProgressPreference = 'SilentlyContinue'
Invoke-RestMethod https://localhost:5001/health -SkipCertificateCheck

# Watch health status
while($true) {
    Clear-Host
    (Invoke-RestMethod https://localhost:5001/health -SkipCertificateCheck | ConvertTo-Json)
    Start-Sleep -Seconds 5
}
```

---

## Troubleshooting

### Health Check Returns 503

**Causes**:
- Database connection failed
- Custom check failed
- Timeout exceeded

**Solution**:
1. Check logs for errors
2. Verify database connectivity
3. Increase timeout if needed
4. Check network connectivity

### Health Check Response Slow

**Causes**:
- Database query slow
- Network latency
- Too many checks

**Solution**:
1. Add indexes to database
2. Reduce check frequency
3. Optimize query
4. Remove slow checks

### Health Check Never Improves

**Causes**:
- Check logic broken
- Dependency still failing
- Cache not clearing

**Solution**:
1. Review check code
2. Verify dependency fixed
3. Clear health check cache
4. Restart service

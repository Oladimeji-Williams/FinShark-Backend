# FinShark Troubleshooting Guide

Common issues and their solutions.

## Build & Compilation Issues

### Issue: Build fails with "The type name 'init' only contains lower-cased ascii characters"

**Symptoms**:
```
CS8981: The type name 'init' only contains lower-cased ascii characters. Such names may become reserved for the language.
```

**Cause**: Migration file named with lowercase prefix conflicting with C# keyword.

**Solution**:
Delete the migration and recreate it with proper naming:

```powershell
cd src/FinShark.Persistence
dotnet ef migrations remove
dotnet ef migrations add InitialCreate  # Use PascalCase
```

---

### Issue: Build fails - Project references are broken

**Symptoms**:
```
The type or namespace name 'FinShark' does not exist
The type or namespace name could not be found
```

**Cause**: Project references not properly added to solution.

**Solution**:
```powershell
# Verify all projects are in solution
dotnet sln list

# If missing, add them
dotnet sln add src/FinShark.Domain/FinShark.Domain.csproj
dotnet sln add src/FinShark.Application/FinShark.Application.csproj
dotnet sln add src/FinShark.Persistence/FinShark.Persistence.csproj
dotnet sln add src/FinShark.Infrastructure/FinShark.Infrastructure.csproj
dotnet sln add src/FinShark.API/FinShark.API.csproj

# Rebuild
dotnet clean
dotnet restore
dotnet build
```

---

### Issue: NuGet package restore fails

**Symptoms**:
```
The remote certificate is invalid according to the validation procedure
NuGet restore failed
```

**Cause**: Network/certificate issues with NuGet.org.

**Solution**:
```powershell
# Clear NuGet cache
dotnet nuget locals all --clear

# Try restore again
dotnet restore

# If still fails, try explicit download
dotnet restore --source https://api.nuget.org/v3/index.json
```

---

## Database & Migrations Issues

### Issue: Migration fails - "Required member must be set in object initializer"

**Symptoms**:
```
CS9035: Required member 'AppDbContext.Stocks' must be set in the object initializer
```

**Cause**: AppDbContext has required properties that must be initialized.

**Solution**:
Ensure all DbSet properties are properly initialized:

```csharp
// FinShark.Persistence/AppDbContext.cs
public class AppDbContext : DbContext
{
    public required DbSet<Stock> Stocks { get; set; }
    public required DbSet<Comment> Comments { get; set; }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) 
        : base(options) { }
}
```

---

### Issue: Database connection fails - "Cannot open database"

**Symptoms**:
```
SqlException: Cannot open database "FinSharkDb" requested by the login
```

**Cause**: 
1. Database doesn't exist
2. Connection string is incorrect
3. SQL Server is not running

**Solution**:

**Check 1**: Verify SQL Server is running
```powershell
# Windows
Get-Service MSSQLSERVER | Start-Service

# Verify connection
sqlcmd -S localhost -E -Q "SELECT @@VERSION"
```

**Check 2**: Verify connection string in appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(local);Database=FinSharkDb;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

**Check 3**: Create database and run migrations
```powershell
cd src/FinShark.API
dotnet ef database update
```

---

### Issue: Migration Add/Update hangs or takes too long

**Symptoms**:
- Command seems to freeze
- No error message after 5+ minutes

**Cause**: 
1. Database lock
2. Network connectivity issue
3. SQL Server resource constraint

**Solution**:
```powershell
# Cancel current operation (Ctrl+C)

# List migrations to see current state
dotnet ef migrations list -p src/FinShark.Persistence -s src/FinShark.API

# Check database connectivity
sqlcmd -S localhost -E -Q "SELECT name FROM sys.databases;"

# Try again
dotnet ef migrations add YourMigration -p src/FinShark.Persistence -s src/FinShark.API
```

---

### Issue: Cannot drop database during migration

**Symptoms**:
```
Cannot drop database because it is currently in use
```

**Cause**: Another connection is using the database.

**Solution**:
```powershell
# Close VS Code and terminate dotnet processes
Get-Process dotnet | Stop-Process -Force

# Use SQL to kill connections
sqlcmd -S localhost -E << EOF
USE master;
ALTER DATABASE FinSharkDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE FinSharkDb;
GO
EOF

# Recreate
dotnet ef database update -p src/FinShark.Persistence -s src/FinShark.API
```

---

## Runtime Issues

### Issue: HTTPS certificate not trusted warning

**Symptoms**:
```
[WRN] The ASP.NET Core developer certificate is not trusted
```

**Cause**: Developer certificate hasn't been trusted in the system.

**Solution**:
```powershell
# Trust the certificate (may require admin)
dotnet dev-certs https --trust

# If that fails, clean and recreate
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

---

### Issue: Port already in use

**Symptoms**:
```
Address already in use
Unable to bind to http://localhost:5000
```

**Cause**: Another application is using the port.

**Solution**:
```powershell
# Find process using port 5000
netstat -ano | findstr :5000
# Example output: TCP    127.0.0.1:5000    0.0.0.0:0    LISTENING    12345

# Kill the process
Stop-Process -Id 12345 -Force

# Or change launch settings
# src/FinShark.API/Properties/launchSettings.json
{
  "applicationUrl": "https://localhost:5002;http://localhost:5001"
}
```

---

### Issue: Application throws "No suitable constructor found"

**Symptoms**:
```
InvalidOperationException: Unable to resolve service for type
ServiceCollection error: No suitable constructor
```

**Cause**: Dependency injection not properly configured.

**Solution**:
Register missing service in Program.cs:

```csharp
// src/FinShark.API/Program.cs
using FinShark.Application;
using FinShark.Infrastructure;
using FinShark.Persistence;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Add services
builder.Services
    .AddApplicationServices()           // From Application layer
    .AddPersistenceServices(builder.Configuration)  // From Persistence
    .AddInfrastructureServices();       // From Infrastructure

var app = builder.Build();
app.Run();
```

---

### Issue: Static files returning 404

**Symptoms**:
```
GET /static/file.js → 404 Not Found
```

**Cause**: Static files not configured or placed in wrong location.

**Solution**:
```csharp
// src/FinShark.API/Program.cs
var app = builder.Build();

// Add BEFORE routing
app.UseStaticFiles(); // Serves from wwwroot/

// Then configure routing
app.UseRouting();
app.MapControllers();

app.Run();
```

Place files in `src/FinShark.API/wwwroot/` directory.

---

## Testing Issues

### Issue: Tests fail - "Required member must be set"

**Symptoms**:
```
CS9035: Required member 'AppDbContext.Stocks' must be set
```

**Cause**: Test DbContext initialization missing required properties.

**Solution**:
```csharp
// Test setup - create in-memory DbContext properly
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseInMemoryDatabase("TestDb" + Guid.NewGuid())
    .Options;

using var context = new AppDbContext(options);

// Ensure navigation properties are initialized
context.Stocks = new List<Stock>();
context.Comments = new List<Comment>();
```

---

### Issue: Integration tests timeout

**Symptoms**:
```
TimeoutException: The operation was not completed within 30000 ms
```

**Cause**: 
1. Database setup taking too long
2. Circular dependencies
3. Network latency

**Solution**:
```csharp
[Fact]
public async Task TestName()
{
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
    
    var result = await handler.Handle(command, cts.Token);
    
    Assert.True(result);
}
```

---

### Issue: Tests pass locally but fail in CI/CD

**Symptoms**:
```
Tests pass: dotnet test
Tests fail: GitHub Actions/Azure Pipelines
```

**Cause**:
1. Database configuration differences
2. Path issues
3. Environment variables missing

**Solution**:
```powershell
# Run tests in CI mode
dotnet test --configuration Release --verbosity detailed

# Check environment
$env:ASPNETCORE_ENVIRONMENT = "Testing"
dotnet test

# Ensure appsettings.Testing.json exists
```

---

## Validation Issues

### Issue: Validation passes but endpoint still fails

**Symptoms**:
- CreateStockValidator passes validation
- But StocksController.CreateStock() still returns error

**Cause**: Validator not registered in DI container.

**Solution**:
```csharp
// FinShark.Application/ServiceCollectionExtensions.cs
public static IServiceCollection AddApplicationServices(
    this IServiceCollection services)
{
    services.AddMediatR(cfg => 
        cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));
    
    // Register validators
    services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
    
    return services;
}

// In Program.cs
builder.Services.AddApplicationServices();
```

---

### Issue: Custom validation rules not executing

**Symptoms**:
```csharp
RuleFor(x => x.Symbol)
    .Must(BeUniqueSymbol) // Never called?
    .WithMessage("Symbol already exists");
```

**Cause**: Custom rule has dependency that's not injected.

**Solution**:
```csharp
public class CreateStockValidator : AbstractValidator<CreateStockCommand>
{
    private readonly IStockRepository _stockRepository;

    public CreateStockValidator(IStockRepository stockRepository)
    {
        _stockRepository = stockRepository;

        RuleFor(x => x.Symbol)
            .NotEmpty()
            .MustAsync(BeUniqueSymbol) // Use async version
            .WithMessage("Symbol already exists");
    }

    private async Task<bool> BeUniqueSymbol(string symbol, CancellationToken ct)
    {
        var exists = await _stockRepository.SymbolExistsAsync(symbol);
        return !exists;
    }
}
```

---

## Performance Issues

### Issue: API responses are slow

**Symptoms**:
```
GET /api/stocks → Takes 5+ seconds
```

**Cause**:
1. N+1 problem (multiple queries)
2. Large dataset without pagination
3. Database indexes missing

**Solution**:

```csharp
// Use .Include() for related data
var stocks = await dbContext.Stocks
    .Include(s => s.Comments)  // Eager load
    .ToListAsync();

// Add pagination
var stocks = await dbContext.Stocks
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();

// Add database indexes
modelBuilder.Entity<Stock>()
    .HasIndex(s => s.Symbol)
    .IsUnique();
```

---

### Issue: Memory usage keeps increasing

**Symptoms**:
```
Application memory grows continuously
Eventually OutOfMemoryException
```

**Cause**: Entity Framework context not disposed properly.

**Solution**:
```csharp
// Use using statement
using (var context = new AppDbContext(options))
{
    var stock = await context.Stocks.FindAsync(id);
} // Disposed automatically

// Or use dependency injection (automatic)
public class StockRepository : IStockRepository
{
    private readonly AppDbContext _context;
    
    public StockRepository(AppDbContext context)
    {
        _context = context; // Disposed by DI container
    }
}
```

---

## Common Solutions Checklist

When troubleshooting:

- [ ] Check error message carefully - it usually tells you the problem
- [ ] Verify all project references are correct
- [ ] Ensure database connection string is correct
- [ ] Verify SQL Server is running and accessible
- [ ] Check that all services are registered in DI
- [ ] Review recent code changes
- [ ] Check logs in `bin/Debug/logs/` directory
- [ ] Try clean build: `dotnet clean && dotnet build`
- [ ] Restart VS Code and .NET runtime
- [ ] Check Stack Overflow for the exact error

---

## Getting Help

### 1. Check Logs
```powershell
# Serilog writes to: bin/Debug/logs/
Get-Content "bin/Debug/logs/*.txt" -Tail 50
```

### 2. Enable Detailed Logging
```csharp
// Program.cs
var builder = WebApplicationBuilder.CreateBuilder(args);
builder.Logging.SetMinimumLevel(LogLevel.Debug);
```

### 3. Enable Development Exception Page
```csharp
app.UseDeveloperExceptionPage(); // Development only
```

### 4. Use Debugger
```powershell
# Run with debugger
dotnet run --configuration Debug
```

In VS Code:
- Press `F5` to start debugging
- Set breakpoints
- Step through code

### 5. Community Resources
- **Stack Overflow**: Tag with `c#`, `asp.net-core`, `entity-framework`
- **GitHub Issues**: Check FinShark repo issues
- **Microsoft Docs**: https://docs.microsoft.com/en-us/dotnet/

---

## Reporting Issues

When asking for help, provide:

1. **Error message** - Full error text
2. **Stack trace** - Complete exception trace
3. **Steps to reproduce** - How to trigger the issue
4. **Environment** - OS, .NET version, SQL Server version
5. **Recent changes** - What you changed recently
6. **Logs** - Any relevant log files

Example issue report:
```
**Error**: CS9035: Required member 'AppDbContext.Stocks' must be set
**When**: Running dotnet test in FinShark.Tests project
**Environment**: Windows 10, .NET 10.0.1, SQL Server 2019
**Steps to reproduce**:
1. dotnet test src/FinShark.Tests
2. Test fails immediately

**Log output**:
[See error message above]

**Recent changes**: 
Added required keyword to AppDbContext properties
```

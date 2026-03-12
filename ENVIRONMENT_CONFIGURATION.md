# FinShark Environment Configuration Guide

Comprehensive guide to configuring FinShark for different environments.

## Environment Overview

| Environment | Purpose | Connection | Logging | SSL |
|------------|---------|-----------|---------|-----|
| **Development** | Local development | Local SQL Server | Verbose | Self-signed |
| **Testing** | Automated tests | In-Memory DB | Minimal | N/A |
| **Staging** | Pre-production | Remote SQL Server | Detailed | Real cert |
| **Production** | Live application | Production DB | Minimal (perf) | Real cert |

---

## Configuration Files

```
finshark-backend/
├── appsettings.json              # Base configuration
├── appsettings.Development.json  # Dev overrides
├── appsettings.Testing.json      # Test overrides
├── appsettings.Staging.json      # Staging overrides
├── appsettings.Production.json   # Production overrides
```

---

## appsettings.json (Base Configuration)

Default settings for all environments:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(local);Database=FinSharkDb;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "bin/Debug/logs/.txt",
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      }
    ]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5000"
      },
      "Https": {
        "Url": "https://localhost:5001",
        "Certificate": {
          "Path": null,
          "Password": null
        }
      }
    }
  },
  "AllowedHosts": "*",
  "AppSettings": {
    "ApiTitle": "FinShark Stock API",
    "ApiVersion": "1.0"
  }
}
```

---

## Development Configuration

**File**: `appsettings.Development.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(local);Database=FinSharkDb_Dev;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "Serilog": {
    "MinimumLevel": "Debug",
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
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information",
      "Microsoft.EntityFrameworkCore": "Debug",
      "Microsoft.EntityFrameworkCore.Database.Command": "Debug"
    }
  },
  "AppSettings": {
    "EnableSwagger": true,
    "EnableDetailedErrors": true,
    "CorsEnabled": true
  }
}
```

**Usage**:
```bash
# Automatically loads when ASPNETCORE_ENVIRONMENT=Development
dotnet run
```

**Key Features**:
- Verbose logging (Debug level)
- Local database with `_Dev` suffix
- Detailed error messages
- Swagger UI enabled
- Self-signed HTTPS certificate

---

## Testing Configuration

**File**: `appsettings.Testing.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=:memory:"
  },
  "Serilog": {
    "MinimumLevel": "Warning",
    "WriteTo": []
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Error"
    }
  },
  "AppSettings": {
    "EnableSwagger": false,
    "EnableDetailedErrors": false
  }
}
```

**Usage**:
```bash
# Set environment variable
$env:ASPNETCORE_ENVIRONMENT = "Testing"
dotnet test

# Or inline
dotnet test --configuration Debug /p:ASPNETCORE_ENVIRONMENT=Testing
```

**Key Features**:
- In-memory database (fast, no I/O)
- Logging disabled (performance)
- No detailed errors
- Swagger disabled
- Each test gets fresh DB

---

## Staging Configuration

**File**: `appsettings.Staging.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=staging-db.company.local;Database=FinSharkDb_Staging;User Id=sa;Password=${DB_PASSWORD};TrustServerCertificate=true;"
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "/var/logs/finshark/finshark-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      }
    ]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5000"
      },
      "Https": {
        "Url": "https://localhost:5001",
        "Certificate": {
          "Path": "/etc/ssl/certs/staging.pfx",
          "Password": "${CERT_PASSWORD}"
        }
      }
    }
  },
  "AppSettings": {
    "EnableSwagger": false,
    "EnableDetailedErrors": false,
    "CorsEnabled": true,
    "AllowedOrigins": [
      "https://staging-app.company.local"
    ]
  }
}
```

**Usage**:
```bash
# Push to staging server
git push staging main

# Server runs with:
$env:ASPNETCORE_ENVIRONMENT = "Staging"
dotnet run --configuration Release
```

**Key Features**:
- Remote database
- Secrets from environment variables
- Real SSL certificate
- Limited logging for performance
- Specific CORS origins

---

## Production Configuration

**File**: `appsettings.Production.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-db.company.local;Database=FinSharkDb_Prod;User Id=prod_user;Password=${DB_PASSWORD};TrustServerCertificate=false;Encrypt=true;"
  },
  "Serilog": {
    "MinimumLevel": "Warning",
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "/var/log/finshark/finshark-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      }
    ]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Error",
      "Microsoft.EntityFrameworkCore": "Error"
    }
  },
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://0.0.0.0:443",
        "Certificate": {
          "Path": "/etc/ssl/certs/finshark.pfx",
          "Password": "${CERT_PASSWORD}"
        }
      }
    }
  },
  "AllowedHosts": "api.finshark.com",
  "AppSettings": {
    "EnableSwagger": false,
    "EnableDetailedErrors": false,
    "CorsEnabled": true,
    "AllowedOrigins": [
      "https://finshark.com",
      "https://www.finshark.com"
    ],
    "SecurityHeaders": {
      "StrictTransportSecurity": "max-age=31536000; includeSubDomains",
      "X-Content-Type-Options": "nosniff",
      "X-Frame-Options": "DENY",
      "X-XSS-Protection": "1; mode=block"
    }
  }
}
```

**Deployment**:
```bash
# Build for production
dotnet publish -c Release -o ./publish

# Run on production server
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:ASPNETCORE_URLS = "https://0.0.0.0:443"
dotnet ./publish/FinShark.API.dll
```

**Key Features**:
- Minimal logging (file only)
- Remote production database
- Real SSL certificate (not self-signed)
- No Swagger UI
- Strict CORS
- Security headers
- 30-day log retention

---

## Environment Variables

Use environment variables for sensitive data:

```powershell
# Development
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:DB_PASSWORD = "local-password"

# Staging
$env:ASPNETCORE_ENVIRONMENT = "Staging"
$env:DB_PASSWORD = "staging-password"
$env:CERT_PASSWORD = "certificate-password"

# Production (use secrets manager in production)
$env:ASPNETCORE_ENVIRONMENT = "Production"
```

**In Program.cs**:
```csharp
var builder = WebApplicationBuilder.CreateBuilder(args);

// Automatically loaded from environment
var dbPassword = builder.Configuration["DB_PASSWORD"];
```

---

## Database Configuration by Environment

### Development

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(local);Database=FinSharkDb_Dev;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

**Setup**:
```powershell
# Create dev database
cd src/FinShark.API
dotnet ef database update
```

### Testing

In-memory database configured in tests:

```csharp
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseInMemoryDatabase("TestDb" + Guid.NewGuid())
    .Options;
```

### Staging/Production

Remote SQL Server:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-db.company.local;Database=FinSharkDb_Prod;User Id=sa;Password=${DB_PASSWORD};Encrypt=true;TrustServerCertificate=false;"
  }
}
```

---

## Logging Configuration by Environment

### Development - Verbose

```
[12:34:56.789] [DBG] Creating stock with symbol 'AAPL'
[12:34:56.890] [DBG] Validating stock data...
[12:34:56.891] [INF] Stock created successfully with ID 1
```

### Production - Minimal

```
[2026-03-12 12:34:56.789] [WRN] Slow database query: 2500ms
[2026-03-12 12:34:57.123] [ERR] Exception: NullReferenceException
```

---

## SSL/TLS Configuration

### Development (Self-Signed)

```powershell
# Create and trust certificate
dotnet dev-certs https --trust

# Run on HTTPS
dotnet run
```

### Staging/Production (Real Certificate)

```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://0.0.0.0:443",
        "Certificate": {
          "Path": "/etc/ssl/certs/finshark.pfx",
          "Password": "${CERT_PASSWORD}"
        }
      }
    }
  }
}
```

---

## CORS Configuration

### Development (Allow All)

```csharp
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", builder => {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
```

### Production (Restrict)

```csharp
var allowedOrigins = builder.Configuration.GetSection("AppSettings:AllowedOrigins")
    .Get<string[]>();

builder.Services.AddCors(options => {
    options.AddPolicy("RestrictedCors", builder => {
        builder.WithOrigins(allowedOrigins)
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});
```

---

## Configuration Provider Priority

(Higher priority overrides lower)

1. Command-line arguments: `dotnet run --key=value`
2. Environment variables: `$env:KEY=value`
3. appsettings.{Environment}.json
4. appsettings.json
5. Default values in code

Example:
```powershell
# This takes highest priority
$env:ConnectionStrings__DefaultConnection = "Server=override;..."
dotnet run
```

---

## Checking Current Configuration

```csharp
// In Program.cs
var config = builder.Configuration;
var connString = config.GetConnectionString("DefaultConnection");
var environment = builder.Environment.EnvironmentName;

Console.WriteLine($"Environment: {environment}");
Console.WriteLine($"Connection: {connString}");
```

Or from command line:
```powershell
# Check current environment
echo $env:ASPNETCORE_ENVIRONMENT

# Check all environment variables
Get-ChildItem env: | Where-Object {$_.Name -like "*ASPNET*"}
```

---

## Configuration Best Practices

✅ **Do This**:
- Use environment-specific files
- Store secrets in environment variables
- Use User Secrets in development
- Encrypt sensitive data in transit
- Document required environment variables

❌ **Never Do This**:
- Hardcode passwords in appsettings.json
- Commit secrets to git
- Share .pfx certificates in repositories
- Log sensitive data
- Use plaintext HTTP in production

---

## Command Reference

```bash
# Run with specific environment
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run

# Build for production
dotnet publish -c Release -o ./publish

# Run published app
dotnet ./publish/FinShark.API.dll

# Check configuration
dotnet run --help
```

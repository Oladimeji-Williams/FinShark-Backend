# Environment Configuration

FinShark reads configuration from:

1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. environment variables
4. `.env` in non-production environments

For database connectivity, persistence also explicitly prefers:

1. `FINSHARK_DB_CONNECTION`
2. `ConnectionStrings:DefaultConnection`

## Core Variables

### Hosting

- `ASPNETCORE_ENVIRONMENT`
- `ASPNETCORE_URLS`

Recommended local values:

```env
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=https://localhost:7235;http://localhost:5192
```

### Database

- `FINSHARK_DB_CONNECTION`
- `FINSHARK_USE_INMEMORY_DB`

Examples:

```env
FINSHARK_DB_CONNECTION=Server=(localdb)\mssqllocaldb;Database=FinSharkDb;Trusted_Connection=True;MultipleActiveResultSets=true
FINSHARK_USE_INMEMORY_DB=false
```

Notes:

- set `FINSHARK_USE_INMEMORY_DB=true` for test-style or ephemeral runs
- when in-memory mode is enabled, SQL Server is not used

### JWT

- `Jwt__Key`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__ExpiryInMinutes`

Production guidance:

- use a strong secret
- manage the secret outside source control
- keep issuer and audience aligned with deployed clients

### Client URL

- `AppSettings__ClientUrl`

Used for:

- generating email confirmation links

Example:

```env
AppSettings__ClientUrl=https://app.example.com
```

### Financial Modeling Prep

- `FMP__BaseUrl`
- `FMP__ApiKey`
- `FMP__TimeoutSeconds`

Example:

```env
FMP__BaseUrl=https://financialmodelingprep.com
FMP__ApiKey=your-real-key
FMP__TimeoutSeconds=30
```

### SMTP

- `Smtp__Provider`
- `Smtp__Host`
- `Smtp__Port`
- `Smtp__EnableSsl`
- `Smtp__UseDefaultCredentials`
- `Smtp__UserName`
- `Smtp__Password`
- `Smtp__FromEmail`
- `Smtp__FromName`
- `Smtp__TimeoutInMilliseconds`

Important behavior:

- if `Host` or `FromEmail` is missing, the app uses `NoOpEmailService`
- the health endpoint then reports a degraded status with SMTP warnings
- registration and resend-confirmation still succeed even if email delivery fails

### CORS

- `Cors__AllowedOrigins`
- `Cors__AllowAllMethods`
- `Cors__AllowAllHeaders`
- `Cors__AllowCredentials`

Production guidance:

- do not leave `AllowedOrigins=*` in production
- only enable credentials when using explicit origins

## Sample Development `.env`

```env
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=https://localhost:7235;http://localhost:5192
FINSHARK_DB_CONNECTION=Server=(localdb)\mssqllocaldb;Database=FinSharkDb;Trusted_Connection=True;MultipleActiveResultSets=true
Jwt__Key=replace-with-a-dev-secret-at-least-32-characters-long
Jwt__Issuer=FinShark
Jwt__Audience=FinSharkUsers
Jwt__ExpiryInMinutes=60
AppSettings__ClientUrl=https://localhost:7235
FMP__BaseUrl=https://financialmodelingprep.com
FMP__ApiKey=your-fmp-key
FMP__TimeoutSeconds=30
Smtp__Provider=Mailtrap
Smtp__Host=smtp.mailtrap.io
Smtp__Port=2525
Smtp__EnableSsl=true
Smtp__UseDefaultCredentials=false
Smtp__UserName=mailtrap-user
Smtp__Password=mailtrap-password
Smtp__FromEmail=no-reply@finshark.local
Smtp__FromName=FinShark
Smtp__TimeoutInMilliseconds=15000
Cors__AllowedOrigins=https://localhost:3001,http://localhost:3000
Cors__AllowAllMethods=true
Cors__AllowAllHeaders=true
```

## Production Checklist

- provide `FINSHARK_DB_CONNECTION`
- provide a strong `Jwt__Key`
- set `AppSettings__ClientUrl` to the deployed frontend
- provide real SMTP credentials
- provide `FMP__ApiKey`
- restrict `Cors__AllowedOrigins`
- run with `ASPNETCORE_ENVIRONMENT=Production`

## Notes About the Current Code

- `.env` loading is skipped in Production
- OpenAPI JSON is exposed only in Development
- the current code still sets `options.SignIn.RequireConfirmedEmail = false`, but login explicitly blocks unconfirmed users, so email confirmation is still required in practice

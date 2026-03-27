# Troubleshooting

## The API Will Not Start

Check:

- `Jwt__Key` is present
- a database source is configured
- your certificate trust is valid when using HTTPS locally

Common fixes:

```powershell
dotnet restore
dotnet build src/FinShark.API/FinShark.API.csproj
```

## `Connection string not found`

Provide one of:

- `FINSHARK_DB_CONNECTION`
- `ConnectionStrings:DefaultConnection`

Or run temporarily with:

```powershell
$env:FINSHARK_USE_INMEMORY_DB='true'
```

## Login Fails Even With Correct Password

Possible causes:

- email is not confirmed
- account is locked out after repeated failures
- JWT config changed between token issuance and validation

## `/api/health` Returns Degraded

The current health endpoint only checks SMTP configuration.

Typical causes:

- `Smtp__Host` missing
- `Smtp__FromEmail` missing
- credentials missing while `Smtp__UseDefaultCredentials=false`

## OpenAPI JSON Is Missing

Expected behavior:

- `GET /openapi/v1.json` is available only in Development

If you run in Production, that route is intentionally not exposed.

## CORS Failures

Check:

- `Cors__AllowedOrigins`
- whether credentials are enabled with explicit origins only

Do not rely on wildcard origins in production.

## FMP Quote Requests Fail

Check:

- `FMP__ApiKey`
- outbound internet access
- symbol validity

FMP-specific failures may include `fmpStatusCode` and `suggestion` in the API response.

## Email Confirmation Link Looks Wrong

Check:

- `AppSettings__ClientUrl`

That value is used when generating confirmation links.

## Integration Tests Behave Differently Than Local SQL Runs

That is expected if the tests are using:

```text
FINSHARK_USE_INMEMORY_DB=true
```

Integration tests use the in-memory provider through the custom web application factory.

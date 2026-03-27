# Health Checks

FinShark currently exposes one HTTP health endpoint.

## Endpoint

### `GET /api/health`

Returns:

- `200 OK`
- `ApiResponse<object>`

The payload contains:

- overall status
- SMTP configuration state
- SMTP warnings when configuration is incomplete

Example shape:

```json
{
  "success": true,
  "data": {
    "status": "Healthy",
    "smtp": {
      "provider": "Mailtrap",
      "configured": true,
      "host": "smtp.mailtrap.io",
      "fromEmail": "no-reply@finshark.local",
      "useDefaultCredentials": false,
      "warnings": []
    }
  },
  "message": "API is healthy",
  "errors": null
}
```

## What It Checks

The current implementation checks SMTP configuration completeness:

- host present
- from address present
- credentials present when `UseDefaultCredentials=false`

## What It Does Not Check

To keep the document honest, the current endpoint does not verify:

- SQL Server connectivity
- FMP availability
- disk capacity
- queue backlogs
- SMTP round-trip delivery

## Operational Interpretation

- `Healthy`: SMTP config looks complete
- `Degraded`: API is running, but email delivery is likely unavailable or incomplete

## Recommended Usage

- use `/api/health` for a lightweight liveness check
- pair it with deployment-specific SQL Server and external dependency monitoring

## Related Files

- [PipelineConfiguration.cs](/c:/Users/OladimejiWilliams/Desktop/Software%20Engineering/FinShark/finshark-backend/src/FinShark.API/Configuration/PipelineConfiguration.cs)
- [health-check.ps1](/c:/Users/OladimejiWilliams/Desktop/Software%20Engineering/FinShark/finshark-backend/health-check.ps1)

# Logging and Monitoring

FinShark uses Serilog as the host logger.

## Current Logging Sinks

Configured in `Program.cs`:

- console
- rolling file sink at `logs/finshark-.txt`

File output uses daily rolling intervals.

## Log Levels

Defaults come from:

- `appsettings.json`
- `appsettings.Development.json`
- `appsettings.Production.json`
- environment variable overrides

Current examples:

- base config defaults to `Information`
- Development raises verbosity
- Production lowers verbosity to `Warning`

## What Gets Logged

Examples of current logging behavior:

- startup and shutdown events
- handler-level business operations
- validation and domain warnings
- unexpected exceptions
- JWT authentication failures
- SMTP send attempts and failures
- FMP lookup activity

## Monitoring Surfaces

Current built-in monitoring surfaces:

- application logs
- `/api/health`
- OpenAPI JSON in Development for contract inspection

## Honest Current State

The application does not currently include:

- Application Insights
- Prometheus metrics
- distributed tracing
- log shipping configuration
- readiness probes for SQL Server or FMP

## Production Recommendations

- ship Serilog output to your centralized logging platform
- restrict access to log files
- monitor 401, 403, 409, and 500 trends
- alert on repeated SMTP or FMP failures
- pair `/api/health` with infrastructure-level database monitoring

## Useful Paths

- log file pattern: `logs/finshark-.txt`
- health endpoint: `/api/health`

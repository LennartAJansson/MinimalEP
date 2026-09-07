# P2 Operational Readiness

## Status
- Implemented in code: yes
- Last validated: build green, tests 37/37
- Remaining operational step: connect metrics/SLOs to platform dashboards and alert rules in target environment

## Approved Policy
- Refresh token retention: 30 days
- Cleanup cadence: hourly background job
- Scope: inactive tokens only (revoked tokens or expired tokens)

## Configuration
Section: `RefreshTokenMaintenance`
- `RetentionDays`: default `30`
- `CleanupIntervalMinutes`: default `60`

## Metrics
The service publishes the following OpenTelemetry metrics through meter `MinimalEP.RefreshTokenMaintenance`:
- `refresh_token_total` (gauge)
- `refresh_token_cleanup_deleted_total` (counter)
- `refresh_token_cleanup_duration_ms` (histogram)

## Initial SLO Targets
- API availability: 99.9%
- Auth endpoint p95 latency: < 250 ms
- Database readiness success ratio: >= 99.9%
- 5xx error ratio: < 0.5%

## Dashboard and Alert Recommendations
- Track p50/p95/p99 latency for `/auth/login`, `/auth/refresh`, `/auth/register`
- Track cleanup deletions/hour and cleanup duration trends
- Alert on readiness failures and sustained 5xx increases
- Alert on abnormal refresh-token table growth (gauge trend)

## Runbook (Auth and DB Pressure)
1. Verify `/health/ready` and recent SQL connectivity signals.
2. Inspect auth failure trends (401/403/409) and lockout spikes.
3. Review refresh-token cleanup metrics for stalling or zero deletions over expected windows.
4. If token-family reuse/concurrency warnings spike, investigate possible token replay and revoke affected families.
5. If DB pressure is elevated, reduce optional workload, inspect slow queries, and validate index health.
6. Confirm trace correlation using `TraceId` across request logs and SQL spans before closing incident.

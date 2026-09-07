# MinimalEP Action Plan for Future Change Work

## Purpose
This plan consolidates future improvements for MinimalEP based on the current solution state, existing architecture review, and observed documentation drift. It is intended as a living roadmap for secure, stable, and measurable evolution.

## Current Baseline
- Build is stable.
- Automated tests are green (37/37).
- Core architecture patterns are consistent (vertical slices, endpoint auto-registration, validation filter, CQRS-style EF write + Dapper read split).

## Implementation Status Snapshot

### Completed
- P1.1 Workload domain transitions are implemented (`StartNew`, `StopAt`, `UpdateDetails`) and used by handlers.
- P1.2 `Result<T>` is expanded with explicit error variants and stable machine-readable error codes.
- P1.3 Documentation sync mechanism is established (`Last Verified` section + PR checklist).
- P2.4 Refresh-token retention policy is implemented (30 days) with hourly background cleanup.
- P2.5 `TimeProvider` is adopted in auth token flows and audit/soft-delete timestamps.
- P2.6 Operational readiness baseline is documented with metrics/SLO/runbook in `docs/p2-operational-readiness.md`.

### Outstanding
- P3.7 Production-like load testing campaign (concurrent clients, p95/p99, SQL plans).
- P3.8 Re-evaluate endpoint reflection/registration strategy only if measurements justify change.
- P3.9 Introduce thin read models only if profiling identifies a hotspot.
- Cross-cutting: continue expanding regression coverage as new behavior is introduced.
- Cross-cutting: enforce migration rollout notes and hardening checklist usage in every relevant PR.

---

## Priority Model
- **P1 (Near-term):** Security, correctness, and contract clarity.
- **P2 (Mid-term):** Reliability, operability, maintainability.
- **P3 (Long-term):** Scalability optimization and architecture refinements after measurement.

---

## P1 — Security, Domain Integrity, and API Contract Improvements

### 1) Strengthen workload domain transitions
**Why:** Critical business invariant should not rely only on handlers.

**Actions:**
- Introduce domain methods on `Workload` for allowed state transitions (`Start`, `Stop`, editable-field updates).
- Restrict invalid state combinations at domain level in addition to validation and DB constraints.
- Keep `Stop` ownership to `StopWorkload` flow only.

**Done when:**
- Domain methods enforce transitions.
- Handlers use domain methods instead of direct mutable setters for state transitions.
- Tests cover valid/invalid transitions.

---

### 2) Expand `Result<T>` error model
**Why:** Current result variants are limited for explicit auth/validation/domain error semantics.

**Actions:**
- Add stable error-code capable result variants (for example validation, unauthorized, forbidden, domain conflict with code).
- Standardize endpoint-to-HTTP mapping for all result variants.
- Ensure Problem Details payloads expose stable machine-readable codes.

**Done when:**
- Common error taxonomy exists.
- Endpoint mappings are consistent and centrally verifiable.
- Integration tests assert error shape and status mapping.

---

### 3) Keep architecture documentation in sync with code
**Why:** Review document currently underreports test count and may drift over time.

**Actions:**
- Update `docs/architecture-and-technical-review.md` to reflect current baseline (37 tests).
- Add a lightweight “last-verified” section (build status, test count, date, verifier).
- Add PR checklist item requiring docs update for architecture-impacting changes.

**Done when:**
- Status sections match repository reality.
- PR process prevents stale architecture claims.

---

## P2 — Reliability, Operability, and Governance

### 4) Define refresh token retention and cleanup policy
**Why:** Security data lifecycle is deferred and needs explicit policy before cleanup automation.

**Actions:**
- Define retention period by compliance/security requirements.
- Define revocation/audit requirements and legal hold exceptions.
- Implement scheduled cleanup only after policy approval.
- Add metrics for token table growth, cleanup duration, and deletion volume.

**Done when:**
- Policy is approved and documented.
- Cleanup job implemented with safe guards and observability.
- Tests verify non-active token cleanup without removing required audit data.

---

### 5) Introduce `TimeProvider` for deterministic time logic
**Why:** Token lifetime/audit/time-sensitive behavior is harder to test deterministically.

**Actions:**
- Inject `TimeProvider` in auth token flows, interceptor time stamps, and other time-based logic.
- Remove direct dependence on `DateTimeOffset.UtcNow` in domain/application paths where test determinism matters.

**Done when:**
- Time-based tests use fake/frozen time.
- Flaky time-sensitive tests are eliminated.

---

### 6) Strengthen operational readiness and SLO-driven monitoring
**Why:** Existing observability foundation should evolve into actionable operations.

**Actions:**
- Define SLOs (availability, auth latency, DB latency, error rate).
- Add dashboards/alerts for p95/p99, saturation, retry/concurrency failures.
- Verify correlation flow (`TraceId`) across HTTP, SQL, and logs for incident triage.
- Add runbook for auth incidents and DB pressure events.

**Done when:**
- SLOs, dashboards, and alert thresholds are documented and active.
- Incident runbook is reviewed and test-exercised.

---

## P3 — Scalability and Architecture Refinement (Measurement-Driven)

### 7) Execute production-like load testing campaign
**Why:** Current performance is improved, but needs validation under realistic concurrent load.

**Actions:**
- Define representative workloads (auth mix, CRUD mix, workload start/stop contention).
- Run controlled tests with larger data volumes and concurrent clients.
- Collect p50/p95/p99 latency, throughput, SQL plans, and resource utilization.
- Compare with baseline and establish regression budgets.

**Done when:**
- Repeatable load test suite exists.
- Capacity profile and bottleneck list are documented.
- Performance gates are integrated into release criteria.

---

### 8) Re-evaluate endpoint registration reflection approach (only if justified)
**Why:** Current approach is acceptable; change should be evidence-based.

**Actions:**
- Measure startup time and DI registration overhead in realistic deployment mode.
- Keep current reflection/scan model unless measurable maintenance/performance issue appears.

**Done when:**
- Decision log captures measured evidence and chosen approach.

---

### 9) Re-evaluate thin read models (only if profiler indicates hotspot)
**Why:** Current pagination and repository profile are healthy; avoid speculative complexity.

**Actions:**
- Monitor profile data in new load campaign.
- Introduce slice-specific thinner DTO read paths only for proven hotspots.

**Done when:**
- Any read model refactor is tied to measured gains.

---

## Cross-Cutting Quality Work

### 10) Expand regression suite in targeted areas
**Focus areas:**
- Authorization negative/positive matrices for new endpoints.
- Concurrency conflict handling (rowversion) for all editable slices.
- Problem Details contract tests (error code + trace correlation fields).
- Cancellation propagation tests for new Dapper query paths.

**Done when:**
- New behavior ships with corresponding automated coverage.
- Critical invariants are tested at both application and database constraint levels.

---

### 11) Change management and release safety
**Actions:**
- Add a mandatory “Architecture impact” section in PR template.
- Require migration rollout notes for DB schema changes.
- Maintain a rolling hardening checklist for security-sensitive features (auth, roles, refresh tokens).

**Done when:**
- Governance checks are part of normal delivery flow.

---

## Suggested Delivery Sequence
1. Done: P1.1 Workload domain methods
2. Done: P1.2 Extended `Result<T>` + error contract
3. Done: P1.3 Documentation sync mechanism
4. Done: P2.4 Refresh token lifecycle policy + implementation
5. Done: P2.5 `TimeProvider` adoption
6. Done: P2.6 SLO/metrics/runbook baseline
7. Next: P3.7 Production-like load campaign
8. Next: P3.8/P3.9 Measurement-gated architecture refinements
9. Ongoing: cross-cutting test/governance expansion with each change

---

## Success Criteria for the Full Action Plan
- No unresolved high-severity security/integrity gaps.
- Stable, explicit API error contracts with regression coverage.
- Documentation continuously aligned with implemented reality.
- Measured scalability decisions (no premature optimization).
- Operational readiness backed by SLOs, observability, and tested incident procedures.

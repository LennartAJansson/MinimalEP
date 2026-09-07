# Architectural and Technical Review of MinimalEP

## Summary

MinimalEP has a clear and pedagogical foundation: .NET 10 Minimal API, vertical slices, explicit handlers, FluentValidation, `Result<T>`, `TypedResults`, EF Core for writes and Dapper for reads. The solution builds without errors.

The original P0/P1 risks concerning authorization boundaries, data invariants, token rotation and atomic account operations have been addressed. Phases 1–4 are complete, and optimistic concurrency has subsequently been introduced for editable entities. The remaining items are primarily need- or policy-dependent improvements, not known critical defects.

Repository performance has been profiled and benchmarked before and after Phase 3. The results are presented further below; production-like load testing with larger data volumes and concurrent clients remains.

## Last Verified

- Build status: success (`MinimalEP.slnx`)
- Test status: 37/37 green (`MinimalEP.Tests`)
- Verified on: 2026-09-07
- Verified by: GitHub Copilot

## Implementation Status

Phase 1 is implemented in code:

- employee and customer management requires `AdminOrAbove`; `/me` remains self-service
- the standalone `AddEmployee` slice has been removed; account and Employee are created together
- the generic workload update can no longer modify `Stop`
- a filtered unique database constraint protects against multiple open workloads per employee
- public registration assigns only `User`; explicit SuperAdmin bootstrap is disabled by default and works only against an empty installation
- account, role and Employee operations use transactions and check Identity results
- the last SuperAdmin and self-demotion are protected
- refresh tokens use a token family and rowversion for reuse and concurrency protection
- login uses Identity lockout and auth endpoints have rate limiting
- central exception handling with Problem Details is enabled

The `HardenPhaseOne` migration must be applied in each environment. Phase 1's core authorization, repository and concurrency rules are regression-verified by the Phase 2 test suite.

Phase 2 is implemented:

- `MinimalEP.Tests` contains an HTTP-based authorization matrix for anonymous users, User, Admin and SuperAdmin
- SQL Server integration tests verify repository soft delete, unique open workload and refresh-token rowversion
- bootstrap and JWT settings use typed options with startup validation
- database startup migration is controlled by `DatabaseOptions`
- `/health/live` and `/health/ready` are available without authentication; readiness checks SQL Server
- OpenTelemetry collects HTTP, HttpClient and SQL tracing as well as HTTP metrics, and uses OTLP when `OTEL_EXPORTER_OTLP_ENDPOINT` is set
- security events are logged in structured form without raw tokens or email addresses
- EF Core sensitive-data logging has been removed
- refresh-token maintenance is policy-driven via typed options (30-day retention, hourly cleanup)
- refresh-token maintenance metrics are exported (`refresh_token_total`, `refresh_token_cleanup_deleted_total`, `refresh_token_cleanup_duration_ms`)
- auth token flows and audit/soft-delete timestamps are TimeProvider-based for deterministic behavior

Phases 3 and 4 are implemented. In addition, optimistic concurrency has been introduced:

- `Customer`, `Employee` and `Workload` use SQL Server `rowversion`
- read responses expose the concurrency token as a Base64-encoded `byte[]`
- update, `/me` and workload stop require the client's last-read `RowVersion`
- stale writes return `409 Conflict` and instruct the client to re-read the resource
- the `AddEditableEntityConcurrency` migration must be applied in each environment
- a SQL Server integration test verifies that a stale customer update is rejected
- the solution build and all 37 tests are green

## Scope and Method

The review covers:

- project and package configuration
- API startup and endpoint registration
- vertical slices and core abstractions
- authentication, authorization and token flows
- EF Core configuration, Dapper repositories and soft delete
- validation, error handling, observability and testability
- static performance and scalability risks
- occurrence of magic strings and magic values

Verified at present:

- the entire solution builds without errors
- `MinimalEP.Tests` contains 37 green tests
- a repository baseline and follow-up measurement have been performed with BenchmarkDotNet and CPU profiling
- production-like load measurement is not yet included

## Strengths and Advantages

### 1. Clear vertical-slice structure

Use cases are separated into request, response, validator, mapping, handler and endpoint. This provides high local cohesion and makes changes easier to isolate than in a traditional horizontal controller/service structure.

### 2. Small, explicit handlers

Handlers generally have a clear responsibility and use `IRequestHandler<TRequest, TResponse>`. This simplifies testing and makes application flows easy to follow.

### 3. Consistent HTTP mapping

Endpoints use `TypedResults` and map `Result<T>` explicitly to HTTP results. This reduces the risk of unintended status codes and keeps the HTTP layer separate from the application logic.

### 4. Central validation pipeline

`ValidationFilter<TRequest>` avoids repeated validation code in each endpoint. The FluentValidation rules live close to each use case.

### 5. Solid foundation for identity and tokens

- ASP.NET Core Identity is used instead of custom password handling.
- Refresh tokens are stored hashed with SHA-256.
- Access tokens validate issuer, audience, lifetime and signing key.
- `MapInboundClaims = false` is used consistently.
- Roles are already centralized in `Roles`.

### 6. Secure ownership control for multiple workload flows

`GetWorkload`, `GetWorkloads`, `StopWorkload` and `DeleteWorkload` restrict regular users to their own records and return 404 on unauthorized access. This reduces information leakage and BOLA/IDOR risk.

### 7. Separate read and write strategies

Dapper is used for projected reads while EF Core is used for tracked writes and interceptor-based audit/soft delete. This is a pragmatic CQRS-like split without unnecessary infrastructure.

### 8. Solid database foundation

- UUID v7 provides better index locality than random UUIDs.
- SQL queries are parameterized.
- `DbContext` is pooled.
- Configurations live in separate `IEntityTypeConfiguration<T>` classes.
- `Address` is modeled as an owned type.
- The refresh-token hash has a unique index.

## Preliminary Prioritization

| Rank | Priority | Finding | Type | Assessment |
|---:|:---:|---|---|---|
| 1 | P0 | Generic employee endpoints lack role/resource authorization | Security/data integrity | Verified in code |
| 2 | P0 | `UpdateWorkload` can set or clear `Stop` and bypass the punch-clock invariant | Domain/data integrity | Verified in code |
| 3 | P0 | The first public registration can become SuperAdmin; the check is race-prone | Security | Verified in code |
| 4 | P1 | Identity user, roles and Employee are not saved atomically | Data integrity | Verified in code |
| 5 | P1 | A unique open workload is not enforced by the database | Concurrency/data integrity | Verified in code |
| 6 | P1 | Refresh-token rotation lacks concurrency protection and complete reuse handling | Security/concurrency | Verified in code |
| 7 | P1 | Login/register/refresh lack rate limiting and login does not use the lockout flow | Security | Verified in code |
| 8 | P1 | The customer flow has inconsistent unique email + soft delete and update collisions | Data integrity | Verified in code |
| 9 | P1 | Generic customer endpoints are open to all authenticated users | Authorization | Verified; the desired policy must be decided |
| 10 | P1 | Role changes are multi-step operations without rollback and Identity results are ignored | Data integrity/security | Verified in code |
| 11 | P1 | No central exception handling/Problem Details and no structured application logging | Operability | Verified in code |
| 12 | P1 | No automated tests exist in the solution | Quality | Verified in the solution |
| 13 | P2 | List endpoints lack pagination and materialize the entire result set | Performance | Static scalability risk |
| 14 | P2 | Dapper calls do not propagate the `CancellationToken` | Resource management | Verified in code |
| 15 | P2 | `StartWorkload` fetches all of the user's workloads for an existence check | Performance | Static scalability risk |
| 16 | P2 | Dapper joins do not filter soft-deleted Customer/Employee | Correctness | Verified in code |
| 17 | P2 | JWT/configuration uses string keys, null-forgiving and runtime parsing without startup validation | Configuration | Verified in code |
| 18 | P2 | Database migration runs automatically on every application start | Operations/deployment | Design risk |
| 19 | P2 | The endpoint/validator binding relies on namespace and repeated reflection | Maintenance/startup time | Verified; performance impact not measured |
| 20 | P2 | The database model and API lack optimistic concurrency for editable entities | Concurrency | Resolved with `rowversion`, API token and 409 handling |
| 21 | P2 | Validation limits and database column lengths are duplicated and partly inconsistent | Correctness/magic values | Verified in code |
| 22 | P3 | `Created` headers do not point to the actual versioned API routes | API quality | Verified in code |
| 23 | P3 | `SELECT *` and SQL/column names are hardcoded | Maintenance/magic strings | Verified in code |
| 24 | P3 | Time is taken directly from static clocks | Testability/magic values | Verified in code |
| 25 | P3 | Comments, language and formatting are inconsistent | Maintenance | Verified in code |

## Detailed Findings

### F-01 — Employee endpoints lack sufficient authorization (P0)

`Program.cs:59-62` only requires the user to be authenticated. `GetEmployeesEndpoint.cs:11`, `AddEmployeeEndpoint.cs:11`, `UpdateEmployeeEndpoint.cs:11` and `DeleteEmployeeEndpoint.cs:11` apply no administrative policy, and the handlers perform no resource check.

Consequences:

- a regular user can list all employees and their profile data
- a regular user can modify or soft-delete other employees
- `AddEmployee` can create an `Employee` without a corresponding `ApplicationUser`, despite the system's stated invariant `ApplicationUser.Id == Employee.Id`

Recommendation:

- restrict administrative employee routes with a central policy constant, e.g. `AuthorizationPolicies.AdminOrAbove`
- keep `/me` as a separate self-service slice
- retire or redefine `AddEmployee`; account + employee should be created through a single atomic provisioning flow
- add integration tests for User/Admin/SuperAdmin per route

### F-02 — `UpdateWorkload` bypasses the punch-clock model (P0)

`UpdateWorkloadRequest.cs:3` exposes both `Start` and `Stop`, and `UpdateWorkloadMapping.cs:11-13` writes both. A regular user can therefore reopen a closed workload by sending `Stop = null`, or close it without `StopWorkload`.

This contradicts the domain rule that `StopWorkload` is the only slice allowed to set `Stop`, and it can also bypass the at-most-one-open-workload check.

Recommendation:

- remove `Stop` from `UpdateWorkloadRequest`
- let the generic update only modify allowed metadata, e.g. comments
- define a separate use case if the start time must be correctable, likely only for administrators
- add domain tests showing that a closed workload cannot be reopened via the generic update

### F-03 — SuperAdmin bootstrap is public and race-prone (P0)

`RegisterEndpoint` is anonymous. `RegisterHandler.cs:41-44` counts users after creation and makes the user that sees count 1 SuperAdmin/Admin/User.

Risks:

- the first external client can take the SuperAdmin role in an empty installation
- parallel first registrations make the bootstrap outcome timing-dependent
- `Count()` is synchronous against the database

Recommendation:

- move bootstrap to deployment/seeding with a secret or an explicit one-time command
- disable public self-registration if the product requirement is that admins create employees
- if self-registration should exist: always assign the lowest role and separate bootstrap entirely

### F-04 — Account, roles and employee record are not saved atomically (P1)

`RegisterHandler.cs:31-66` and `CreateEmployeeAccountHandler.cs:39-67` perform several Identity and EF operations without a shared transaction. If role assignment or the employee save fails, an orphaned Identity user may be left behind. If Identity succeeds but Employee fails, the user cannot sign in correctly.

In addition, the result of `AddToRolesAsync`/`AddToRoleAsync` is not checked.

Recommendation:

- introduce an application service/unit of work that uses the same `ApplicationDbContext` transaction for the entire operation
- check every `IdentityResult`
- roll back or compensate deterministically on failure
- catch uniqueness conflicts and map them to stable domain/HTTP results

### F-05 — Unique open workload lacks database protection (P1)

`StartWorkloadHandler.cs:18-25` performs a check-then-insert. Two concurrent requests can both see that no open record exists and then each create one.

Recommendation:

- create a filtered unique index for `EmployeeId` where `Stop IS NULL AND Deleted IS NULL`
- replace fetching all records with `HasOpenWorkloadAsync`/`EXISTS`
- map the unique-constraint error to 409 Conflict

### F-06 — Refresh-token rotation is race-prone (P1)

`RefreshTokenHandler.cs:67-95` reads, verifies, revokes and creates a replacement without a concurrency token or conditional update. Two concurrent requests with the same token can both manage to rotate it.

Moreover, the comment about reuse detection only amounts to the presented token being denied; no token family or all subsequent tokens are revoked on suspected reuse.

Recommendation:

- add optimistic concurrency/rowversion or an atomic conditional update
- model a token family/session and revoke the family on reuse
- log the security event without logging the raw token
- consider an index for active tokens per user/session and a cleanup strategy

### F-07 — Brute-force protection is missing (P1)

`LoginHandler.cs:22-24` uses `CheckPasswordAsync`, which does not drive Identity lockout. No rate limiter is registered for login, register or refresh.

Recommendation:

- use `SignInManager.CheckPasswordSignInAsync(..., lockoutOnFailure: true)` or an equivalent explicit lockout flow
- add separate rate-limit policies for auth endpoints
- return the same external error response for an unknown user and a wrong password
- log aggregated failures without PII/secrets

### F-08 — Customer email and soft delete are inconsistent (P1)

`CustomerConfiguration.cs:28` creates an unconditional unique index on email. `CustomerRepository.cs:45`, on the other hand, checks only active customers. After a soft delete, the application check approves reuse, but the database rejects the insert.

`UpdateCustomerHandler` does not check email uniqueness at all. This can produce unhandled database errors instead of 409.

Recommendation:

- decide whether email should be reusable after soft delete
- use a filtered unique index if the answer is yes; otherwise also check soft-deleted records
- normalize email consistently
- handle competing inserts/updates via constraint + exception mapping, not only a pre-check

### F-09 — Customer endpoints lack an administrative policy (P1)

All authenticated users can create, read, modify and delete customers because the endpoints only inherit the group's general authorization. `ToDo.md`, however, describes the scenario that an admin creates customers.

Recommendation:

- establish an access matrix
- add `AdminOrAbove` to writes if that is the intent
- determine whether regular users should be able to read all customers, a limited projection, or only customers linked to their own workloads

### F-10 — Role changes can leave an inconsistent or locked system (P1)

`AssignRoleHandler.cs:36-39` first removes all roles and then adds a new one, without a transaction or checking `IdentityResult`. A failure in step two leaves the user without a role. A SuperAdmin can also demote themselves or the last SuperAdmin user.

Recommendation:

- check all Identity results
- make the role change atomic
- protect the last SuperAdmin and consider forbidding self-demotion
- log role changes as a security audit event

### F-11 — Central error handling and observability are missing (P1)

`Program.cs` registers no `AddProblemDetails`, `UseExceptionHandler`, health checks or application-specific structured logging. Database/Identity errors therefore risk becoming generic 500 responses without a stable error model or sufficient correlation data.

Recommendation:

- introduce central exception mapping to RFC 9457 Problem Details
- include a correlation/trace id in the error response and logs
- add health/readiness checks for SQL Server
- instrument auth, database latency, request latency and errors with OpenTelemetry or equivalent
- avoid PII, JWT and refresh tokens in logs

### F-12 — Automated tests are missing (P1)

The solution contains no test project. The highest-risk rules are at the same time authorization, concurrency and multi-step transactions, which are difficult to secure with manual testing.

Recommended first test portfolio:

1. integration tests for all endpoint policies and ownership rules
2. integration tests for register/provisioning and rollback
3. a concurrency test for starting a workload
4. a concurrency test for refresh-token rotation
5. repository tests for soft delete and unique email
6. unit tests for validators and mappings
7. architecture tests for dependency direction and slice conventions

### F-13 — Listings lack pagination (P2, static scalability risk)

`CustomerRepository.GetAllAsync` (`CustomerRepository.cs:33-38`), `EmployeeRepository.GetAllAsync` (`EmployeeRepository.cs:54-59`) and `WorkloadRepository.GetAllAsync` (`WorkloadRepository.cs:62-68`) read and materialize entire tables.

The risks are increasing database I/O, memory allocation, serialization time and large responses. This is likely a future bottleneck but is not profiler-verified.

Recommendation:

- use mandatory, capped pagination
- prefer keyset/cursor pagination for large and changing tables
- define a stable sort order, e.g. `(Created, Id)` or `(Start, Id)`
- return thin DTO projections
- measure with realistic row counts before and after

### F-14 — Dapper calls ignore cancellation (P2)

Repository methods accept a `CancellationToken`, but the Dapper calls do not pass the token along. Cancelled HTTP requests can therefore continue to burden SQL Server and the connection pool.

Recommendation:

- use `CommandDefinition` with `cancellationToken`
- introduce a central command timeout via typed options
- add an integration test where a cancelled request cancels the database operation

### F-15 — The open-workload check reads too much (P2, static scalability risk)

`StartWorkloadHandler.cs:18-20` fetches all of the user's workloads and then runs `Any` in memory. The cost grows with the entire history.

Recommendation:

- introduce `HasOpenWorkloadAsync(employeeId, ct)` with SQL `EXISTS`
- support the query with the filtered unique index from F-05
- benchmark or profile with realistic history

### F-16 — Dapper joins do not respect soft delete for related entities (P2)

`WorkloadRepository.BaseSelect` filters `w.Deleted`, but not `c.Deleted` or `e.Deleted`. Workloads can therefore be returned with a soft-deleted customer or employee. Dapper is not covered by EF's query filters.

Recommendation:

- add explicit filters for all soft-deleted tables in hand-written SQL
- define the desired history behavior: hide the record, show a snapshot, or show the relation marked as deleted
- test that the Dapper and EF paths produce the same semantics

### F-17 — JWT configuration is string-based and validated late (P2)

`JwtTokenService.cs:17-36`, `AuthExtensions.cs:34-58`, `LoginHandler.cs:34` and `RefreshTokenHandler.cs:25-26,79` use repeated key strings, null-forgiving and `int.Parse`/`double.Parse` at runtime. The production configuration also has an empty key in `appsettings.json`.

Recommendation:

- create `JwtOptions` with constants for the section name
- bind with the Options pattern
- use `ValidateDataAnnotations`, a custom validator and `ValidateOnStart`
- represent durations as validated integers or `TimeSpan`
- check the minimum key length and forbid empty issuer/audience
- inject `IOptions<JwtOptions>` into the token flows

### F-18 — Automatic migration at startup is an operational risk (P2)

`Program.cs:36-37` migrates and seeds before the app starts. That is convenient locally, but in production multiple replicas can compete, startup can be blocked and the application identity needs DDL permissions.

Recommendation:

- run migration as a separate deployment job in production
- keep automatic migration only behind explicit development/demo configuration
- log and verify the result of role seeding; the `CreateAsync` result is ignored today

### F-19 — Endpoint registration is convention-sensitive (P2)

`EndpointExtensions.cs:58-89` scans all types again for each endpoint and binds a handler to an endpoint by the fact that they happen to be in the same namespace. This is brittle with multiple handlers/endpoints in the same namespace and causes unnecessary startup reflection.

Recommendation:

- make the request type explicit in the endpoint contract/metadata
- build the registration metadata once at startup or use source generation
- create a startup/architecture test that verifies exactly one intended handler per endpoint

This is primarily a maintenance risk. The startup cost must be measured before it is prioritized as a performance defect.

### F-20 — Optimistic concurrency is missing (P2)

Customer, Employee and Workload have no rowversion/concurrency token. Concurrent updates become last-write-wins and can silently overwrite each other.

Recommendation:

- add `rowversion` to editable entities
- expose an ETag or version field in the API
- require `If-Match` or an equivalent version check on update/delete
- return 409/412 on conflict

### F-21 — Validation and database limits are duplicated (P2)

Examples:

- the age limit `16..100` is repeated in several validators
- name, address, phone and position have database lengths but validators lack the corresponding `MaximumLength`
- Customer email is 255 characters while Employee email is 256
- workload comments have a database limit of 1000 but should be validated before save

The consequence can be database errors instead of 400, and gradual drift between slices.

Recommendation:

- create domain-level constraints, e.g. `EmployeeConstraints`
- reuse the constraints in validators and EF configuration
- avoid centralizing business rules that share no common meaning; centralize only genuinely shared invariants

### F-22 — Incorrect or incomplete `Location` headers (P3)

Created responses use e.g. `/customers/{id}` and `/workloads/{id}`, while the actual route lives under `/api/v{version:apiVersion}`. Register also points to `/auth/{userId}`, where no corresponding GET route exists.

Recommendation:

- name the GET endpoints and use `CreatedAtRoute`
- include the current API version
- only use Location for a resource that can actually be retrieved

### F-23 — Hardcoded SQL and `SELECT *` (P3)

`CustomerRepository.cs:29,37,45` contains SQL and table/column names directly in the methods. `SELECT *` makes mapping sensitive to schema changes and fetches more data than the API needs.

Recommendation:

- select explicit columns and project to read models
- keep queries close to the slice or in clearly named query objects
- avoid a generic repository layer that hides the query's intent
- add repository integration tests against a real SQL Server

### F-24 — Direct use of the system clock (P3)

`DateTimeOffset.UtcNow` and `DateTime.UtcNow` are used in token and audit logic. This complicates deterministic tests of expiry and audit.

Recommendation:

- inject .NET `TimeProvider`
- use the same time source for token expiry, refresh-token activity and audit

### F-25 — Code style and comments are inconsistent (P3)

The codebase mixes Swedish and English comments, two- and four-space indentation, and long pedagogical comments with trivial numbering. It does not harm runtime but makes the template less consistent.

Recommendation:

- add `.editorconfig`
- choose one language for code comments and error messages
- keep comments that explain why, remove those that only describe what the code does
- enable analyzers and treat relevant warnings as errors

## Magic Strings and Magic Values

### Identified Categories

| Category | Example | Recommended Action |
|---|---|---|
| Configuration paths | `"Jwt"`, `"Jwt:RefreshTokenExpiresInDays"`, `"DefaultConnection"` | Typed options and `SectionName`/connection-name constants |
| Authorization policies | `"AdminOrAbove"`, `"SuperAdminOnly"` | `AuthorizationPolicies` class |
| Routes | `"/employees"`, `"/workloads/{id}"`, `"api/v{version:apiVersion}"` | Route constants or named routes; avoid over-centralizing unique routes |
| JWT claims | `"name"`, `"age"`, `"position"` | Claim constants or standard claims where the semantics fit |
| Token durations | `60`, `7` | `JwtOptions` with validated properties |
| Password policy | length `8`, temporary password format | `IdentityOptions` as the single source of truth; the generator should read the policy or be a separate service |
| Validation limits | age `16..100`, comments `1000`, string lengths | Shared domain constraints |
| SQL identifiers | table/column names, `splitOn: "Street"`, `splitOn: "Id,Id"` | Explicit read models, local SQL constants and integration tests |
| Error messages | repeated auth/conflict texts | Stable error code + localizable message; avoid a global text bag |
| OpenAPI/UI | `"MinimalEP API"`, theme/client | Typed documentation configuration if environment-dependent |
| Time | `DateTimeOffset.UtcNow`, `DateTime.UtcNow` | `TimeProvider` |

### Proposed Structure

- `Infrastructure/Auth/JwtOptions.cs`
- `Infrastructure/Auth/AuthorizationPolicies.cs`
- `Domain/Model/EmployeeConstraints.cs`
- `Domain/Model/CustomerConstraints.cs`
- `Domain/Model/WorkloadConstraints.cs`
- named routes per aggregate or use case
- stable error codes in `Result<T>`/Problem Details

Not everything should become a global constant. A string used only once and local to a use case can be clearer where it is. Centralization should be done when the value is a shared invariant, an external contract or a configuration key.

## Performance and Bottlenecks

### Statically Identified Risks

1. Unbounded list queries materialize entire tables.
2. Dapper ignores the request's cancellation token.
3. `StartWorkload` reads the user's entire history for a boolean question.
4. The workload listing does multi-mapping of full Customer/Employee objects instead of a thin read model.
5. The indexes support foreign keys but not clearly the most common filters together with soft delete and sorting.
6. The registration flow uses a synchronous `Count()`.
7. Endpoint registration does repeated reflection at startup.
8. Refresh tokens have no described retention/cleanup and can grow unbounded.

### What Should Be Measured

First create realistic data volumes, for example:

- 100,000 customers
- 25,000 employees
- 5–20 million workloads
- several years of workloads per employee
- multiple active and historical refresh tokens per user

Then measure:

| Scenario | Primary Metrics |
|---|---|
| `GET /workloads` with and without filter | p50/p95/p99, SQL duration, reads, allocations, response size |
| `GET /employees` and `GET /customers` | latency and memory relative to row count |
| `POST /workloads/start` | latency, rows read, concurrency errors |
| login/refresh | latency, DB roundtrips, contention on parallel refresh |
| application startup | migration time and endpoint registration |

Recommended order:

1. add an integration test/benchmark that reproduces the respective query
2. collect CPU and allocation traces
3. capture SQL execution plans and logical reads
4. change one thing at a time
5. run the same measurement after the change and document before/after

### Phase 3 — Measured Result

A BenchmarkDotNet baseline was run against LocalDB with 1,000 customers, 1,000 employees and 10,000 workloads. The same benchmark artifact was run before and after the change.

| Method | Before | After | Change |
|---|---:|---:|---:|
| `GetAllCustomers` | 526.8 µs | 141.7 µs | −73.1 % |
| `GetAllEmployees` | 810.1 µs | 232.9 µs | −71.3 % |
| `GetAllWorkloads` | 36,249.3 µs | 372.3 µs | −99.0 % |
| `GetEmployeeWorkloadHistory` | 33,226.2 µs | 402.7 µs | −98.8 % |

The CPU share in `SqlDataReader.GetValue` dropped from 52.78 % to 9.25 %. The after-profile shows no remaining significant cost in the repository code itself; `WorkloadRepository.QueryPageAsync` accounted for 0.04 % of total CPU.

Completed:

- keyset pagination with a UUID v7 cursor, default page 50 and max 100
- `nextCursor` in list responses
- `CommandDefinition` and request cancellation in all Dapper calls
- SQL `EXISTS` for the open-workload check
- filtering of soft-deleted Customer/Employee in workload joins
- filtered composite indexes for `(EmployeeId, Id)` and `(CustomerId, Id)`
- integration tests for page boundary, cursor without overlap and cancellation

Thin read models were not introduced in this iteration: after pagination the measured repository cost is 0.14–0.40 ms and the profile points to SqlClient/Dapper, not an actionable hotspot in our own code. Refresh-token retention is no longer deferred: a policy is now implemented with typed options (30-day retention) and hourly background cleanup.

## Architectural Assessment

### What Should Be Kept

- vertical slices
- explicit request/response contracts
- separate mappings and validators
- `Result<T>` as the application result
- Dapper for optimized read models and EF Core for transactional writes
- central endpoint discovery, but with more robust metadata
- `/me` as a separate resource-oriented self-service surface

### What Should Be Strengthened

#### Domain invariants

Several rules currently live only in handlers. Critical invariants should have several layers of protection:

1. request validation for fast feedback
2. a domain method for a correct state transition
3. a database constraint/index for concurrency

`Workload.Start`, `Workload.Stop` and the open/closed state are the clearest candidates. Consider methods like `Start`, `Stop` and `CorrectComments` instead of public setters everywhere.

#### Transaction boundaries

A repository per aggregate works for simple operations, but account provisioning spans Identity and Employee. An explicit transaction boundary is needed there. Avoid creating a general `GenericRepository<T>`; instead model use-case-specific transactions.

#### Read models

Dapper queries should return slice-specific DTOs directly. Materializing rich domain objects and navigations for list views creates coupling and unnecessary data work.

#### Error model

`Result<T>` likely needs to be complemented with at least validation, unauthorized/forbidden and a stable domain error with a code. Auth errors should not be modeled as 404/409 merely because the types are missing.

## Proposed Action Plan

### Phase 1 — Stop Security and Integrity Risks

1. Lock down employee and customer routes according to a decided access matrix.
2. Remove `Stop` from the generic workload update.
3. Move SuperAdmin bootstrap away from public registration.
4. Make account/employee/role provisioning atomic and check all Identity results.
5. Add a database constraint for one open workload per employee.
6. Secure refresh-token rotation against concurrency.
7. Introduce rate limiting, lockout and central Problem Details.

### Phase 2 — Create a Safety Net

1. Add a test project and authorization matrix.
2. Add SQL Server integration tests for repositories and constraints.
3. Add concurrency tests.
4. Introduce typed options with startup validation.
5. Add health checks, structured logging and tracing.

### Phase 3 — Scalability

1. Done: establish a profiling baseline and before/after measurement.
2. Done: introduce keyset pagination; thin read models are deferred on measurement grounds.
3. Done: propagate cancellation to Dapper.
4. Done: replace history reading with `EXISTS`.
5. Done: add indexes for the actual filter and cursor shape.
6. Done (follow-up): refresh-token retention policy implemented (30 days) with hourly cleanup and metrics.

### Phase 4 — Maintainability and Magic Values

1. Done: centralize policies, option sections and genuinely shared constraints.
2. Done: synchronize validators with database limits.
3. Done: name routes and correct `Location` headers.
4. Done: add `.editorconfig` and analyzers.
5. Done: simplify comments and standardize language/formatting.

### After Phase 4 — Optimistic Concurrency

1. Done: add SQL Server `rowversion` to `Customer`, `Employee` and `Workload`.
2. Done: include `RowVersion` in read contracts and require it on update, `/me` and workload stop.
3. Done: map `DbUpdateConcurrencyException` to `409 Conflict`.
4. Done: add a migration and an integration test for stale writes.

### Remaining and Deferred

1. Replace the namespace/reflection binding in endpoint registration only if measurement or maintenance problems justify it.
2. Introduce thin read models only if new profiling shows the current mapping has become a hotspot.
3. Run production-like load testing with larger data volumes, concurrent clients, p95/p99 and SQL execution plans.
4. Complement the existing OpenTelemetry instrumentation with a tracing backend, e.g. Jaeger via OTLP. Ensure every trace has a unique `TraceId` that follows W3C Trace Context through incoming HTTP calls, internal spans, `HttpClient` and SQL calls, and is included in structured logs and Problem Details for correlation.

### Completed Since Initial Review

1. `Workload` domain methods for state transitions are implemented and used by workload handlers.
2. `Result<T>` now includes stable error codes and explicit validation/unauthorized/forbidden results.
3. Refresh-token retention policy and scheduled cleanup are implemented (30-day retention, hourly cleanup).
4. `TimeProvider` is introduced in token, refresh, and audit/soft-delete time logic.

## Definition of Done for High-Priority Fixes

A P0/P1 finding should not be marked complete until:

- the behavior is covered by automated tests
- authorization is tested both positively and negatively
- a database constraint exists for the concurrency-sensitive invariant
- errors map to a documented HTTP response
- logging contains a correlation id but no secrets
- the build and relevant tests are green
- a performance change has a before/after measurement with the same workload

## Conclusion

The codebase is a good pedagogical starting point with clear slices and modern .NET patterns. The most important progression is from a working demo to a robust multi-user system: an explicit access matrix, database-enforced invariants, atomic identity flows, central operational telemetry and automated tests. Magic strings/values should be reduced selectively through typed options, policy constants and shared domain constraints. The performance work should begin with the pagination and cancellation risks, but actual bottlenecks should be prioritized only after measurement.

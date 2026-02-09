# Known Issues and Limitations

This file tracks unresolved issues, operational limitations, and scale constraints. It includes select items from [.context/AI-LOG.md](.context/AI-LOG.md) that remain relevant.

## Operational Limitations

- Event-driven consistency: reads from Mongo (audit, dashboard) are eventually consistent with SQL writes; transient delays are expected.
- Dashboard summaries are updated only on event consumption. Missed events or consumer downtime can lead to stale summaries until new events arrive.
- RabbitMQ is a hard dependency for worker updates; if unavailable, audit and dashboard updates pause and retry behavior can amplify backlog.
- Cross-store transactions: no distributed transaction coordinator is present, so SQL and Mongo writes are not truly atomic across stores.
- No authentication or authorization is implemented on the API endpoints.

## Failure Modes (Unaddressed)

- **Partial failure during API writes**: If Postgres commits succeed but MongoDB writes fail, the transaction may roll back SQL changes, but the domain event may already be in RabbitMQ. This can lead to inconsistent state where workers process events for rolled-back transactions.
- **Postgres down, Mongo up**: API requests fail immediately with 500 errors. No graceful degradation or circuit breaker implemented.
- **Mongo down, Postgres up**: Leave request endpoints and audit/dashboard queries fail. Write operations to SQL may succeed but events cannot be stored in Mongo, leading to data loss for audit and leave tracking.
- **RabbitMQ down**: API continues to function for read operations, but write operations that publish events will fail or hang depending on RabbitMQ client timeout settings. Workers cannot consume events and fall behind.
- **Event ordering violations**: RabbitMQ does not guarantee cross-exchange message ordering. If `EmployeeUpdated` arrives at dashboard worker before `EmployeeCreated`, the worker may fail or produce incorrect aggregations. No sequence number or causal ordering implemented.
- **Concurrent dashboard worker instances**: Multiple dashboard worker instances may race to write the same hourly summary. MongoDB upsert by `summaryKey` provides last-write-wins semantics, but could lead to lost updates if events arrive out of order or near hour boundaries.
- **Migration safety in multi-instance deployments**: Migrations run on API startup. If multiple API replicas start simultaneously, concurrent schema migrations may conflict or corrupt database state. No distributed lock or single-instance migration coordinator implemented.

## Scale Constraints

- **Event burst handling**: Burst traffic can trigger repeated dashboard summary recomputation (one per event). Rough ceiling is undefined but estimated at 100-500 events/sec before dashboard worker CPU saturation occurs.
- **Audit retry and DLQ growth**: Audit retry and DLQ queues can grow quickly under sustained failures; operational monitoring and cleanup are required.
- **Worker DB call frequency**: Burst events may cause workers to make frequent DB calls, potentially leading to increased latency; rate limiting or backoff strategies may be needed for high-throughput scenarios.
- **Dashboard aggregation cost**: Dashboard worker performs full SQL aggregations (headcount, task counts, leave stats) on every event. No incremental update strategy. This does not scale beyond moderate workloads (estimated <10K employees, <100K tasks).
- **Audit log storage growth**: Audit logs are append-only with no TTL, archival, or partitioning strategy. Storage will grow unbounded and eventually exhaust disk or MongoDB limits.
- **RabbitMQ queue depth**: No limit on queue depth for audit, retry, or dashboard queues. Prolonged worker downtime can cause memory exhaustion in RabbitMQ broker.

## Retry and DLQ Operational Gaps

- **DLQ consumption strategy**: Dead-letter queue exists for audit worker but no automated or manual replay mechanism is documented. Operators must manually inspect and republish messages.
- **DLQ alerting**: No monitoring or alerting configured for DLQ message arrival. Failed events may go unnoticed until manual inspection.
- **Retry exhaustion tracking**: After 3 retries, messages move to DLQ but no metrics or logs aggregate failure rate or causes.
- **Dashboard worker DLQ**: Dashboard worker has retry logic but no dedicated DLQ. Failed events after max retries are logged and discarded, leading to permanent data loss for dashboard summaries.

## Known Issues from AI-LOG

- Migrations run on API startup by default. Tests bypass this with `SkipMigrationsOnStartup`, but production startup time can increase with large datasets.
- Default credentials are present in configuration templates. These must be replaced in production to avoid insecure defaults.

## Data Retention and Archival

- **No TTL policy**: Audit logs, leave requests, and dashboard summaries have no expiration or archival strategy. Historical data will accumulate indefinitely.
- **No partitioning**: MongoDB collections are not partitioned by date or entity type. Query performance will degrade as collections grow beyond millions of documents.
- **No cold storage migration**: No mechanism to move old audit logs or completed leave requests to cheaper cold storage (e.g., S3, archival databases).

## Capacity Planning Guidance (Missing)

- **Throughput ceiling**: No documented maximum events/sec, API requests/sec, or worker processing rate.
- **Scaling triggers**: No guidance on when to scale API replicas, worker instances, or database resources.
- **Resource sizing**: No baseline CPU, memory, or storage recommendations for production deployments.
- **Load testing results**: No load test data or performance benchmarks to validate system behavior under stress.

## What I Would Do With More Time

- Complete the frontend feature set and verify all user flows against the API contract.
- Add a DLQ workflow and alerting to make failed event recovery operationally safe. Tradeoff: operational complexity increases and replay can introduce duplicate processing risk.
- Add worker rate limiting for burst traffic to avoid DB overload. Tradeoff: higher event latency and slower dashboard freshness under spikes.
- Switch workers to batch reads and writes where possible to reduce per-event query cost. Tradeoff: batching adds memory usage and delays visibility of recent changes.
- Add caching for hot reads (dashboard, lookups) with explicit invalidation on event processing. Tradeoff: cache invalidation complexity and risk of serving stale data.
- Implement distributed locks or coordination to reduce concurrent summary recomputation. Tradeoff: coordination can reduce throughput and create a new dependency surface.

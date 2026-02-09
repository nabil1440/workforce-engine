# AI Log

This log captures user-identified problems and the user instructions for fixes, along with the actual fixes applied to the codebase. This is not an exhaustive list of all changes, but highlights key issues and resolutions during development.

## AppCore

- Problem: TaskStatus ambiguity between AppCore enum and System.Threading.Tasks.TaskStatus in Task events.
  - Instruction: fix the error and commit.
  - Fix: add an alias for the domain TaskStatus in TaskEvents.
- Problem: TaskStatus ambiguity in TaskQuery.
  - Instruction: fix the error and commit.
  - Fix: add an alias for the domain TaskStatus in TaskQuery.
- Problem: TaskStatus ambiguity in TaskRules.
  - Instruction: fix the error on the reported line.
  - Fix: add an alias for the domain TaskStatus in TaskRules.
- Problem: question about unit of work and EF Core; user wanted no infra-specific names in AppCore.
  - Instruction: rename unit of work, then keep AppCore free of the concept.
  - Fix: removed IUnitOfWork (and removed the temporary ITransactionCoordinator) from AppCore.

## Infrastructure and Docker

- Problem: errors in multiple files including docker-compose.yml.
  - Instruction: fix the errors.
  - Fix: replaced tabs with spaces in docker-compose.yml so YAML parses.
- Problem: build errors for missing packages and extensions in Infrastructure projects.
  - Instruction: fix the errors.
  - Fix: added missing Microsoft.Extensions packages and aligned versions to 10.0.2; set Npgsql provider to 10.0.0 (available in restore).
- Problem: Mongo serialization and query errors (DateOnly serializer, SortDirection ambiguity, nullable return).
  - Instruction: fix the errors.
  - Fix: added custom DateOnly serializer, aliased SortDirection to AppCore enum, adjusted DashboardSummaryRepository to return nullable properly.
- Problem: SQL UseNpgsql extension not found due to package mismatch.
  - Instruction: fix the error.
  - Fix: aligned packages and restored UseNpgsql via correct package version.
- Problem: missing indexes and performance issues at scale.
  - Instruction: add SQL and Mongo indexes, run migrations, build and test, then commit.
  - Fix: added SQL composite indexes for employee/task/project queries, created EF migration, added Mongo index initializer (audit, leaves, dashboard), applied migration, built solution, and ran tests.

## Testing and build verification

- Instruction: write tests first, then run tests and build.
  - Fix: added AppCore unit tests and ran dotnet test; then built the solution successfully.

## Audit worker reliability

- Problem: audit worker lacked idempotency, retries with backoff, DLQ, and a health endpoint.
  - Instruction: add exponential backoff (5/10/20), max 3 retries, upsert to avoid duplicates, retry/DLQ queues, and health endpoint.
  - Fix: added retry + DLQ queues, x-retry-count header tracking, exponential backoff, audit upsert, and a /health endpoint.

## Configuration

- Problem: RabbitMQ username/password were hardcoded in options.
  - Instruction: require credentials from config (.env).
  - Fix: removed defaults and validated that credentials are provided during connection setup.
- Problem: needed .env support with a tracked template and compose defaults.
  - Instruction: ignore .env, keep .env.example, use docker-compose interpolation with fallback values.
  - Fix: added .env.example, ignored .env, and switched docker-compose to ${VAR:-default} interpolation.

## Migrations and seeding

- Problem: need initial SQL migrations plus seed data (50 employees with related records).
  - Instruction: generate migrations and seed data using Bogus; keep related data only.
  - Fix: added initial migration, SQL seeder for departments/designations/employees/projects/members/tasks, and startup hook to migrate + seed.
- Problem: need Mongo leave requests seeded to match SQL employees.
  - Instruction: seed MongoDB with matching leave requests.
  - Fix: added Mongo seeder that creates leave requests for seeded employees and runs after SQL seeding.

## Testing

- Problem: API tests failing because migrations run on startup without a DB.
  - Instruction: keep migrations on startup but make tests skip them.
  - Fix: introduced SkipMigrationsOnStartup and set it in ApiTestFactory.

## Node dashboard worker

- Problem: need a Node worker to generate dashboard summaries from SQL + Mongo.
  - Instruction: build a TypeScript worker with ES modules, clean architecture, Prisma for SQL, MongoDB driver, and hourly scheduling.
  - Fix: added a dashboard worker service with Prisma schema, Mongo aggregation, and node-cron scheduling; wired Dockerfile, compose service, and env template.
- Problem: dashboard worker should be idempotent across instances.
  - Instruction: make dashboard summary writes idempotent.
  - Fix: added hourly summaryKey and upserted summaries in Mongo by summaryKey.
- Problem: dashboard worker should consume domain events and not run periodically.
  - Instruction: remove cron scheduling, subscribe to RabbitMQ event exchanges, and rebuild container for testing.
  - Fix: added RabbitMQ consumer with queue bindings to existing domain event exchanges, switched scheduler to per-event summary generation, updated config/compose env, and rebuilt the dashboard worker container.

## API documentation

- Problem: need Swagger UI with docs; build errors on OpenApiInfo.
  - Instruction: add Swagger UI and fix the error.
  - Fix: added Swashbuckle + XML docs, configured Swagger UI, and pinned Microsoft.OpenApi 1.6.23 with explicit OpenApiInfo usage.

## API health

- Problem: API needed a health endpoint.
  - Instruction: add API health endpoint.
  - Fix: registered health checks and exposed /health.

## API responses

- Problem: dashboard summary endpoint returned 404 when no summaries existed.
  - Instruction: return a usable response instead of not_found.
  - Fix: return an empty summary payload with id "latest" and zeroed counts.
- Problem: task detail response only returned assignedEmployeeId.
  - Instruction: include assigned employee name, department, and designation.
  - Fix: enriched task responses with assigned employee details and batch lookups.

## Docker runtime

- Problem: docker builds failed restoring .NET solution and Prisma failed due to missing libssl.
  - Instruction: make docker builds and runtime healthy.
  - Fix: switched Dockerfiles to restore per-project and moved dashboard worker to a glibc-based Node image with libssl support.
- Problem: audit worker failed to start due to missing Microsoft.AspNetCore.App framework.
  - Instruction: fix audit worker runtime.
  - Fix: switched audit worker runtime image to dotnet/aspnet.

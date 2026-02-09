# Workforce Engine

[![CI Status](https://github.com/nabil1440/workforce-engine/actions/workflows/ci.yml/badge.svg)](https://github.com/nabil1440/workforce-engine/actions/workflows/ci.yml)

Workforce Engine is a distributed, event-driven HR and work management system. It combines a .NET API, a .NET audit worker, and a Node.js dashboard worker with Postgres, MongoDB, and RabbitMQ to manage operational data and reporting.

## Architecture (Quick View)

[View Architecture Diagram](https://mermaid.live/edit#pako:eNp1U01v4jAQ_SuWT60EUUNoE3JYiZKCesiK0qqVmvRgksFEgJ21HWlbxH-vPwINu-0lmXnz3sz4Jd7jgpeAY0wFqdfoKckZQrJZunSyrYApaTCEpnfZVHCmgJWoj54rBWgBpFDo6fHNMDSeszP5LSk2FjXy8fw-e-Fis-KiAG9cV28tXtcTLuCsZpG2fs9Wgjz-2WY2kEo0hWoEeBrqMlLOKP-XY8EzlqT_cUBKQitGfziE2QtE68G4KSvVXdXkjtGOSYhcZ-ax5ESUrVr79Vvb_MOEhCjixPNZdjHnUlEB8rJtmGrMniO5PUKL9CG7WJDlslLpw-V51-kd6vd_GbdNpl8udZZayIUWPnr7LW6Hfl-R1A076m1pPjtBVmrBtANKaiG9vVPrwC1nTOwCxj5HsaVOI1PpDDul6Qz39C9clTheka2EHt6B2BGT471h5litYQc5jnVYErHJcc4OWlQT9sr5Dsf6h9AywRu6PjVp6pIoSCqiP9QXRXsNYsIbpnA8GAS2B473-C-Oo6EXDfxoFA2vRkHgX_fwO459bxSEw9AP_WF0cx1GN8Ghhz_s0CsvCjUJ9Cm5SN1NtBfy8AmUOSe0)

### Component Responsibilities

- **Frontend**: React SPA served by Nginx. Sends HTTP requests to the API and displays workforce data. Acts only when users interact with the UI.
- **Workforce API**: .NET 10 REST service. Handles all business logic, validates commands, writes to Postgres and MongoDB transactionally, and publishes domain events to RabbitMQ on successful commits. Runs migrations and seeds data on startup (configurable).
- **Postgres**: Relational database storing employees, departments, projects, tasks, and related org/work data. Acts as the source of truth for operational state.
- **MongoDB**: Document database storing audit logs, leave requests, and precomputed dashboard summaries. Acts as the append-only history and reporting store.
- **RabbitMQ**: Message broker routing domain events from the API to workers. Decouples synchronous API writes from asynchronous worker processing.
- **Audit Worker**: .NET background service consuming all domain events. Writes immutable audit logs to MongoDB with retry logic (exponential backoff, DLQ). Runs continuously and processes events as they arrive.
- **Dashboard Worker**: Node.js service consuming domain events. Rebuilds dashboard summaries in MongoDB on each event with retry logic. Runs continuously and responds to event-driven triggers.

For authoritative architecture rules and required API surface, see [.context/architecture.md](.context/architecture.md). For overall Clean Architecture conventions, see [.context/README.md](.context/README.md).

## Services

- API: .NET 10 REST API that owns business workflows and publishes domain events.
- Audit worker: .NET background service that consumes events and writes immutable audit logs to Mongo.
- Dashboard worker: Node.js worker that consumes events and materializes dashboard summaries in Mongo.
- Frontend: Vite + React app served as static assets behind Nginx.

## Tech Choices (Justification)

- .NET 10: strong typing and high throughput for domain-heavy API and workers.
- Postgres: relational source of truth for org/work data (employees, projects, tasks).
- MongoDB: document store for audit history and precomputed dashboard summaries. This is a good fit for flexible schema.
- RabbitMQ: event bus for decoupling API from workers and enabling async processing. RabbitMQ's documentation, and support for .NET and Node.js made it a good choice for this use case.
- Node.js + Prisma: quick SQL access and efficient aggregation logic for dashboard worker.
- Vite + React + Tailwind: fast frontend iteration and modern UI tooling. Minimal setup friction for building a simple SPA.

## Third-Party Libraries (and Why)

- Bogus: realistic seed data generation for SQL and Mongo development datasets.
- Microsoft.EntityFrameworkCore: ORM for relational data access in the API and infrastructure layers.
- Microsoft.EntityFrameworkCore.Relational: shared relational EF Core features used by the Postgres provider.
- Microsoft.EntityFrameworkCore.Design: design-time migrations and tooling support.
- Npgsql.EntityFrameworkCore.PostgreSQL: EF Core provider for Postgres.
- Microsoft.Extensions.Configuration: configuration loading and binding from appsettings and environment variables.
- Microsoft.Extensions.Configuration.Abstractions: shared config contracts for library reuse.
- Microsoft.Extensions.DependencyInjection: DI container integration across services.
- Microsoft.Extensions.DependencyInjection.Abstractions: DI abstractions to keep infrastructure decoupled.
- Microsoft.Extensions.Hosting.Abstractions: background service hosting contracts for workers.
- Microsoft.Extensions.Options: options pattern for typed configuration.
- Microsoft.Extensions.Options.ConfigurationExtensions: binds configuration to options classes.
- MongoDB.Driver: MongoDB access for audit, leave, and dashboard storage.
- RabbitMQ.Client: AMQP client for publishing and consuming domain events.
- Microsoft.OpenApi: OpenAPI models used by Swagger tooling.
- Swashbuckle.AspNetCore: Swagger generation and UI for API docs.
- Microsoft.AspNetCore.Mvc.Testing: in-memory API test server for integration tests.
- Microsoft.NET.Test.Sdk: test discovery and execution for .NET.
- xunit: unit testing framework.
- xunit.runner.visualstudio: VS test runner integration.
- coverlet.collector: code coverage collection during test runs.
- @prisma/client: typed SQL access in the dashboard worker.
- prisma: schema management and client generation for the dashboard worker.
- amqplib: RabbitMQ client for Node.js.
- mongodb: MongoDB driver for the dashboard worker.
- dotenv: environment variable loading for local worker development.
- tsx: TypeScript runtime for local dashboard worker development.
- typescript: TypeScript compiler for frontend and worker builds.
- @types/amqplib: TypeScript types for RabbitMQ client.
- @types/node: TypeScript types for Node.js runtime.
- react: UI library for the frontend.
- react-dom: DOM bindings for React.
- vite: frontend dev server and build tooling.
- @vitejs/plugin-react: React Fast Refresh and JSX support in Vite.
- tailwindcss: utility-first CSS framework for UI styling.
- @tailwindcss/postcss: Tailwind integration with PostCSS.
- postcss: CSS processing pipeline.
- autoprefixer: vendor prefixing for CSS compatibility.
- eslint: linting for JavaScript and TypeScript.
- @eslint/js: base ESLint rules.
- eslint-plugin-react-hooks: React hooks lint rules.
- eslint-plugin-react-refresh: lint rules for React Fast Refresh.
- typescript-eslint: TypeScript-aware linting rules and parser.
- globals: shared global definitions for ESLint.
- @types/react: TypeScript types for React.
- @types/react-dom: TypeScript types for React DOM.

## Setup (Docker)

1. Copy [.env.example](.env.example) to `.env` and adjust values as needed.
2. From the repo root, run:

```bash
docker compose up --build
```

### Exposed Ports (defaults)

- API: 8080
- Audit worker: 8081
- Dashboard worker health: 8090
- Frontend: 5173
- Postgres: 5432
- MongoDB: 27017
- RabbitMQ: 5672 (management: 15672)

## API Docs and Health Checks

- Swagger UI: `http://localhost:8080/swagger`
- API health: `http://localhost:8080/health`
- Audit worker health: `http://localhost:8081/health`
- Dashboard worker health: `http://localhost:8090/health`

## Setup (Manual / Local Dev)

### Backend (.NET)

```bash
cd backend/dotnet
dotnet restore
dotnet build
dotnet run --project src/Hosts/Workforce.Api/Workforce.Api.csproj
dotnet run --project src/Hosts/Workforce.AuditWorker/Workforce.AuditWorker.csproj
```

Key configuration is in [backend/dotnet/src/Hosts/Workforce.Api/appsettings.json](backend/dotnet/src/Hosts/Workforce.Api/appsettings.json) and [backend/dotnet/src/Hosts/Workforce.AuditWorker/appsettings.json](backend/dotnet/src/Hosts/Workforce.AuditWorker/appsettings.json). Use environment variables from [.env.example](.env.example) to override.

### Dashboard Worker (Node)

```bash
cd backend/node/dashboard-worker
npm install
npm run generate
npm run build
npm run dev
```

### Frontend

```bash
cd frontend
npm install
npm run dev
```

## Environment Variables

The complete list is in [.env.example](.env.example). The most important values are:

- `POSTGRES_*` and `MONGO_*` for database connectivity
- `RABBITMQ_*` for event bus connectivity
- `API_PORT`, `AUDIT_WORKER_PORT`, `FRONTEND_PORT`, `DASHBOARD_HEALTH_PORT`

## Tests

```bash
cd backend/dotnet
dotnet test
```

## Known Limitations

See [KNOWN-ISSUES.md](KNOWN-ISSUES.md) for current limitations, scale constraints, and unresolved items.

## AI Workflow

See [AI-WORKFLOW.md](AI-WORKFLOW.md) for how AI was used during development and validation.

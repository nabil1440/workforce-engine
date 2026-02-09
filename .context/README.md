# Default Instructions for Context

We are working in a distributed system using Clean Architecture.

Rules:

- Business logic lives in AppCore only
- No DbContext, Mongo client, or broker SDK in AppCore
- API controllers must be thin
- Workers consume events, not HTTP
- Publish domain events on successful state changes only
- Prefer explicit code over clever abstractions
- All DB changes must be a transaction across SQL + Mongo

If unsure, ask for clarification instead of inventing architecture.

## Backend Folder Structure:

```
backend/dotnet/
├─ Workforce.slnx
├─ src/
│ ├─ Core/
│ │ └─ Workforce.AppCore
│ ├─ Infrastructure/
│ │ ├─ Workforce.Infrastructure.Sql
│ │ ├─ Workforce.Infrastructure.Mongo
│ │ └─ Workforce.Infrastructure.Messaging
│ └─ Hosts/
│ ├─ Workforce.Api
│ └─ Workforce.AuditWorker
└─ tests/
└─ Workforce.AppCore.Tests
```

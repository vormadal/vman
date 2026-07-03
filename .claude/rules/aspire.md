---
paths:
  - "**/VideoManager.AppHost/**/*"
  - "**/VideoManager.ServiceDefaults/**/*"
---

# .NET Aspire Orchestration

## Starting the App

```bash
cd VideoManager/VideoManager.AppHost
dotnet run
```

This starts all services:
- **API**: localhost:5001 (VManBackend)
- **Frontend**: localhost:3000 (Next.js dev server)
- **PostgreSQL**: port 5432 with pgAdmin
- **Aspire Dashboard**: localhost:17037

## Configuration

- `USE_STUB_IMMICH`: Set to `"true"` for mock Immich (no real Immich server needed)
- `IMMICH_API_KEY`: Optional. If left empty, the backend bootstraps one itself against
  `Immich:BaseUrl` via admin-sign-up/login/api-key calls -- see `ImmichBootstrapper`. Requires
  `IMMICH_ADMIN_PASSWORD` when bootstrapping.
- `IMMICH_ADMIN_PASSWORD`: Password for the Immich bootstrap admin account (only needed when
  `IMMICH_API_KEY` isn't supplied and `USE_STUB_IMMICH` is `"false"`)
- `TestUser:Email` / `TestUser:Password`: Auto-injected test credentials for both API and frontend

## Rules

- Always use Aspire for local development -- never start services individually
- Database migrations run automatically on startup in dev
- Test user is seeded automatically in dev
- Admin user is seeded in all environments

# Aspire Integration Guide

## Overview

Your Next.js frontend is now integrated with .NET Aspire! Aspire will orchestrate both your backend API and frontend together.

## Architecture

```
VideoManager.AppHost (Aspire Orchestrator)
├── PostgreSQL Database (with pgAdmin)
├── VideoManager API (.NET)
└── Frontend (Next.js) ✨ NEW
```

## What Changed

### 1. AppHost Configuration
**File**: `VideoManager/VideoManager.AppHost/Program.cs`

Added the Next.js app to Aspire orchestration:
```csharp
var frontend = builder.AddNpmApp("frontend", "../../video-manager-frontend", "dev")
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);
```

This tells Aspire to:
- Run `npm run dev` in the frontend directory
- Assign a dynamic port (via PORT environment variable)
- Make it accessible externally
- Inject the API service URL automatically

### 2. Next.js Configuration
**File**: `next.config.ts`

Added Aspire environment variable support:
```typescript
env: {
  NEXT_PUBLIC_API_URL: process.env.services__apiservice__http__0 || ...
}
```

Aspire automatically injects service URLs in this format:
- `services__<servicename>__http__0` → HTTP endpoint
- `services__<servicename>__https__0` → HTTPS endpoint

### 3. NuGet Package
**File**: `VideoManager.AppHost.csproj`

Added Node.js hosting support:
```xml
<PackageReference Include="Aspire.Hosting.NodeJs" Version="9.5.2" />
```

## Running with Aspire

### Start Everything at Once
```bash
cd C:\workspaces\vormadal\vman\VideoManager\VideoManager.AppHost
dotnet run
```

This starts:
1. PostgreSQL database
2. pgAdmin (database UI)
3. VideoManager API
4. Next.js Frontend

### Aspire Dashboard
Open: `http://localhost:15XXX` (check console output)

The dashboard shows:
- ✅ All service health status
- 📊 Logs from all services
- 🔗 Service endpoints
- 📈 Metrics and traces

## Benefits

### ✨ Automatic Service Discovery
The frontend automatically knows the API URL - no manual configuration needed!

### 🔄 Unified Logging
All logs (frontend + backend) in one place via Aspire Dashboard.

### 🚀 One Command Start
Start your entire stack with a single `dotnet run`.

### 📊 Observability
Built-in metrics, distributed tracing, and health checks.

### 🔗 Service Dependencies
Aspire manages startup order (DB → API → Frontend).

## Development Workflow

### Option 1: Aspire (Recommended for full-stack work)
```bash
cd C:\workspaces\vormadal\vman\VideoManager\VideoManager.AppHost
dotnet run
```

### Option 2: Frontend Only (Recommended for UI work)
```bash
cd C:\workspaces\vormadal\vman\video-manager-frontend
npm run dev
```

### Option 3: Backend Only
```bash
cd C:\workspaces\vormadal\vman\VideoManager\VideoManager
dotnet run
```

## Environment Variables

Aspire automatically provides:

| Variable | Value | Usage |
|----------|-------|-------|
| `PORT` | Dynamic port | Next.js listens on this port |
| `services__apiservice__http__0` | API URL | Backend endpoint |
| `services__apiservice__https__0` | API HTTPS URL | Secure backend endpoint |

These are automatically injected - no `.env` file needed when running via Aspire!

## Troubleshooting

### Port Conflicts
If ports are in use, Aspire will automatically assign different ports.

### Frontend Not Starting
Check that `npm install` has been run:
```bash
cd C:\workspaces\vormadal\vman\video-manager-frontend
npm install
```

### Can't Find API Service
Make sure the service name matches in Program.cs:
- Backend: `"apiservice"`
- Frontend reference: `services__apiservice__http__0`

## Next Steps

When your backend is ready:
1. Update `src/lib/api/imageApi.ts` to use real API calls
2. The API URL will automatically be correct via Aspire!
3. Enjoy automatic service discovery ✨

## Learn More

- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Aspire Node.js Integration](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/build-aspire-apps-with-nodejs)

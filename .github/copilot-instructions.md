---
applyTo: '**'
name: 'GitHub Copilot Instructions for Video Manager'
description: 'Instructions for GitHub Copilot to assist in development of the Video Manager project, covering both backend and frontend guidelines, architecture patterns, and best practices.'
---

## Project Overview
**Video Manager** is a simple video and media management application that adds additional features to existing platforms, such as Immich. The user can tag assets, and create collections with videos and images from Immich which then can be exported to Shotcuts MLT format for further video editing.

### Technology Stack
- **Backend**: .NET 10, ASP.NET Core MVC, C# 13
- **Frontend**: Next.js 15 (App Router), React 19, TypeScript 5
- **Database**: PostgreSQL (via EF Core 10 with Npgsql)
- **Orchestration**: .NET Aspire 13.1.0 using podman (instead of docker)
- **Authentication**: JWT Bearer tokens with BCrypt password hashing
- **Observability**: OpenTelemetry (traces, metrics, logs)
- **UI**: Tailwind CSS 4, shadcn/ui, Radix UI
- **State Management**: Zustand (client state) + React Query (server state)
- **Testing**: Playwright (E2E)
- **Video Processing**: FFmpeg (planned)

---

## Repository Structure

```
vman/
├── VideoManager/                      # Main .NET Solution
│   ├── VideoManager.AppHost/         # Aspire orchestration (run from here)
│   ├── VideoManager.ServiceDefaults/ # Shared Aspire configuration
│   ├── VideoManager/                 # Main API (.NET 10)
│   └── VManBackend/                  # Alternative backend project
├── video-manager-frontend/           # Next.js 15 frontend
│   ├── src/                          # Source code (app, components, lib)
│   ├── tests/                        # Playwright E2E tests
│   └── playwright.config.ts          # Test configuration
├── VideoSplitter/                    # Utility project
└── vlc-extension/                    # VLC media player extension
```


## Development Workflow

* NEVER run migration scripts manually against the database. Always use the EF Core tools to manage migrations.
* 

### Running the Application

**Option 1: Aspire (Recommended)**
```bash
cd VideoManager\VideoManager.AppHost
dotnet run
# Access Aspire Dashboard at http://localhost:15XXX
# API runs on dynamically assigned port
```

### API Client Generation (Frontend)
```bash
cd video-manager-frontend
npm run generate:client  # Runs Kiota to generate TypeScript client
```

### Running E2E Tests (Frontend)
```bash
cd video-manager-frontend

# Run tests (starts dev server automatically)
npm run test:e2e

# Interactive mode for debugging
npm run test:e2e:ui
```

## Documentation References
- **Setup**: See `PROJECT_SETUP.md`, `FRONTEND_SETUP.md`, `ASPIRE_FRONTEND_SETUP.md`
- **Aspire**: See `VideoManager/ASPIRE_README.md`
- **Architecture**: See `SETUP_SUMMARY.md`
- **Agents**: See `AGENTS.md` for AI assistant guidelines
- **Testing**: See `video-manager-frontend/tests/README.md` for Playwright E2E test documentation

- Always use Aspire for local development to ensure proper configuration and service orchestration.
- always use DateTimeOffset (NOT DateTime) for timestamps in backend code.
- always ensure contracts and domain models are immutable and are defined as records.

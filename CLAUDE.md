# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Video Manager is a full-stack application for managing media from providers like Immich and OneDrive. Users can tag assets, create collections, and export to Shotcut MLT format for video editing.

## Tech Stack

- **Backend**: .NET 10, ASP.NET Core, C# 13, EF Core 10, PostgreSQL
- **Frontend**: Next.js 15 (App Router), React 19, TypeScript 5, Tailwind CSS 4, shadcn/ui
- **State**: Zustand (client) + React Query (server)
- **Orchestration**: .NET Aspire 13.1.0 (uses podman)
- **Auth**: JWT with BCrypt hashing

## Running the Application

**Always use Aspire** for local development:
```bash
cd VideoManager/VideoManager.AppHost
dotnet run
# Or: aspire run
```

This starts: API (localhost:5001), Frontend (localhost:3000), PostgreSQL, pgAdmin, and Aspire Dashboard (localhost:17037).

**Frontend standalone:**
```bash
cd video-manager-frontend
npm install
npm run dev
```

## Common Commands

### Backend
```bash
# EF Core migrations (from VManBackend directory)
cd VideoManager/VManBackend
dotnet ef migrations add {MigrationName}
dotnet ef database update
```

### Frontend
```bash
cd video-manager-frontend
npm run dev                  # Dev server
npm run lint                 # ESLint
npm run generate:client      # Generate Kiota API client
npm run test:e2e             # Playwright tests
npm run test:e2e:ui          # Interactive test mode
```

## Architecture

### Backend: Vertical Slice + Custom CQRS

Each feature is self-contained in `VManBackend/Features/`. Single file contains Request, Response, Validator, Handler:

```
Features/
├── Authentication/
│   ├── Register.cs
│   └── Login.cs
├── Tags/
│   ├── CreateTag.cs
│   ├── GetTags.cs
│   └── ...
└── Items/
    └── ...
```

Endpoints are extension methods in `Endpoints/` folder, registered in Program.cs:
```csharp
app.MapAuthEndpoints();
app.MapTagEndpoints();
```

### Frontend: App Router with Route Groups

```
src/app/
├── (auth)/         # Public: login, register
├── (dashboard)/    # Protected: videos, images
├── layout.tsx
└── middleware.ts   # Route protection
```

State strategy:
- **Zustand**: Auth state, UI preferences only
- **React Query**: All API data (never duplicate server data in Zustand)

## Key Patterns

### Backend DO
- Use records for DTOs and requests
- Use `ApplicationDbContext` directly in handlers (no repository pattern)
- Use `Results.Problem()` for all errors (ProblemDetails RFC 7807)
- Use `DateTimeOffset` for timestamps (not DateTime)
- Manual validation (no FluentValidation library)
- Custom mediator in `Mediator/` (not MediatR library)

### Backend DON'T
- Don't use attribute routing (`[Route]`, `[Authorize]`)
- Don't use `.WithOpenApi()` (deprecated), use `.WithTags()` instead
- Don't define endpoints inline in Program.cs
- Don't run migrations manually against database

### Frontend DO
- Use `cn()` helper for conditional Tailwind classes
- Use shadcn/ui components
- Wrap API calls in custom hooks with React Query
- Use Zod for form validation with React Hook Form

### Frontend DON'T
- Don't use Pages Router (use App Router)
- Don't use direct `fetch()` calls (use generated API client)
- Don't store server data in Zustand

## API Endpoints

```
POST /api/auth/register
POST /api/auth/login
GET/POST /api/tags
GET/PUT/DELETE /api/tags/{id}
GET /api/items
GET /api/items/{provider}/{id}
POST/DELETE /api/items/{provider}/{id}/tags
```

## Environment Variables

Backend: `IMMICH_API_KEY` for Immich integration
Frontend: `NEXT_PUBLIC_API_URL`, test credentials in `.env`

## Testing

E2E tests use Playwright with Page Object Model pattern:
- Tests in `video-manager-frontend/tests/`
- Page objects in `tests/pages/`

## Project Structure

```
vman/
├── VideoManager/
│   ├── VideoManager.AppHost/      # Aspire orchestration (start here)
│   ├── VideoManager.ServiceDefaults/
│   └── VManBackend/               # Main API
│       ├── Common/Data/           # DbContext, models
│       ├── Features/              # Vertical slices
│       ├── Endpoints/             # API endpoint mappings
│       ├── Infrastructure/        # JWT, Immich client, providers
│       └── Mediator/              # Custom CQRS implementation
├── video-manager-frontend/        # Next.js frontend
│   ├── src/app/                   # App Router pages
│   ├── src/components/            # React components
│   ├── src/lib/                   # Hooks, stores, API client
│   └── tests/                     # Playwright E2E tests
└── .github/instructions/          # Detailed AI assistant guidelines
```

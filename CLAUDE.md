# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Video Manager is a full-stack application for managing media from providers like Immich and OneDrive. Users can tag assets, manage people (face recognition), create collections, and export to Shotcut MLT format for video editing. Registration is invite-only -- admins create invite links for new users.

## Tech Stack

- **Backend**: .NET 10, ASP.NET Core, C# 13, EF Core 10, PostgreSQL
- **Frontend**: Next.js 16, React 19, TypeScript 5.9, Tailwind CSS 4, shadcn/ui
- **State**: Zustand (client) + React Query (server)
- **Orchestration**: .NET Aspire 13.1.0 (uses podman)
- **Auth**: JWT with BCrypt hashing, role-based (User/Admin)

## Running the Application

**Always create a plan** before implementing and save it in the `plans` folder.
**Always implement features** in small increments and get verification from the user.

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
npm run dev:with-gen         # Generate API client then start dev server
npm run lint                 # ESLint
npm run generate:client      # Generate Kiota API client
npm run generate:watch       # Watch OpenAPI spec for changes
npm run test:e2e             # Playwright tests
npm run test:e2e:ui          # Interactive test mode
npm run test:e2e:debug       # Debug test mode
npm run test:e2e:report      # View test report
```

## Architecture

### Backend: Vertical Slice + Custom CQRS

Each feature is self-contained in `VManBackend/Features/`. A single file contains Request, Response, Validator, and Handler:

```
Features/
├── Admin/              # User management, invites, roles
│   ├── BlockUser.cs
│   ├── ChangeUserRole.cs
│   ├── CreateInvite.cs
│   ├── GetInvites.cs
│   ├── GetUsers.cs
│   └── UnblockUser.cs
├── Authentication/     # Login, invite acceptance, profile
│   ├── AcceptInvite.cs
│   ├── CompleteProfile.cs
│   ├── Login.cs
│   └── Register.cs
├── Collections/        # Media collections, ordering, export
│   ├── AddItemToCollection.cs
│   ├── CreateCollection.cs
│   ├── DeleteCollection.cs
│   ├── ExportCollectionToShotcut.cs
│   ├── GetCollectionById.cs
│   ├── GetCollections.cs
│   ├── RemoveItemFromCollection.cs
│   └── UpdateCollectionItemOrder.cs
├── Items/              # Media items, tagging
│   ├── AddTagToItem.cs
│   ├── GetItemById.cs
│   ├── GetItems.cs
│   ├── GetItemsByTag.cs
│   └── RemoveTagFromItem.cs
├── People/             # Face recognition data from Immich
│   ├── GetPeople.cs
│   └── GetPersonById.cs
├── Sync/               # Background sync with Immich
│   ├── CancelSync.cs
│   ├── GetSyncStatus.cs
│   └── TriggerSync.cs
└── Tags/
    ├── CreateTag.cs
    ├── DeleteTag.cs
    ├── GetTagById.cs
    ├── GetTags.cs
    └── RenameTag.cs
```

Endpoints are extension methods in `Endpoints/` folder, registered in Program.cs:
```csharp
app.MapAuthEndpoints();
app.MapAdminEndpoints();
app.MapTagEndpoints();
app.MapItemEndpoints();
app.MapSyncEndpoints();
app.MapProviderEndpoints();
app.MapCollectionEndpoints();
app.MapPeopleEndpoints();
```

### Frontend: App Router with Route Groups

```
src/app/
├── (auth)/                 # Public: login, register, accept-invite
├── (dashboard)/            # Protected routes
│   ├── admin/              # Admin: invites, users
│   ├── collections/        # Collection list + [id] detail
│   ├── images/             # Image gallery
│   ├── items/              # Items + tagging mode
│   ├── sync/               # Sync management
│   ├── tags/               # Tag management
│   └── videos/             # Video gallery (redirects to items)
├── complete-profile/       # Profile completion after invite
├── layout.tsx
└── middleware.ts           # Route protection
```

State strategy:
- **Zustand**: Auth state, UI preferences, collection mode only
- **React Query**: All API data (never duplicate server data in Zustand)

## Key Patterns

### Backend DO
- Use records for DTOs and requests
- Use `ApplicationDbContext` directly in handlers (no repository pattern)
- Use `Results.Problem()` for all errors (ProblemDetails RFC 7807)
- Use `DateTimeOffset` for timestamps (not DateTime)
- Manual validation (no FluentValidation library)
- Custom mediator in `Mediator/` (not MediatR library)
- Register new handlers in `Program.cs`
- Apply `.RequireAuthorization()` to route groups, not individual endpoints
- Use `"AdminOnly"` policy for admin-only endpoints

### Backend DON'T
- Don't use attribute routing (`[Route]`, `[Authorize]`)
- Don't use `.WithOpenApi()` (deprecated), use `.WithTags()` instead
- Don't define endpoints inline in Program.cs
- Don't run migrations manually against database
- Don't use FluentValidation or MediatR libraries

### Frontend DO
- Use `cn()` helper for conditional Tailwind classes
- Use shadcn/ui components
- Wrap API calls in custom hooks with React Query
- Use Zod for form validation with React Hook Form
- Use `npm run generate:client` to create the api client
- Format dates as `dd-MM-yyyy` and time as 24H like `HH:mm`
- Use the following import for toast `import { useToast } from '@/hooks/use-toast';`
- **Always** use generated API client to fetch data
- Use `'use client'` directive only when necessary (state, effects, interactivity)

### Frontend DON'T
- Don't use Pages Router (use App Router)
- Don't store server data in Zustand
- Don't create or update the api client manually
- **Never** run `npm run build` to verify changes (changes are hot reloaded)
- **Never** make manual changes in the `video-manager-frontend/src/lib/api/client.ts` file

## API Endpoints

### Authentication (public, except complete-profile)
```
POST /api/auth/register         # Disabled -- returns 403 (invite-only)
POST /api/auth/login
POST /api/auth/accept-invite
POST /api/auth/complete-profile # Requires auth
```

### Admin (requires AdminOnly policy)
```
POST   /api/admin/invites
GET    /api/admin/invites
GET    /api/admin/users
POST   /api/admin/users/{userId}/block
POST   /api/admin/users/{userId}/unblock
PUT    /api/admin/users/{userId}/role
```

### Tags (requires auth)
```
GET    /api/tags
POST   /api/tags
GET    /api/tags/{id}
PUT    /api/tags/{id}
DELETE /api/tags/{id}
```

### Items (requires auth)
```
GET    /api/items                          # Filters: provider, type, untagged, tagId, personId, sortBy, sortDescending, page, pageSize
GET    /api/items/{provider}/{id}
POST   /api/items/{provider}/{id}/tags
DELETE /api/items/{provider}/{id}/tags/{tagId}
```

### Collections (requires auth)
```
GET    /api/collections
POST   /api/collections
GET    /api/collections/{id}
DELETE /api/collections/{id}
POST   /api/collections/{id}/items
DELETE /api/collections/{collectionId}/items/{itemId}
PUT    /api/collections/{id}/items/reorder
GET    /api/collections/{id}/export/shotcut  # Returns ZIP file
```

### Sync (requires auth)
```
POST   /api/sync
GET    /api/sync/status
POST   /api/sync/{jobId}/cancel
```

### People (requires auth)
```
GET    /api/people                          # Filters: search, page, pageSize
GET    /api/people/{id}
```

### Providers (requires auth)
```
GET    /api/providers/{provider}/items/{id}/thumbnail
GET    /api/providers/{provider}/items/{id}/preview
```

## Database Models

Key entities in `VManBackend/Common/Models/`:
- **User**: Email, hashed password, display name, role (User/Admin), blocked status
- **UserInvite**: Token-based invite system for new users
- **Tag**: Global tags for categorizing items
- **Item**: Media items synced from Immich (provider + external ID)
- **ItemTag**: Junction table for Item-Tag relationships
- **Collection**: User-created ordered collections of items
- **CollectionItem**: Junction table with sort order
- **Person**: Face recognition data (synced from Immich)
- **ItemPerson**: Junction table for Item-Person relationships
- **SyncJob**: Tracks background sync operations with status
- **SyncHistory**: Historical sync records
- **ImmichAsset** / **ImmichExifData**: Immich-specific asset metadata

## Background Services

- **SyncBackgroundService**: Hosted service that processes sync jobs from a channel
- **ImmichSyncProcessor**: Handles actual sync logic with Immich API
- **SyncChannel**: In-memory channel for queuing sync requests

## Environment Variables

Backend: `IMMICH_API_KEY`, `USE_STUB_IMMICH` (set to `"true"` for mock Immich in dev)
Frontend: `NEXT_PUBLIC_API_URL`, test credentials in `.env`
Aspire: `TestUser:Email`, `TestUser:Password` (auto-injected to both services)

## Testing

E2E tests use Playwright with Page Object Model pattern:
- Tests in `video-manager-frontend/tests/`
- Page objects in `tests/pages/`
- Auth setup in `tests/auth.setup.ts`
- Authenticated fixture in `tests/fixtures/authenticated.ts`
- Runs against Chromium, Firefox, WebKit + mobile viewports (Pixel 5, iPhone 12)

## Project Structure

```
vman/
├── VideoManager/
│   ├── VideoManager.AppHost/          # Aspire orchestration (start here)
│   ├── VideoManager.ServiceDefaults/  # Shared Aspire service defaults
│   └── VManBackend/                   # Main API
│       ├── Common/
│       │   ├── Data/                  # ApplicationDbContext
│       │   └── Models/               # Entity models
│       ├── Features/                  # Vertical slices (Admin, Auth, Collections, Items, People, Sync, Tags)
│       ├── Endpoints/                 # API endpoint mappings
│       ├── Infrastructure/
│       │   ├── Authentication/        # JWT service
│       │   ├── Data/                  # DbSeeder
│       │   ├── Immich/               # Immich client + generated Kiota code
│       │   ├── Providers/            # Media provider abstraction + caching
│       │   └── Sync/                 # Background sync engine
│       ├── Mediator/                  # Custom CQRS implementation
│       └── Migrations/               # EF Core migrations
├── video-manager-frontend/            # Next.js frontend
│   ├── src/
│   │   ├── app/                       # App Router pages
│   │   ├── components/               # React components + shadcn/ui
│   │   ├── hooks/                    # use-toast
│   │   └── lib/
│   │       ├── api/                  # Generated Kiota client + types
│   │       ├── hooks/                # useApi, useAuth, useHydration, useImages
│   │       ├── store/                # Zustand stores (authStore, collectionModeStore)
│   │       ├── validations/          # Zod schemas
│   │       └── utils.ts              # cn() helper
│   └── tests/                         # Playwright E2E tests
├── VideoSplitter/                     # Companion .NET project for video frame extraction
├── vlc-extension/                     # VLC plugin for integration
├── plans/                             # Implementation plans (create before implementing)
├── .claude/rules/                     # Claude AI assistant rules (path-scoped)
│   ├── backend-endpoints.md
│   ├── backend-features.md
│   ├── frontend-components.md
│   └── playwright-tests.md
├── .github/workflows/                 # CI/CD (Playwright test workflow)
└── Dockerfile                         # Multi-stage production build (Node + .NET)
```

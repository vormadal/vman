# GitHub Copilot Instructions - Video Manager

## Project Overview
**Video Manager** is an enterprise-grade, multi-platform video and media management application built with modern cloud-native technologies. It integrates with multiple video providers (Immich, OneDrive) to enable unified browsing, management, and processing of videos with automated thumbnail and GIF generation capabilities.

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

---

## Backend Guidelines (VideoManager/)

### Architecture: Vertical Slice + Custom CQRS

#### Vertical Slice Pattern
- Each feature is self-contained in `Features/` folder
- Single file contains: Request, Response, Validator, Handler
- Group related code within feature folder (no cross-feature dependencies)
- Use `Common/` for shared models/data, `Infrastructure/` for cross-cutting concerns




### Data Access
- **Avoid Repository Pattern** - DbContext already provides it
- Use `ApplicationDbContext` directly in handlers
- Entity configuration via Fluent API in `OnModelCreating`
- Migrations: `dotnet ef migrations add {Name}` from VideoManager project

### Authentication & Security
- JWT tokens via custom `JwtService` (Infrastructure/Authentication/)
- Token expiration: 24 hours (configurable via appsettings)
- Password hashing: BCrypt.Net-Next with cost factor 4
- Bearer authentication scheme
- User claims: Id, Email, Name stored in JWT

### Aspire Integration
- **Always run via AppHost** for local development: `dotnet run` in `VideoManager.AppHost/`
- Connection strings auto-injected by Aspire
- Service defaults provide: OpenTelemetry, health checks, resilience patterns
- Database provisioned automatically (PostgreSQL + pgAdmin)

### API Conventions
- OpenAPI/Swagger enabled at `/swagger`
- Health checks: `/health` (detailed), `/alive` (simple)
- Minimal API endpoints (no attribute-based routing)
- Return `Results.*` helpers (Ok, NotFound, BadRequest, etc.)

### External Integrations
- **Immich**: Kiota-generated client in `Infrastructure/Immich/Generated/`
- **OneDrive**: Microsoft Graph SDK (planned)
- API keys via environment variables (e.g., `IMMICH_API_KEY`)

---

## Frontend Guidelines (video-manager-frontend/)

### Architecture: Next.js 15 App Router

#### Routing Structure
```
app/
├── (auth)/              # Auth routes (login, register)
│   ├── login/
│   └── register/
├── (dashboard)/         # Protected routes
│   ├── videos/
│   └── images/
├── layout.tsx           # Root layout
├── providers.tsx        # React Query & Theme providers
└── middleware.ts        # Route protection
```

### State Management Strategy
- **Client State** (Zustand): Auth state, UI preferences, dark mode
- **Server State** (React Query/TanStack Query): Videos, images, tags, users
- **Never duplicate server data in Zustand** - always use React Query


### API Integration
- Kiota-generated clients from backend OpenAPI specs found in ` https://localhost:<port>/openapi/v1.json`
- Custom wrappers in `lib/api/` for auth header injection
- **Note**: Currently using stub/mock data - backend integration in progress

### Styling Conventions
- Tailwind CSS 4 utility classes
- Use `cn()` helper for conditional classes
- Dark mode via `next-themes` (provider in `app/providers.tsx`)
- shadcn/ui components for consistency

---

## Common Patterns & Best Practices

### Dependency Injection (Backend)
```csharp
// Service registration in Program.cs
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddSingleton<IImmichService, ImmichService>();

// Constructor injection in handlers
public class Handler(ApplicationDbContext db, IJwtService jwt, ILogger<Handler> logger)
```

### Configuration Management
- **Backend**: `appsettings.json` for defaults, `appsettings.Development.json` for overrides
- **Secrets**: User Secrets for development, environment variables for production
- **Frontend**: Environment variables in `.env.local` (e.g., `NEXT_PUBLIC_API_URL`)

### Error Handling
- **Backend**: Return appropriate HTTP status codes (400, 401, 404, 500)
- **Frontend**: React Query handles loading/error states automatically
- Use toast notifications (Sonner) for user feedback

### Testing

#### Frontend E2E Testing (Playwright)
- **Framework**: Playwright with TypeScript
- **Location**: `video-manager-frontend/tests/`
- **Browsers**: Chromium, Firefox, WebKit, Mobile Chrome, Mobile Safari
- **Configuration**: `playwright.config.ts`

**Running Tests:**
```bash
cd video-manager-frontend

# Run all tests
npm run test:e2e

# Interactive UI mode (recommended during development)
npm run test:e2e:ui

# Debug mode with browser DevTools
npm run test:e2e:debug

# View HTML report
npm run test:e2e:report

# Run specific test file
npx playwright test auth.spec.ts

# Run specific browser
npx playwright test --project=chromium
```

**Test Structure:**
- `auth.spec.ts` - Authentication flows (login, register)
- `home.spec.ts` - Home page and redirects
- `videos.spec.ts` - Video management (requires auth)
- `auth.setup.ts` - Authentication setup for authenticated tests
- `fixtures/authenticated.ts` - Reusable auth fixture

**Writing Tests:**
```typescript
// Basic test
import { test, expect } from '@playwright/test';

test('should display login page', async ({ page }) => {
  await page.goto('/login');
  await expect(page.getByRole('heading', { name: /sign in/i })).toBeVisible();
});

// Authenticated test
import { test, expect } from './fixtures/authenticated';

test('should access videos page', async ({ page }) => {
  test.use({ storageState: 'playwright/.auth/user.json' });
  await page.goto('/videos');
  await expect(page).toHaveURL(/.*videos/);
});
```

**Playwright in Aspire:**
- **Do NOT** include Playwright tests in main AppHost
- Tests are a separate validation step, not part of app orchestration
- Run tests independently after starting the app
- See `tests/README.md` for detailed documentation

#### Backend Testing
- **Not currently implemented** - focus on feature delivery first
- Future: xUnit, integration tests with WebApplicationFactory

### Database Schema Conventions
- UUID primary keys (`Guid` in C#, `UUID` in PostgreSQL)
- Timestamp fields: `CreatedAt` (required), `UpdatedAt` (optional)
- Foreign keys with cascade delete where appropriate
- Indexes on frequently queried columns

---

## Development Workflow

### Running the Application

**Option 1: Aspire (Recommended)**
```bash
cd VideoManager\VideoManager.AppHost
dotnet run
# Access Aspire Dashboard at http://localhost:15XXX
# API runs on dynamically assigned port
```

**Option 2: Standalone Services**
```bash
# Backend
cd VideoManager\VideoManager
dotnet run

# Frontend
cd video-manager-frontend
npm run dev  # http://localhost:3000
```

### Database Migrations
```bash
cd VideoManager\VideoManager
dotnet ef migrations add MigrationName
dotnet ef database update
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

### Code Generation (Backend - Immich Client)
```bash
cd VideoManager\VideoManager
dotnet tool restore
dotnet kiota generate -d path/to/openapi.json -o Infrastructure/Immich/Generated
```

## Key Design Decisions

### Why Custom Mediator?
- Avoid external dependencies for simple request/response pattern
- Full control over handler resolution and lifecycle
- Lightweight alternative to MediatR

### Why Vertical Slice?
- Feature cohesion: all related code in one place
- Easier to reason about individual features
- Reduces coupling between features
- Simpler to onboard new developers

### Why Aspire?
- Simplified local development (one command starts everything)
- Built-in observability (OpenTelemetry dashboard)
- Service discovery and resilience patterns
- Cloud-native architecture from day one

### Why Next.js App Router?
- Server Components for better performance
- Built-in routing conventions
- Middleware for auth protection
- Modern React patterns (RSC, Streaming)

---

## Prohibited Patterns

### ❌ Don't Do This (Backend)
- Repository pattern over DbContext
- Generic repositories
- Service layer between handlers and DbContext
- Attribute routing (`[ApiController]`, `[Route]`)
- Using MediatR library
- FluentValidation library (use manual validation)

### ❌ Don't Do This (Frontend)
- Storing server data in Zustand (use React Query)
- Client-side auth logic (use middleware)
- Direct fetch() calls (use generated API clients)
- Inline Tailwind classes without `cn()` helper
- Custom CSS files (use Tailwind utilities)
- Adding Playwright tests to Aspire AppHost (keep tests separate)

---

## File Naming Conventions

### Backend (C#)
- Features: `Features/{FeatureName}/{Action}.cs` (e.g., `Features/Auth/Login.cs`)
- Models: PascalCase (e.g., `User.cs`, `Video.cs`)
- Interfaces: `I{Name}.cs` (e.g., `IJwtService.cs`)

### Frontend (TypeScript)
- Components: `{name}.tsx` (lowercase with dashes, e.g., `video-card.tsx`)
- Pages: Route folder structure (e.g., `app/(dashboard)/videos/page.tsx`)
- Hooks: `use{Name}.ts` (e.g., `useAuth.ts`)
- Types: `{name}.types.ts` or co-located with component
- Tests: `{name}.spec.ts` (e.g., `auth.spec.ts`, `videos.spec.ts`)

---

## Documentation References
- **Setup**: See `PROJECT_SETUP.md`, `FRONTEND_SETUP.md`, `ASPIRE_FRONTEND_SETUP.md`
- **Aspire**: See `VideoManager/ASPIRE_README.md`
- **Architecture**: See `SETUP_SUMMARY.md`
- **Agents**: See `AGENTS.md` for AI assistant guidelines
- **Testing**: See `video-manager-frontend/tests/README.md` for Playwright E2E test documentation



- Always use Aspire for local development to ensure proper configuration and service orchestration.
- always use DateTimeOffset (NOT DateTime) for timestamps in backend code.
- always ensure contracts and domain models are immutable and are defined as records.

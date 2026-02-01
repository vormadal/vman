---
applyTo: 'VideoManager/**/*.cs, VideoManager/**/*.csproj, VideoManager/**/*.config, VideoManager/**/*.json'
name: Backend Coding Style and Best Practices
description: This file describes the coding style and best practices for the Video Manager backend codebase.
---

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

### ❌ Don't Do This (Backend)
- Repository pattern over DbContext
- Generic repositories
- Service layer between handlers and DbContext
- Attribute routing (`[ApiController]`, `[Route]`)
- Using MediatR library
- FluentValidation library (use manual validation)

### Code Generation (Backend - Immich Client)
```bash
cd VideoManager\VideoManager
dotnet tool restore
dotnet kiota generate -d path/to/openapi.json -o Infrastructure/Immich/Generated
```

## File Naming Conventions
- Controllers: `{FeatureName}Controller.cs` (e.g., `AuthController.cs`)
- Features: `Features/{FeatureName}/{Action}.cs` (e.g., `Features/Auth/Login.cs`)
- Models: PascalCase (e.g., `User.cs`, `Video.cs`)
- Interfaces: `I{Name}.cs` (e.g., `IJwtService.cs`)
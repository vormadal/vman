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
- **Endpoint Organization:**
  - Create endpoint extension methods in `Endpoints/` folder
  - Pattern: `public static RouteGroupBuilder MapXxxEndpoints(this IEndpointRouteBuilder routes)`
  - Use `.WithTags("Category")` for OpenAPI categorization
  - Apply authorization to groups: `routes.MapGroup("/api/xxx").RequireAuthorization()`
  - Call in `Program.cs`: `app.MapAuthEndpoints(); app.MapTagEndpoints();`
- **Error Handling:**
  - Use `Results.Problem()` for all non-2xx responses (ProblemDetails RFC 7807)
  - Format: `Results.Problem(detail: "message", statusCode: code, title: "Title")`
  - ProblemDetails middleware configured via `app.UseExceptionHandler()` and `app.UseStatusCodePages()`

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
- **Endpoint definition anti-patterns:**
  - ❌ Defining endpoints inline in `Program.cs` (use extension methods in `Endpoints/` folder)
  - ❌ Using `.WithOpenApi()` (deprecated in .NET 10, use `.WithTags()` instead)
  - ❌ Returning anonymous objects for errors (use `Results.Problem()` with ProblemDetails)
  - ❌ Applying `[Authorize]` to individual endpoints (apply to route groups instead)

### Code Generation (Backend - Immich Client)
```bash
cd VideoManager\VideoManager
dotnet tool restore
dotnet kiota generate -d path/to/openapi.json -o Infrastructure/Immich/Generated
```

## File Naming Conventions
- Endpoints: `Endpoints/{FeatureName}Endpoints.cs` (e.g., `Endpoints/AuthEndpoints.cs`)
- Features: `Features/{FeatureName}/{Action}.cs` (e.g., `Features/Auth/Login.cs`)
- Models: PascalCase (e.g., `User.cs`, `Video.cs`)
- Interfaces: `I{Name}.cs` (e.g., `IJwtService.cs`)
## Endpoint Organization Pattern

### Structure
```
Endpoints/
├── AuthEndpoints.cs       # Authentication (register, login)
├── TagEndpoints.cs        # Tag CRUD operations
└── ItemEndpoints.cs       # Item operations
```

### Extension Method Template
```csharp
using VManBackend.Features.Auth;
using VManBackend.Mediator;

namespace VManBackend.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost("/register", async (Register.Request request, IMediator mediator) =>
        {
            if (!Register.Validator.Validate(request, out var error))
            {
                return Results.Problem(
                    detail: error,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Validation Error"
                );
            }

            var response = await mediator.Send(request);
            return response != null 
                ? Results.Ok(response) 
                : Results.Problem(
                    detail: "Email already in use",
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Registration Failed"
                );
        })
        .WithName("Register");

        return group;
    }
}
```

### Program.cs Usage
```csharp
using VManBackend.Endpoints;

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapControllers();

// Map API Endpoints
app.MapAuthEndpoints();
app.MapTagEndpoints();
app.MapItemEndpoints();

app.Run();
```

### Best Practices
- One endpoint file per feature area (Auth, Tags, Items, etc.)
- Return `RouteGroupBuilder` for fluent chaining (optional)
- Use `.WithTags()` for OpenAPI categorization (NOT `.WithOpenApi()`)
- Apply authorization to entire group: `.RequireAuthorization()`
- Keep endpoint logic minimal - delegate to handlers via mediator
- Use `Results.Problem()` for all error responses (ProblemDetails format)

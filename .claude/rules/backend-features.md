---
paths: "**/Features/**/*.cs"
---

# Backend Feature Pattern (Vertical Slice)

Each feature is self-contained in `VManBackend/Features/`. A single file contains Request, Response, Validator, and Handler.

## Template

```csharp
// Features/{FeatureName}/{Action}.cs
namespace VManBackend.Features.{FeatureName};

public static class {Action}
{
    public record Request(/* parameters */) : IRequest<Response?>;
    public record Response(/* return data */);

    public static class Validator
    {
        public static bool Validate(Request request, out string? error)
        {
            // Manual validation logic - do NOT use FluentValidation
            error = null;
            return true;
        }
    }

    public class Handler(ApplicationDbContext db)
        : IRequestHandler<Request, Response?>
    {
        public async Task<Response?> Handle(Request request, CancellationToken cancellationToken)
        {
            // Implementation using DbContext directly (no repository pattern)
        }
    }
}
```

## Rules

- Use custom mediator in `Mediator/` -- do NOT use MediatR library
- Use records for Request/Response (immutable)
- Use `DateTimeOffset` for all timestamps (not DateTime)
- Return nullable Response to indicate failure (null = error case)
- Use `ApplicationDbContext` directly in handlers (no repository pattern)
- Register new handlers in `Program.cs` with `builder.Services.AddRequestHandler<>()`
- Manual validation only -- do NOT use FluentValidation library
- Use `IHttpContextAccessor` when you need the current user in a handler

## Existing Features

```
Features/
├── Admin/              # User management, invites, roles (6 handlers)
├── Authentication/     # Login, invite acceptance, profile (4 handlers)
├── Collections/        # Media collections, ordering, Shotcut export (8 handlers)
├── Items/              # Media items, tagging (5 handlers)
├── People/             # Face recognition data from Immich (2 handlers)
├── Sync/               # Background sync with Immich (3 handlers)
└── Tags/               # CRUD for global tags (5 handlers)
```

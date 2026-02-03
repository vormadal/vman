---
paths: "**/Features/**/*.cs"
---

# Backend Feature Pattern (Vertical Slice)

Use this exact pattern for all features:

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

- Use custom mediator in `Mediator/` - do NOT use MediatR library
- Handlers are auto-registered as scoped services
- Use records for Request/Response (immutable)
- Use `DateTimeOffset` for all timestamps (not DateTime)
- Return nullable Response to indicate failure (null = error case)
- Use `ApplicationDbContext` directly in handlers

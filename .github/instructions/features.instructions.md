---
applyTo: '**/Features/**/*.cs'
---

#### Code Organization Pattern
```csharp
// Features/{FeatureName}/{Action}.cs
namespace VideoManager.Features.{FeatureName};

public static class {Action}
{
    public record Request : IRequest<Response>(/* parameters */);
    public record Response(/* return data */);
    
    // Validation (NOT using FluentValidation currently)
    public class Validator
    {
        public static bool Validate(Request request, out string? error)
        {
            // Manual validation logic
        }
    }
    
    // Business logic
    public class Handler(ApplicationDbContext db, /* other dependencies */) 
        : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            // Implementation
        }
    }
}
```

### Custom Mediator Pattern
- **Do NOT use MediatR library** - custom implementation exists
- Location: `Infrastructure/Mediator/`
- Request types: `IRequest<TResponse>` or `IRequest` (no response)
- Handlers: `IRequestHandler<TRequest, TResponse>` or `IRequestHandler<TRequest>`
- Registration: Handlers auto-registered as scoped services

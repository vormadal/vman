---
applyTo: 'VideoManager/**/*'
name: GitHub Copilot Instructions - VideoManager API
description: Instructions for generating code for the VideoManager .NET 10 MVC API project using Vertical Slice Architecture.
---

## Project Context
This is a .NET 10 MVC API for managing videos from multiple providers (OneDrive, Immich) with thumbnail and GIF generation capabilities. The project uses Vertical Slice Architecture and PostgreSQL database.

## Architecture Guidelines

### Vertical Slice Architecture
- Each feature is a self-contained vertical slice in `Features/` folder
- Group all related code (models, handlers, validators, controllers) within the feature folder
- Avoid cross-feature dependencies; use shared code in `Common/` or `Infrastructure/`
- Each slice should contain:
  - Request/Response DTOs
  - Validator (FluentValidation)
  - Handler (business logic)
  - Controller endpoint

### Code Organization Pattern
```csharp
// Features/{FeatureName}/{Action}.cs
namespace VideoManager.API.Features.{FeatureName};

public static class {Action}
{
    public record Request(/* parameters */);
    public record Response(/* return data */);
    
    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            // Validation rules
        }
    }
    
    public class Handler : IRequestHandler<Request, Response>
    {
        // Dependencies via constructor injection
        // Business logic implementation
    }
}
```

## Common Patterns to Follow

### Repository Pattern
- **Avoid** - DbContext already implements repository pattern
- Use DbContext directly in handlers

### CQRS with custom Mediator implementation
- Separate commands (write) from queries (read)
- Commands: `public record CreateVideoCommand(...) : IRequest<VideoDto>`
- Queries: `public record GetVideosQuery(...) : IRequest<List<VideoDto>>`
- Handlers implement `IRequestHandler<TRequest, TResponse>` or `IRequestHandler<TRequest>` (when no response)

### Result Pattern (Optional)
```csharp
public record Result<T>(bool IsSuccess, T? Value, string? Error);
```


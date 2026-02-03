---
paths: "**/Endpoints/**/*.cs"
---

# Backend Endpoint Pattern

Define endpoints as extension methods, never inline in Program.cs:

```csharp
namespace VManBackend.Endpoints;

public static class {Feature}Endpoints
{
    public static RouteGroupBuilder Map{Feature}Endpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/{feature}")
            .WithTags("{Feature}");

        // Apply auth to entire group if needed
        // .RequireAuthorization();

        group.MapPost("/action", async (Request request, IMediator mediator) =>
        {
            if (!Validator.Validate(request, out var error))
            {
                return Results.Problem(
                    detail: error,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Validation Error"
                );
            }

            var response = await mediator.Send(request);
            return response is not null
                ? Results.Ok(response)
                : Results.Problem(
                    detail: "Operation failed",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Error"
                );
        })
        .WithName("ActionName");

        return group;
    }
}
```

## Rules

- Use `Results.Problem()` for ALL errors (ProblemDetails RFC 7807)
- Use `.WithTags()` for OpenAPI grouping - do NOT use `.WithOpenApi()` (deprecated)
- Apply `.RequireAuthorization()` to route groups, not individual endpoints
- Keep endpoint logic minimal - delegate to handlers via mediator
- Register in Program.cs: `app.Map{Feature}Endpoints();`

---
paths: "**/Endpoints/**/*.cs"
---

# Backend Endpoint Pattern

Define endpoints as extension methods in `VManBackend/Endpoints/`, never inline in Program.cs.

## Template

```csharp
namespace VManBackend.Endpoints;

public static class {Feature}Endpoints
{
    public static RouteGroupBuilder Map{Feature}Endpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/{feature}")
            .WithTags("{Feature}");

        // Apply auth to entire group
        // .RequireAuthorization();
        // Or for admin-only: .RequireAuthorization("AdminOnly");

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
- Use `.WithTags()` for OpenAPI grouping -- do NOT use `.WithOpenApi()` (deprecated)
- Apply `.RequireAuthorization()` to route groups, not individual endpoints
- Use `"AdminOnly"` policy for admin-only route groups
- Keep endpoint logic minimal -- delegate to handlers via mediator
- Register in Program.cs: `app.Map{Feature}Endpoints();`

## API Reference

### Authentication (public, except complete-profile)
- `POST /api/auth/register` -- disabled, returns 403 (invite-only)
- `POST /api/auth/login`
- `POST /api/auth/accept-invite`
- `POST /api/auth/complete-profile` -- requires auth

### Admin (requires AdminOnly policy)
- `POST /api/admin/invites` | `GET /api/admin/invites`
- `GET /api/admin/users`
- `POST /api/admin/users/{userId}/block` | `POST .../unblock`
- `PUT /api/admin/users/{userId}/role`

### Tags (requires auth)
- `GET /api/tags` | `POST /api/tags`
- `GET /api/tags/{id}` | `PUT /api/tags/{id}` | `DELETE /api/tags/{id}`

### Items (requires auth)
- `GET /api/items` -- filters: provider, type, untagged, tagId, personId, sortBy, sortDescending, page, pageSize
- `GET /api/items/{provider}/{id}`
- `POST /api/items/{provider}/{id}/tags` | `DELETE .../tags/{tagId}`

### Collections (requires auth)
- `GET /api/collections` | `POST /api/collections`
- `GET /api/collections/{id}` | `DELETE /api/collections/{id}`
- `POST /api/collections/{id}/items` | `DELETE .../items/{itemId}`
- `PUT /api/collections/{id}/items/reorder`
- `GET /api/collections/{id}/export/shotcut` -- returns ZIP

### Sync (requires auth)
- `POST /api/sync` | `GET /api/sync/status` | `POST /api/sync/{jobId}/cancel`

### People (requires auth)
- `GET /api/people` -- filters: search, page, pageSize
- `GET /api/people/{id}`

### Providers (requires auth)
- `GET /api/providers/{provider}/items/{id}/thumbnail`
- `GET /api/providers/{provider}/items/{id}/preview`

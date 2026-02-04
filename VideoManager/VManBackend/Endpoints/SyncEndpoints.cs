using VManBackend.Features.Sync;
using VManBackend.Mediator;

namespace VManBackend.Endpoints;

public static class SyncEndpoints
{
    public static RouteGroupBuilder MapSyncEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/sync")
            .RequireAuthorization()
            .WithTags("Sync");

        group.MapPost("/", async (IMediator mediator, TriggerSync.Request? request) =>
        {
            request ??= new TriggerSync.Request();

            if (!TriggerSync.Validator.Validate(request, out var error))
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
                    detail: "Failed to trigger sync",
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Sync Error"
                );
        })
        .WithName("TriggerSync");

        group.MapGet("/status", async (IMediator mediator, Guid? jobId, string provider = "immich") =>
        {
            var request = new GetSyncStatus.Request(jobId, provider);

            if (!GetSyncStatus.Validator.Validate(request, out var error))
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
                    detail: "No sync job found",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found"
                );
        })
        .WithName("GetSyncStatus");

        group.MapPost("/{jobId:guid}/cancel", async (IMediator mediator, Guid jobId) =>
        {
            var request = new CancelSync.Request(jobId);

            if (!CancelSync.Validator.Validate(request, out var error))
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
                    detail: "Sync job not found",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found"
                );
        })
        .WithName("CancelSync");

        return group;
    }
}

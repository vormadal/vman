using VManBackend.Features.Collections;
using VManBackend.Mediator;

namespace VManBackend.Endpoints;

public static class CollectionEndpoints
{
    public static RouteGroupBuilder MapCollectionEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/collections")
            .RequireAuthorization()
            .WithTags("Collections");

        group.MapGet("/", async (IMediator mediator, int page = 1, int pageSize = 50) =>
        {
            var request = new GetCollections.Request(page, pageSize);
            if (!GetCollections.Validator.Validate(request, out var error))
            {
                return Results.Problem(
                    detail: error,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Validation Error"
                );
            }

            var response = await mediator.Send(request);
            return Results.Ok(response);
        })
        .WithName("GetCollections");

        group.MapGet("/{id:guid}", async (IMediator mediator, Guid id) =>
        {
            var request = new GetCollectionById.Request(id);
            if (!GetCollectionById.Validator.Validate(request, out var error))
            {
                return Results.Problem(
                    detail: error,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Validation Error"
                );
            }

            try
            {
                var response = await mediator.Send(request);
                return Results.Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Collection Not Found"
                );
            }
        })
        .WithName("GetCollectionById");

        group.MapPost("/", async (IMediator mediator, CreateCollection.Request request) =>
        {
            if (!CreateCollection.Validator.Validate(request, out var error))
            {
                return Results.Problem(
                    detail: error,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Validation Error"
                );
            }

            var response = await mediator.Send(request);
            return Results.Created($"/api/collections/{response.Id}", response);
        })
        .WithName("CreateCollection");

        group.MapPost("/{id:guid}/items", async (IMediator mediator, Guid id, AddItemToCollection.Request request) =>
        {
            if (id != request.CollectionId)
            {
                return Results.Problem(
                    detail: "Collection ID mismatch",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Validation Error"
                );
            }

            if (!AddItemToCollection.Validator.Validate(request, out var error))
            {
                return Results.Problem(
                    detail: error,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Validation Error"
                );
            }

            try
            {
                var response = await mediator.Send(request);
                return Results.Created($"/api/collections/{id}/items/{response.Id}", response);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found"
                );
            }
        })
        .WithName("AddItemToCollection");

        group.MapPatch("/{collectionId:guid}/items/{itemId:guid}/note", async (
            IMediator mediator,
            Guid collectionId,
            Guid itemId,
            UpdateCollectionItemNote.Request request) =>
        {
            if (collectionId != request.CollectionId || itemId != request.ItemId)
            {
                return Results.Problem(
                    detail: "ID mismatch",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Validation Error"
                );
            }

            if (!UpdateCollectionItemNote.Validator.Validate(request, out var error))
            {
                return Results.Problem(
                    detail: error,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Validation Error"
                );
            }

            try
            {
                var response = await mediator.Send(request);
                return Results.Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found"
                );
            }
        })
        .WithName("UpdateCollectionItemNote");

        group.MapDelete("/{collectionId:guid}/items/{itemId:guid}", async (IMediator mediator, Guid collectionId, Guid itemId) =>
        {
            var request = new RemoveItemFromCollection.Request(collectionId, itemId);
            if (!RemoveItemFromCollection.Validator.Validate(request, out var error))
            {
                return Results.Problem(
                    detail: error,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Validation Error"
                );
            }

            try
            {
                var response = await mediator.Send(request);
                return Results.Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found"
                );
            }
        })
        .WithName("RemoveItemFromCollection");

        group.MapPut("/{id:guid}/items/reorder", async (IMediator mediator, Guid id, UpdateCollectionItemOrder.Request request) =>
        {
            if (id != request.CollectionId)
            {
                return Results.Problem(
                    detail: "Collection ID mismatch",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Validation Error"
                );
            }

            if (!UpdateCollectionItemOrder.Validator.Validate(request, out var error))
            {
                return Results.Problem(
                    detail: error,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Validation Error"
                );
            }

            try
            {
                var response = await mediator.Send(request);
                return Results.Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found"
                );
            }
        })
        .WithName("UpdateCollectionItemOrder");

        group.MapPost("/{id:guid}/items/bulk-by-filter", async (
            IMediator mediator,
            Guid id,
            [Microsoft.AspNetCore.Mvc.FromQuery] string? provider,
            [Microsoft.AspNetCore.Mvc.FromQuery] string? type,
            [Microsoft.AspNetCore.Mvc.FromQuery] Guid? tagId,
            [Microsoft.AspNetCore.Mvc.FromQuery] Guid? personId) =>
        {
            VManBackend.Infrastructure.Providers.MediaType? mediaType = null;
            if (type != null && Enum.TryParse<VManBackend.Infrastructure.Providers.MediaType>(type, true, out var parsed))
            {
                mediaType = parsed;
            }

            var request = new BulkAddFilteredItemsToCollection.Request(id, provider, mediaType, tagId, personId);
            if (!BulkAddFilteredItemsToCollection.Validator.Validate(request, out var error))
            {
                return Results.Problem(
                    detail: error,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Validation Error"
                );
            }

            try
            {
                var response = await mediator.Send(request);
                return Results.Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found"
                );
            }
        })
        .WithName("BulkAddFilteredItemsToCollection");

        group.MapDelete("/{id:guid}", async (IMediator mediator, Guid id) =>
        {
            var request = new DeleteCollection.Request(id);
            if (!DeleteCollection.Validator.Validate(request, out var error))
            {
                return Results.Problem(
                    detail: error,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Validation Error"
                );
            }

            try
            {
                var response = await mediator.Send(request);
                return Results.Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Collection Not Found"
                );
            }
        })
        .WithName("DeleteCollection");

        group.MapGet("/{id:guid}/export/shotcut", async (IMediator mediator, Guid id) =>
        {
            var request = new ExportCollectionToShotcut.Request(id);
            if (!ExportCollectionToShotcut.Validator.Validate(request, out var error))
            {
                return Results.Problem(
                    detail: error,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Validation Error"
                );
            }

            try
            {
                var response = await mediator.Send(request);
                return Results.File(
                    response.ZipStream,
                    "application/zip",
                    response.FileName
                );
            }
            catch (InvalidOperationException ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found"
                );
            }
        })
        .WithName("ExportCollectionToShotcut");

        return group;
    }
}

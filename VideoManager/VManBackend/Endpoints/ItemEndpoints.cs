using VManBackend.Common.Models;
using VManBackend.Features.Items;
using VManBackend.Mediator;
using VManBackend.Infrastructure.Providers;

namespace VManBackend.Endpoints;

public static class ItemEndpoints
{
    public static RouteGroupBuilder MapItemEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/items")
            .RequireAuthorization()
            .WithTags("Items");

        group.MapGet("/", async (IMediator mediator, string? provider = null, string? type = null,
            bool? untagged = null, Guid? tagId = null, Guid? personId = null, string? sortBy = "createdAt", bool sortDescending = true,
            int page = 1, int pageSize = 50) =>
        {
            MediaType? mediaType = null;
            if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<MediaType>(type, true, out var parsedType))
            {
                mediaType = parsedType;
            }

            var request = new GetItems.Request(provider, mediaType, untagged, tagId, personId, sortBy, sortDescending, page, pageSize);
            if (!GetItems.Validator.Validate(request, out var error))
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
        .WithName("GetItems");

        group.MapGet("/{provider}/{id}", async (IMediator mediator, string provider, string id) =>
        {
            var request = new GetItemById.Request(provider, id);
            if (!GetItemById.Validator.Validate(request, out var error))
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
                    detail: "Item not found",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Item Not Found"
                );
        })
        .WithName("GetItemById");

        group.MapPost("/{provider}/{id}/tags", async (IMediator mediator, string provider, string id, 
            AddTagToItem.Request request) =>
        {
            var fullRequest = new AddTagToItem.Request(provider, id, request.TagId);
            
            if (!AddTagToItem.Validator.Validate(fullRequest, out var error))
            {
                return Results.Problem(
                    detail: error,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Validation Error"
                );
            }

            try
            {
                var response = await mediator.Send(fullRequest);
                return Results.Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Resource Not Found"
                );
            }
        })
        .WithName("AddTagToItem");

        group.MapDelete("/{provider}/{id}/tags/{tagId:guid}", async (IMediator mediator, string provider, 
            string id, Guid tagId) =>
        {
            var request = new RemoveTagFromItem.Request(provider, id, tagId);
            
            if (!RemoveTagFromItem.Validator.Validate(request, out var error))
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
        .WithName("RemoveTagFromItem");

        return group;
    }
}

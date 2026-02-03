using VManBackend.Features.Tags;
using VManBackend.Features.Items;
using VManBackend.Mediator;

namespace VManBackend.Endpoints;

public static class TagEndpoints
{
    public static RouteGroupBuilder MapTagEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/tags")
            .RequireAuthorization()
            .WithTags("Tags");

        group.MapGet("/", async (IMediator mediator, string? search = null, int page = 1, int pageSize = 50) =>
        {
            var request = new GetTags.Request(search, page, pageSize);
            if (!GetTags.Validator.Validate(request, out var error))
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
        .WithName("GetTags");

        group.MapGet("/{id:guid}", async (IMediator mediator, Guid id) =>
        {
            var request = new GetTagById.Request(id);
            if (!GetTagById.Validator.Validate(request, out var error))
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
                    title: "Tag Not Found"
                );
            }
        })
        .WithName("GetTagById");

        group.MapPost("/", async (IMediator mediator, CreateTag.Request request) =>
        {
            if (!CreateTag.Validator.Validate(request, out var error))
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
                return Results.Created($"/api/tags/{response.Id}", response);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Tag Already Exists"
                );
            }
        })
        .WithName("CreateTag");

        group.MapPut("/{id:guid}", async (IMediator mediator, Guid id, RenameTag.Request request) =>
        {
            if (id != request.Id)
            {
                return Results.Problem(
                    detail: "ID mismatch",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Validation Error"
                );
            }

            if (!RenameTag.Validator.Validate(request, out var error))
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
                    title: "Tag Not Found"
                );
            }
        })
        .WithName("RenameTag");

        group.MapDelete("/{id:guid}", async (IMediator mediator, Guid id) =>
        {
            var request = new DeleteTag.Request(id);
            if (!DeleteTag.Validator.Validate(request, out var error))
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
                    title: "Tag Not Found"
                );
            }
        })
        .WithName("DeleteTag");

        group.MapGet("/{id:guid}/items", async (IMediator mediator, Guid id, int page = 1, int pageSize = 50) =>
        {
            var request = new GetItemsByTag.Request(id, page, pageSize);
            
            if (!GetItemsByTag.Validator.Validate(request, out var error))
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
                    title: "Tag Not Found"
                );
            }
        })
        .WithName("GetItemsByTag");

        return group;
    }
}

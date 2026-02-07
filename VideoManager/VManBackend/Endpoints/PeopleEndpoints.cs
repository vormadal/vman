using VManBackend.Features.People;
using VManBackend.Mediator;

namespace VManBackend.Endpoints;

public static class PeopleEndpoints
{
    public static RouteGroupBuilder MapPeopleEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/people")
            .RequireAuthorization()
            .WithTags("People");

        group.MapGet("/", async (IMediator mediator, string? search = null, int page = 1, int pageSize = 50) =>
        {
            var request = new GetPeople.Request(search, page, pageSize);
            if (!GetPeople.Validator.Validate(request, out var error))
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
        .WithName("GetPeople");

        group.MapGet("/{id:guid}", async (IMediator mediator, Guid id) =>
        {
            var request = new GetPersonById.Request(id);
            var response = await mediator.Send(request);

            if (response == null)
            {
                return Results.Problem(
                    detail: "Person not found",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Person Not Found"
                );
            }

            return Results.Ok(response);
        })
        .WithName("GetPersonById");

        return group;
    }
}

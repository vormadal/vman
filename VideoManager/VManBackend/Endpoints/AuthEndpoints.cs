using VManBackend.Features.Authentication;
using VManBackend.Mediator;

namespace VManBackend.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost("/register", async (Register.Request request, IMediator mediator) =>
        {
            if (!Register.Validator.Validate(request, out var error))
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
                    detail: "Email already in use",
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Registration Failed"
                );
        })
        .WithName("Register");

        group.MapPost("/login", async (Login.Request request, IMediator mediator) =>
        {
            if (!Login.Validator.Validate(request, out var error))
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
                    detail: "Invalid email or password",
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Authentication Failed"
                );
        })
        .WithName("Login");

        return group;
    }
}

using VManBackend.Features.Authentication;
using VManBackend.Mediator;
using Microsoft.AspNetCore.Authorization;

namespace VManBackend.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/auth")
            .WithTags("Authentication");

        // Disable public registration - keep endpoint but return 403
        group.MapPost("/register", () =>
        {
            return Results.Problem(
                detail: "Public registration is disabled. Please use an invite link from an administrator.",
                statusCode: StatusCodes.Status403Forbidden,
                title: "Registration Disabled"
            );
        })
        .WithName("Register");

        group.MapPost("/accept-invite", async (AcceptInvite.Request request, IMediator mediator) =>
        {
            if (!AcceptInvite.Validator.Validate(request, out var error))
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
                    detail: "Failed to accept invite. The invite may be invalid, expired, or the email is already in use.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invite Acceptance Failed"
                );
        })
        .WithName("AcceptInvite");

        group.MapPost("/complete-profile", async (CompleteProfile.Request request, IMediator mediator) =>
        {
            if (!CompleteProfile.Validator.Validate(request, out var error))
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
                    detail: "Failed to complete profile",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Profile Completion Failed"
                );
        })
        .RequireAuthorization()
        .WithName("CompleteProfile");

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

        group.MapPost("/refresh", async (RefreshTokens.Request request, IMediator mediator) =>
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return Results.Problem(
                    detail: "Refresh token is required",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Validation Error"
                );
            }

            var response = await mediator.Send(request);
            return response != null
                ? Results.Ok(response)
                : Results.Problem(
                    detail: "Invalid or expired refresh token",
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Token Refresh Failed"
                );
        })
        .WithName("RefreshToken");

        group.MapPost("/logout", async (Logout.Request request, IMediator mediator) =>
        {
            await mediator.Send(request);
            return Results.Ok();
        })
        .WithName("Logout");

        return group;
    }
}

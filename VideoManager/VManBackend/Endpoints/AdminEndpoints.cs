using VManBackend.Features.Admin;
using VManBackend.Mediator;
using Microsoft.AspNetCore.Authorization;

namespace VManBackend.Endpoints;

public static class AdminEndpoints
{
    public static RouteGroupBuilder MapAdminEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/admin")
            .WithTags("Admin")
            .RequireAuthorization("AdminOnly");

        // Invite management
        group.MapPost("/invites", async (CreateInvite.Request request, IMediator mediator) =>
        {
            if (!CreateInvite.Validator.Validate(request, out var error))
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
                    detail: "Failed to create invite. Email may already be in use.",
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Invite Creation Failed"
                );
        })
        .WithName("CreateInvite");

        group.MapGet("/invites", async (IMediator mediator) =>
        {
            var response = await mediator.Send(new GetInvites.Request());
            return response != null 
                ? Results.Ok(response) 
                : Results.Problem(
                    detail: "Failed to retrieve invites",
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Authorization Failed"
                );
        })
        .WithName("GetInvites");

        // User management
        group.MapGet("/users", async (IMediator mediator) =>
        {
            var response = await mediator.Send(new GetUsers.Request());
            return response != null 
                ? Results.Ok(response) 
                : Results.Problem(
                    detail: "Failed to retrieve users",
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Authorization Failed"
                );
        })
        .WithName("GetUsers");

        group.MapPost("/users/{userId:guid}/block", async (Guid userId, IMediator mediator) =>
        {
            var response = await mediator.Send(new BlockUser.Request(userId));
            return response != null 
                ? Results.Ok(response) 
                : Results.Problem(
                    detail: "Failed to block user",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Block User Failed"
                );
        })
        .WithName("BlockUser");

        group.MapPost("/users/{userId:guid}/unblock", async (Guid userId, IMediator mediator) =>
        {
            var response = await mediator.Send(new UnblockUser.Request(userId));
            return response != null 
                ? Results.Ok(response) 
                : Results.Problem(
                    detail: "Failed to unblock user",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Unblock User Failed"
                );
        })
        .WithName("UnblockUser");

        group.MapPut("/users/{userId:guid}/role", async (Guid userId, ChangeUserRole.Request request, IMediator mediator) =>
        {
            if (!ChangeUserRole.Validator.Validate(request, out var error))
            {
                return Results.Problem(
                    detail: error,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Validation Error"
                );
            }

            var response = await mediator.Send(request with { UserId = userId });
            return response != null 
                ? Results.Ok(response) 
                : Results.Problem(
                    detail: "Failed to change user role",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Change Role Failed"
                );
        })
        .WithName("ChangeUserRole");

        return group;
    }
}

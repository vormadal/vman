using VManBackend.Infrastructure.Immich;

namespace VManBackend.Endpoints;

public static class ProviderEndpoints
{
    public static RouteGroupBuilder MapProviderEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/providers")
            .RequireAuthorization()
            .WithTags("Providers");

        group.MapGet("/{provider}/items/{id}/thumbnail", async (
            string provider,
            string id,
            IImmichService immichService,
            CancellationToken cancellationToken) =>
        {
            if (provider != "immich")
            {
                return Results.Problem(
                    detail: "Only 'immich' provider is currently supported",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Unsupported Provider"
                );
            }

            if (!Guid.TryParse(id, out var assetId))
            {
                return Results.Problem(
                    detail: "Invalid asset ID format",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Validation Error"
                );
            }

            var stream = await immichService.GetThumbnailAsync(assetId, cancellationToken);
            if (stream == null)
            {
                return Results.Problem(
                    detail: "Thumbnail not found",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found"
                );
            }

            return Results.File(stream, "image/jpeg");
        })
        .WithName("GetItemThumbnail");

        group.MapGet("/{provider}/items/{id}/preview", async (
            string provider,
            string id,
            IImmichService immichService,
            CancellationToken cancellationToken) =>
        {
            if (provider != "immich")
            {
                return Results.Problem(
                    detail: "Only 'immich' provider is currently supported",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Unsupported Provider"
                );
            }

            if (!Guid.TryParse(id, out var assetId))
            {
                return Results.Problem(
                    detail: "Invalid asset ID format",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Validation Error"
                );
            }

            var stream = await immichService.GetPreviewAsync(assetId, cancellationToken);
            if (stream == null)
            {
                return Results.Problem(
                    detail: "Preview not found",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found"
                );
            }

            return Results.File(stream, "image/jpeg");
        })
        .WithName("GetItemPreview");

        return group;
    }
}

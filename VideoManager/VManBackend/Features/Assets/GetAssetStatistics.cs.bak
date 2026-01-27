using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Common.Models;
using VManBackend.Mediator;

namespace VManBackend.Features.Assets;

public static class GetAssetStatistics
{
    public record Request : IRequest<Response>;

    public record Response(
        int TotalAssets,
        int TotalVideos,
        int TotalImages,
        int TotalAudio,
        int TotalOther,
        int FavoriteCount,
        int ArchivedCount
    );

    public class Handler(ApplicationDbContext db) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var totalAssets = await db.ImmichAssets.CountAsync(cancellationToken);
            var totalVideos = await db.ImmichAssets.CountAsync(a => a.AssetType == AssetType.Video, cancellationToken);
            var totalImages = await db.ImmichAssets.CountAsync(a => a.AssetType == AssetType.Image, cancellationToken);
            var totalAudio = await db.ImmichAssets.CountAsync(a => a.AssetType == AssetType.Audio, cancellationToken);
            var totalOther = await db.ImmichAssets.CountAsync(a => a.AssetType == AssetType.Other, cancellationToken);
            var favoriteCount = await db.ImmichAssets.CountAsync(a => a.IsFavorite, cancellationToken);
            var archivedCount = await db.ImmichAssets.CountAsync(a => a.IsArchived, cancellationToken);

            return new Response(
                totalAssets,
                totalVideos,
                totalImages,
                totalAudio,
                totalOther,
                favoriteCount,
                archivedCount
            );
        }
    }
}

using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Common.Models;
using VManBackend.Mediator;

namespace VManBackend.Features.Assets;

public static class GetAssetById
{
    public record Request(Guid Id) : IRequest<Response?>;

    public record Response(
        Guid Id,
        string OriginalFileName,
        string OriginalPath,
        AssetType AssetType,
        DateTimeOffset CreatedAt,
        DateTimeOffset? FileCreatedAt,
        DateTimeOffset? LocalDateTime,
        string? Duration,
        int? Width,
        int? Height,
        string? Description,
        bool IsFavorite,
        bool IsArchived,
        double? Rating,
        DateTimeOffset LastSyncedAt,
        ExifDataDto? ExifData
    );

    public record ExifDataDto(
        string? Make,
        string? Model,
        string? LensModel,
        string? ExposureTime,
        double? FNumber,
        double? FocalLength,
        double? Iso,
        double? Latitude,
        double? Longitude,
        string? City,
        string? State,
        string? Country
    );

    public class Handler(ApplicationDbContext db) : IRequestHandler<Request, Response?>
    {
        public async Task<Response?> Handle(Request request, CancellationToken cancellationToken)
        {
            var asset = await db.ImmichAssets
                .Include(a => a.ExifData)
                .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (asset == null)
                return null;

            ExifDataDto? exifDto = null;
            if (asset.ExifData != null)
            {
                exifDto = new ExifDataDto(
                    asset.ExifData.Make,
                    asset.ExifData.Model,
                    asset.ExifData.LensModel,
                    asset.ExifData.ExposureTime,
                    asset.ExifData.FNumber,
                    asset.ExifData.FocalLength,
                    asset.ExifData.Iso,
                    asset.ExifData.Latitude,
                    asset.ExifData.Longitude,
                    asset.ExifData.City,
                    asset.ExifData.State,
                    asset.ExifData.Country
                );
            }

            return new Response(
                asset.Id,
                asset.OriginalFileName,
                asset.OriginalPath,
                asset.AssetType,
                asset.CreatedAt,
                asset.FileCreatedAt,
                asset.LocalDateTime,
                asset.Duration,
                asset.Width,
                asset.Height,
                asset.Description,
                asset.IsFavorite,
                asset.IsArchived,
                asset.Rating,
                asset.LastSyncedAt,
                exifDto
            );
        }
    }
}

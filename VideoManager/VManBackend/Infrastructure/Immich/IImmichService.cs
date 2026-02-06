namespace VManBackend.Infrastructure.Immich;

public interface IImmichService
{
    Task<ImmichAsset?> GetAssetAsync(Guid assetId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<ImmichAsset> GetAssetsAsync(AssetType? type = null, int? limit = null, CancellationToken cancellationToken = default);
    Task<int> GetAssetsTotalCountAsync(AssetType? type = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<ImmichAsset>> GetVideoAssetsAsync(int? limit = null, CancellationToken cancellationToken = default);
    Task UpdateAssetMetadataAsync(Guid assetId, UpdateAssetMetadata metadata, CancellationToken cancellationToken = default);
    Task UpdateAssetsMetadataAsync(IEnumerable<Guid> assetIds, UpdateAssetMetadata metadata, CancellationToken cancellationToken = default);
    Task<Stream?> GetThumbnailAsync(Guid assetId, CancellationToken cancellationToken = default);
    Task<Stream?> GetPreviewAsync(Guid assetId, CancellationToken cancellationToken = default);
    Task<Stream?> GetOriginalAssetAsync(Guid assetId, CancellationToken cancellationToken = default);
}

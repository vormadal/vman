using System.Text;
using System.Xml.Linq;
using System.IO.Compression;
using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Infrastructure.Providers;
using VManBackend.Infrastructure.Immich;
using VManBackend.Mediator;

namespace VManBackend.Features.Collections;

public static class ExportCollectionToShotcut
{
    public record Request(Guid CollectionId) : IRequest<Response>;
    
    public record Response(Stream ZipStream, string FileName);

    public class Validator
    {
        public static bool Validate(Request request, out string? error)
        {
            if (request.CollectionId == Guid.Empty)
            {
                error = "Collection ID is required";
                return false;
            }

            error = null;
            return true;
        }
    }

    public class Handler(ApplicationDbContext db, IImmichService immichService) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var collection = await db.Collections
                .Include(c => c.CollectionItems)
                .FirstOrDefaultAsync(c => c.Id == request.CollectionId, cancellationToken);

            if (collection == null)
            {
                throw new InvalidOperationException($"Collection with ID {request.CollectionId} not found");
            }

            // Get ordered items with their details
            var orderedItems = collection.CollectionItems
                .OrderBy(ci => ci.Order)
                .ToList();

            if (orderedItems.Count == 0)
            {
                throw new InvalidOperationException("Collection is empty. Add items before exporting.");
            }

            // Batch fetch all item details in a single DB query to avoid concurrent DbContext access
            var providerItemIds = orderedItems.Select(ci => ci.ProviderItemId).ToList();
            var providerNames = orderedItems.Select(ci => ci.ProviderName).Distinct().ToList();

            var dbItems = await db.Items
                .Where(i => providerNames.Contains(i.ProviderName) && providerItemIds.Contains(i.ProviderItemId))
                .Select(i => new { i.ProviderName, i.ProviderItemId, i.Type, i.OriginalFileName })
                .ToListAsync(cancellationToken);

            var matchedItems = orderedItems
                .Select(ci => dbItems.FirstOrDefault(i => i.ProviderName == ci.ProviderName && i.ProviderItemId == ci.ProviderItemId))
                .Where(i => i != null)
                .ToList();

            // Fan out Immich HTTP calls in parallel — safe because no DbContext is involved
            var metadataTasks = matchedItems.Select(async item =>
            {
                string? duration = null;
                if (item!.ProviderName.ToLower() == "immich" &&
                    (item.Type == MediaType.Video || item.Type == MediaType.Audio))
                {
                    try
                    {
                        var assetId = Guid.Parse(item.ProviderItemId);
                        var assetMetadata = await immichService.GetAssetAsync(assetId, cancellationToken);
                        duration = assetMetadata?.Duration;
                    }
                    catch
                    {
                        // If we can't get duration, we'll use placeholder
                    }
                }

                return new
                {
                    item.ProviderName,
                    item.ProviderItemId,
                    item.Type,
                    item.OriginalFileName,
                    Duration = duration
                };
            });

            var itemsWithMetadata = await Task.WhenAll(metadataTasks);
            var itemDetails = itemsWithMetadata
                .Select(item => (item.ProviderName, item.ProviderItemId, item.Type, item.OriginalFileName, item.Duration))
                .ToList();

            // Create a memory stream for the zip file
            var zipStream = new MemoryStream();
            
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                // Create assets directory in zip
                var assetFileNames = new List<(int Index, string FileName)>();
                
                // Download and add each asset to the zip
                for (int i = 0; i < itemDetails.Count; i++)
                {
                    var item = itemDetails[i];
                    
                    // Only download from Immich for now
                    if (item.ProviderName.ToLower() == "immich")
                    {
                        try
                        {
                            var assetId = Guid.Parse(item.ProviderItemId);
                            var assetStream = await immichService.GetOriginalAssetAsync(assetId, cancellationToken);
                            
                            if (assetStream != null)
                            {
                                // Get file extension from original filename or default based on type
                                var extension = Path.GetExtension(item.OriginalFileName);
                                if (string.IsNullOrEmpty(extension))
                                {
                                    extension = item.Type == MediaType.Image ? ".jpg" : 
                                               item.Type == MediaType.Video ? ".mp4" : 
                                               item.Type == MediaType.Audio ? ".mp3" : ".dat";
                                }
                                
                                var fileName = $"asset_{i:D3}{extension}";
                                var entry = archive.CreateEntry($"assets/{fileName}", CompressionLevel.NoCompression);
                                
                                using (var entryStream = entry.Open())
                                {
                                    await assetStream.CopyToAsync(entryStream, cancellationToken);
                                }
                                
                                assetFileNames.Add((i, fileName));
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log error but continue with other assets
                            // In production, proper logging should be added
                            Console.WriteLine($"Failed to download asset {item.ProviderItemId}: {ex.Message}");
                        }
                    }
                }
                
                // Generate MLT XML with local file references
                var mltXml = GenerateMltXml(collection.Name, itemDetails, assetFileNames);
                
                // Add MLT file to zip
                var mltEntry = archive.CreateEntry($"{SanitizeFileName(collection.Name)}.mlt", CompressionLevel.Optimal);
                using (var mltStream = mltEntry.Open())
                using (var writer = new StreamWriter(mltStream, Encoding.UTF8))
                {
                    await writer.WriteAsync(mltXml);
                }
            }
            
            // Reset stream position for reading
            zipStream.Position = 0;
            
            var zipFileName = $"{SanitizeFileName(collection.Name)}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.zip";
            return new Response(zipStream, zipFileName);
        }

        private static string GenerateMltXml(
            string collectionName, 
            List<(string ProviderName, string ProviderItemId, MediaType Type, string FileName, string? Duration)> items,
            List<(int Index, string FileName)> assetFileNames)
        {
            var mlt = new XElement("mlt",
                new XAttribute("version", "7.20.0"),
                new XAttribute("title", collectionName),
                new XAttribute("producer", "main_bin")
            );

            // Add profile (HD 1080p 30fps)
            var profile = new XElement("profile",
                new XAttribute("description", "HD 1080p 30 fps"),
                new XAttribute("width", "1920"),
                new XAttribute("height", "1080"),
                new XAttribute("progressive", "1"),
                new XAttribute("sample_aspect_num", "1"),
                new XAttribute("sample_aspect_den", "1"),
                new XAttribute("display_aspect_num", "16"),
                new XAttribute("display_aspect_den", "9"),
                new XAttribute("frame_rate_num", "30"),
                new XAttribute("frame_rate_den", "1"),
                new XAttribute("colorspace", "709")
            );
            mlt.Add(profile);

            // Add playlist for main track
            var playlist = new XElement("playlist",
                new XAttribute("id", "main_track")
            );

            int producerId = 0;
            var producers = new List<XElement>();

            // Add each item as a producer and playlist entry
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                
                // Find the corresponding asset file name
                var assetFile = assetFileNames.FirstOrDefault(a => a.Index == i);
                
                // Skip if asset wasn't downloaded
                if (assetFile.FileName == null)
                {
                    continue;
                }
                
                // Calculate frame count based on duration
                // Images use 5 seconds (150 frames at 30fps)
                // Videos/Audio use actual duration from metadata or 10 second placeholder
                int outFrame;
                if (item.Type == MediaType.Image)
                {
                    outFrame = 149; // 5 seconds at 30fps (150 frames - 1 for 0-indexed)
                }
                else
                {
                    // Parse duration string (format: "HH:MM:SS.mmm" or "M:SS.mmm")
                    outFrame = ParseDurationToFrames(item.Duration, frameRate: 30);
                }
                
                var producer = new XElement("producer",
                    new XAttribute("id", $"producer{producerId}"),
                    new XAttribute("in", "0"),
                    new XAttribute("out", outFrame.ToString())
                );

                // Use relative path to asset file within the zip
                var resourcePath = $"assets/{assetFile.FileName}";
                producer.Add(new XElement("property",
                    new XAttribute("name", "resource"),
                    resourcePath
                ));

                // Choose appropriate MLT service based on media type
                string mltService = item.Type switch
                {
                    MediaType.Image => "pixbuf",
                    MediaType.Video => "avformat",
                    MediaType.Audio => "avformat",
                    _ => "avformat" // Default to avformat for Other and unknown types
                };
                
                producer.Add(new XElement("property",
                    new XAttribute("name", "mlt_service"),
                    mltService
                ));

                producers.Add(producer);

                // Add to playlist
                playlist.Add(new XElement("entry",
                    new XAttribute("producer", $"producer{producerId}"),
                    new XAttribute("in", "0"),
                    new XAttribute("out", outFrame.ToString())
                ));

                producerId++;
            }

            // Add all producers before playlist
            foreach (var producer in producers)
            {
                mlt.Add(producer);
            }

            mlt.Add(playlist);

            // Add tractor (timeline)
            var tractor = new XElement("tractor",
                new XAttribute("id", "tractor0"),
                new XAttribute("in", "0"),
                new XAttribute("out", "0")
            );

            tractor.Add(new XElement("track",
                new XAttribute("producer", "main_track")
            ));

            mlt.Add(tractor);

            var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), mlt);
            return doc.ToString();
        }

        private static int ParseDurationToFrames(string? durationString, int frameRate)
        {
            // Default to 10 seconds if no duration provided
            if (string.IsNullOrWhiteSpace(durationString))
            {
                return (10 * frameRate) - 1; // 299 frames for 10 seconds at 30fps
            }

            try
            {
                // Duration format from Immich is typically "HH:MM:SS.mmm" or "M:SS.mmm"
                TimeSpan duration;
                
                if (TimeSpan.TryParse(durationString, out duration))
                {
                    // Convert to total seconds and multiply by frame rate
                    var totalFrames = (int)(duration.TotalSeconds * frameRate);
                    // Return frame count - 1 (for 0-indexed frame counting)
                    return Math.Max(0, totalFrames - 1);
                }
                
                // If parsing fails, return default
                return (10 * frameRate) - 1;
            }
            catch
            {
                // Fallback to 10 seconds on any error
                return (10 * frameRate) - 1;
            }
        }

        private static string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(fileName.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
            return sanitized.Trim();
        }
    }
}

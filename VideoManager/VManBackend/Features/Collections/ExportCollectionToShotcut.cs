using System.Text;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Infrastructure.Providers;
using VManBackend.Mediator;

namespace VManBackend.Features.Collections;

public static class ExportCollectionToShotcut
{
    public record Request(Guid CollectionId) : IRequest<Response>;
    
    public record Response(string MltXml, string FileName);

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

    public class Handler(ApplicationDbContext db, IConfiguration configuration) : IRequestHandler<Request, Response>
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

            // Fetch item details from the database
            var itemTasks = orderedItems.Select(async collectionItem =>
            {
                var item = await db.Items
                    .Where(i => i.ProviderName == collectionItem.ProviderName && 
                                i.ProviderItemId == collectionItem.ProviderItemId)
                    .Select(i => new { i.ProviderName, i.ProviderItemId, i.Type, i.OriginalFileName })
                    .FirstOrDefaultAsync(cancellationToken);
                    
                return item;
            });
            
            var items = await Task.WhenAll(itemTasks);
            var itemDetails = items
                .Where(item => item != null)
                .Select(item => (item!.ProviderName, item.ProviderItemId, item.Type, item.OriginalFileName))
                .ToList();

            // Get Immich base URL from configuration
            var immichBaseUrl = configuration["Immich:BaseUrl"] ?? throw new InvalidOperationException("Immich BaseUrl not configured");
            
            // Generate MLT XML
            var mltXml = GenerateMltXml(collection.Name, itemDetails, immichBaseUrl);
            var fileName = $"{SanitizeFileName(collection.Name)}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.mlt";

            return new Response(mltXml, fileName);
        }

        private static string GenerateMltXml(string collectionName, List<(string ProviderName, string ProviderItemId, MediaType Type, string FileName)> items, string immichBaseUrl)
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
            foreach (var item in items)
            {
                // TODO: Retrieve actual video duration from provider metadata
                // For now, using a placeholder duration of 10 seconds (300 frames at 30fps)
                // Images use 5 seconds (150 frames)
                var outFrame = item.Type == MediaType.Image ? "149" : "299";
                
                var producer = new XElement("producer",
                    new XAttribute("id", $"producer{producerId}"),
                    new XAttribute("in", "0"),
                    new XAttribute("out", outFrame)
                );

                // Add resource property with the file path or URL
                var resourceUrl = GetResourceUrl(item.ProviderName, item.ProviderItemId, immichBaseUrl);
                producer.Add(new XElement("property",
                    new XAttribute("name", "resource"),
                    resourceUrl
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
                    new XAttribute("out", outFrame)
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

        private static string GetResourceUrl(string providerName, string providerItemId, string immichBaseUrl)
        {
            // IMPORTANT: Resource URL considerations for Shotcut MLT files
            //
            // Current implementation returns Immich API URLs directly. This approach has limitations:
            //
            // 1. AUTHENTICATION: Immich asset URLs require API key authentication. Shotcut cannot
            //    automatically provide auth headers when loading these URLs. Users must either:
            //    - Configure Immich to allow unauthenticated access to specific assets (NOT recommended)
            //    - Use a proxy that adds authentication headers
            //    - Download files locally before using them in Shotcut (RECOMMENDED for production)
            //
            // 2. NETWORK RELIABILITY: Remote URLs can introduce latency and availability issues
            //    during video editing. Local file paths provide better editing performance.
            //
            // RECOMMENDED PRODUCTION APPROACH:
            // - Download all collection items to a local directory
            // - Store files with consistent naming (e.g., {collectionName}/{index}_{originalFileName})
            // - Return local file paths in the MLT instead of remote URLs
            // - Include downloaded files alongside the .mlt file when distributing the project
            //
            // The current URL-based approach works for testing but should be replaced with
            // file download logic for production use.
            
            if (providerName.ToLower() == "immich")
            {
                // Construct Immich asset original URL
                return $"{immichBaseUrl}/api/assets/{providerItemId}/original";
            }

            return $"{providerName}://{providerItemId}";
        }

        private static string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(fileName.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
            return sanitized.Trim();
        }
    }
}

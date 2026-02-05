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
            var itemDetails = new List<(string ProviderName, string ProviderItemId, MediaType Type, string FileName)>();
            
            foreach (var collectionItem in orderedItems)
            {
                var item = await db.Items
                    .FirstOrDefaultAsync(i => 
                        i.ProviderName == collectionItem.ProviderName && 
                        i.ProviderItemId == collectionItem.ProviderItemId, 
                        cancellationToken);

                if (item != null)
                {
                    itemDetails.Add((item.ProviderName, item.ProviderItemId, item.Type, item.OriginalFileName));
                }
            }

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
                var producer = new XElement("producer",
                    new XAttribute("id", $"producer{producerId}"),
                    new XAttribute("in", "0"),
                    new XAttribute("out", item.Type == MediaType.Image ? "149" : "0") // 5 seconds for images (30fps * 5 = 150 frames)
                );

                // Add resource property with the file path or URL
                var resourceUrl = GetResourceUrl(item.ProviderName, item.ProviderItemId, immichBaseUrl);
                producer.Add(new XElement("property",
                    new XAttribute("name", "resource"),
                    resourceUrl
                ));

                producer.Add(new XElement("property",
                    new XAttribute("name", "mlt_service"),
                    item.Type == MediaType.Video ? "avformat" : "pixbuf"
                ));

                producers.Add(producer);

                // Add to playlist
                playlist.Add(new XElement("entry",
                    new XAttribute("producer", $"producer{producerId}"),
                    new XAttribute("in", "0"),
                    new XAttribute("out", item.Type == MediaType.Image ? "149" : "0")
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
            // For now, we'll use a placeholder path
            // In a real implementation, you would:
            // 1. Download the file from the provider
            // 2. Store it in a temporary location
            // 3. Return the local file path
            // OR return the provider URL directly (Shotcut can handle URLs)
            
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

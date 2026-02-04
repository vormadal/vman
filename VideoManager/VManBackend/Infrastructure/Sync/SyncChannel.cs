using System.Threading.Channels;

namespace VManBackend.Infrastructure.Sync;

public record SyncRequest(Guid JobId, string ProviderName);

public class SyncChannel
{
    private readonly Channel<SyncRequest> _channel = Channel.CreateUnbounded<SyncRequest>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

    public ChannelWriter<SyncRequest> Writer => _channel.Writer;
    public ChannelReader<SyncRequest> Reader => _channel.Reader;
}

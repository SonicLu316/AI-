using System.Threading.Channels;
using AI錄音文字轉換.Models;

namespace AI錄音文字轉換.Services;

public interface ITranscriptionQueue
{
    ValueTask EnqueueAsync(TranscriptionJob job, CancellationToken cancellationToken = default);

    ValueTask<TranscriptionJob> DequeueAsync(CancellationToken cancellationToken);
}

public class TranscriptionQueue : ITranscriptionQueue
{
    private readonly Channel<TranscriptionJob> _channel;

    public TranscriptionQueue()
    {
        // Unbounded channel to keep a simple FIFO queue.
        _channel = Channel.CreateUnbounded<TranscriptionJob>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    }

    public async ValueTask EnqueueAsync(TranscriptionJob job, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(job);
        await _channel.Writer.WriteAsync(job, cancellationToken);
    }

    public async ValueTask<TranscriptionJob> DequeueAsync(CancellationToken cancellationToken)
    {
        var job = await _channel.Reader.ReadAsync(cancellationToken);
        return job;
    }
}

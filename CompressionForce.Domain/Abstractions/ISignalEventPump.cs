using CompressionForce.Domain.Entities;

namespace CompressionForce.Domain.Abstractions
{
    public interface ISignalEventPump
    {
        Task StartAsync(IEnumerable<PlcSignal> signals, CancellationToken token);
    }
}

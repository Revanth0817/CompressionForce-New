using System.Threading.Tasks;
using CompressionForce.Domain.Events;

namespace CompressionForce.Domain.Abstractions
{
    public interface ISignalEventSink
    {
        Task PublishAsync(SignalEvent signalEvent);
    }
}

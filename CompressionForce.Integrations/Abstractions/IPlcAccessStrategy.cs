using System.Collections.Generic;
using CompressionForce.Domain.Entities;

namespace CompressionForce.Integrations.Abstractions
{
    public interface IPlcAccessStrategy
    {
        void Start(IEnumerable<PlcSignal> signals);
        void Stop();
    }
}

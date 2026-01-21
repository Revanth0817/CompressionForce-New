using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Domain.PLC;

public interface IPlcProtocol
{
    Task ConnectAsync();
    Task DisconnectAsync();

    Task<bool> ReadDiscreteInputAsync(int address);
    Task<bool> ReadCoilAsync(int address);
    Task<int> ReadInputRegisterAsync(int address);
    Task<int> ReadHoldingRegisterAsync(int address);

    Task WriteCoilAsync(int address, bool value);
    Task WriteHoldingRegisterAsync(int address, int value);
}

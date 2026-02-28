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

    // Address-based (Modbus fallback)
    Task<bool> ReadDiscreteInputAsync(int address);
    Task<bool> ReadCoilAsync(int address);
    Task<int> ReadInputRegisterAsync(int address);
    Task<int> ReadHoldingRegisterAsync(int address);
    Task WriteCoilAsync(int address, bool value);
    Task WriteHoldingRegisterAsync(int address, int value);

    // Symbol-based (ADS)
    Task<bool> ReadBoolAsync(string symbolPath);
    Task<short> ReadIntAsync(string symbolPath);
    Task<float> ReadFloatAsync(string symbolPath);
    Task WriteBoolAsync(string symbolPath, bool value);
    Task WriteIntAsync(string symbolPath, short value);
    Task WriteFloatAsync(string symbolPath, float value);

    /// <summary>True when symbol-based reads are supported (ADS).</summary>
    bool SupportsSymbolPath { get; }

    /// <summary>True when the PLC connection is alive.</summary>
    bool IsConnected { get; }

    /// <summary>The PLC target address (IP or AMS Net ID).</summary>
    string Host { get; }
}
using EasyModbus;
using CompressionForce.Domain.PLC;

namespace CompressionForce.Integrations.PLC.Modbus;

public class ModbusTcpProtocol : IPlcProtocol
{
    private readonly ModbusClient _client;
    private readonly object _lock = new();

    // 🔥 STORE IP & PORT HERE
    private readonly string _ip;
    private readonly int _port;

    public ModbusTcpProtocol(string ip, int port)
    {
        _ip = ip;
        _port = port;

        _client = new ModbusClient(ip, port)
        {
            ConnectionTimeout = 3000,
            UnitIdentifier = 1
        };
    }

    // ===============================
    // CONNECTION
    // ===============================
    public Task ConnectAsync()
    {
        Console.WriteLine("PLC CONNECT ATTEMPT");

        lock (_lock)
        {
            if (!_client.Connected)
            {
                _client.Connect();
                Console.WriteLine("PLC CONNECTED");
            }
        }

        return Task.CompletedTask;
    }

    public Task DisconnectAsync()
    {
        lock (_lock)
        {
            if (_client.Connected)
                _client.Disconnect();
        }

        return Task.CompletedTask;
    }

    // ===============================
    // READ
    // ===============================
    public Task<bool> ReadDiscreteInputAsync(int address)
    {
        lock (_lock)
            return Task.FromResult(_client.ReadDiscreteInputs(address, 1)[0]);
    }

    public Task<bool> ReadCoilAsync(int address)
    {
        lock (_lock)
            return Task.FromResult(_client.ReadCoils(address, 1)[0]);
    }

    public Task<int> ReadInputRegisterAsync(int address)
    {
        lock (_lock)
            return Task.FromResult(_client.ReadInputRegisters(address, 1)[0]);
    }

    public Task<int> ReadHoldingRegisterAsync(int address)
    {
        lock (_lock)
            return Task.FromResult(_client.ReadHoldingRegisters(address, 1)[0]);
    }

    // ===============================
    // WRITE
    // ===============================
    public Task WriteCoilAsync(int address, bool value)
    {
        lock (_lock)
            _client.WriteSingleCoil(address, value);

        return Task.CompletedTask;
    }

    public Task WriteHoldingRegisterAsync(int address, int value)
    {
        lock (_lock)
            _client.WriteSingleRegister(address, value);

        return Task.CompletedTask;
    }
}

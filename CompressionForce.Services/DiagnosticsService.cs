using CompressionForce.Domain.Plc;
using CompressionForce.Domain.ViewModels;
using Microsoft.AspNetCore.Hosting;
using System.Text.Json;

namespace CompressionForce.Services;

public class DiagnosticsService
{
    private readonly PlcMapping _map;
    private readonly PlcModbusClient _plc;

    public DiagnosticsService(IWebHostEnvironment env)
    {
        // 1️⃣ Resolve JSON path
        var path = Path.Combine(env.ContentRootPath, "plc-mapping.json");

        if (!File.Exists(path))
            throw new FileNotFoundException("plc-mapping.json not found", path);

        // 2️⃣ Deserialize JSON safely (case-insensitive)
        _map = JsonSerializer.Deserialize<PlcMapping>(
            File.ReadAllText(path),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        ) ?? throw new Exception("PLC mapping JSON deserialization failed");

        // 3️⃣ Validate mandatory sections
        if (_map.Plc == null)
            throw new Exception("PLC section missing in plc-mapping.json");

        if (_map.DigitalInputs == null || _map.DigitalInputs.Count == 0)
            throw new Exception("DigitalInputs section missing or empty in JSON");

        if (_map.AnalogInputs == null || _map.AnalogInputs.Count == 0)
            throw new Exception("AnalogInputs section missing or empty in JSON");

        if (_map.Counters == null || _map.Counters.Count == 0)
            throw new Exception("Counters section missing or empty in JSON");

        // 4️⃣ HARD proof JSON is loaded
        Console.WriteLine("=== PLC JSON LOADED ===");
        Console.WriteLine($"PLC IP: {_map.Plc.Ip}, PORT: {_map.Plc.Port}");

        foreach (var kv in _map.DigitalInputs)
            Console.WriteLine($"DI MAP: {kv.Key} -> {kv.Value}");

        foreach (var kv in _map.AnalogInputs)
            Console.WriteLine($"AI MAP: {kv.Key} -> {kv.Value}");

        foreach (var kv in _map.Counters)
            Console.WriteLine($"COUNTER MAP: {kv.Key} -> {kv.Value}");

        // 5️⃣ Connect to PLC
        _plc = new PlcModbusClient(_map.Plc.Ip, _map.Plc.Port);
    }

    public DiagnosticsFixedVM Read()
    {
        var vm = new DiagnosticsFixedVM();

        /* =========================
         * DIGITAL INPUTS
         * ========================= */

        int diCount = _map.DigitalInputs.Values.Max() + 1;
        bool[] diRaw = _plc.ReadDiscreteInputs(0, diCount);

        vm.EmergencyControl = ReadDI(diRaw, "EmergencyControl");
        vm.DoorSafety = ReadDI(diRaw, "DoorSafety");
        vm.TurretDriveTrip = ReadDI(diRaw, "TurretDriveTrip");
        vm.LHSFeederDriveTrip = ReadDI(diRaw, "LhsFeederTrip");
        vm.RHSFeederDriveTrip = ReadDI(diRaw, "RhsFeederTrip");
        vm.LubeMotorTrip = ReadDI(diRaw, "LubeMotorTrip");
        vm.S1UpperCam = ReadDI(diRaw, "S1UpperCam");
        vm.S1LowerCam = ReadDI(diRaw, "S1LowerCam");
        vm.S2UpperCam = ReadDI(diRaw, "S2UpperCam");
        vm.S2LowerCam = ReadDI(diRaw, "S2LowerCam");

        vm.S1MainPenRollerPos = ReadDI(diRaw, "S1MainPenetration");
        vm.S1PrePenRollerPos = ReadDI(diRaw, "S1PrePenetration");
        vm.S2MainPenRollerPos = ReadDI(diRaw, "S2MainPenetration");
        vm.S2PrePenRollerPos = ReadDI(diRaw, "S2PrePenetration");

        vm.TurretInPosition = ReadDI(diRaw, "TurretPosition");
        vm.S1ScrapperPos = ReadDI(diRaw, "S1Scrapper");
        vm.S2ScrapperPos = ReadDI(diRaw, "S2Scrapper");

        vm.RhsPowderLevel = ReadDI(diRaw, "RhsPowderLevel");
        vm.LhsPowderLevel = ReadDI(diRaw, "LhsPowderLevel");
        vm.LubeOilLow = ReadDI(diRaw, "LubeOilLow");

        int doCount = _map.DigitalOutputs.Values.Max() + 1;
        bool[] doRaw = _plc.ReadCoils(0, doCount);

        vm.S1InitialRej = ReadDO(doRaw, "S1InitialReject");
        vm.S2InitialRej = ReadDO(doRaw, "S2InitialReject");
        vm.S1Sample = ReadDO(doRaw, "S1SampleValve");
        vm.S2Sample = ReadDO(doRaw, "S2SampleValve");
        vm.S1Reject = ReadDO(doRaw, "S1RejectValve");
        vm.S2Reject = ReadDO(doRaw, "S2RejectValve");

        vm.Lubrication = ReadDO(doRaw, "Lubrication");
        vm.RedLamp = ReadDO(doRaw, "RedLamp");
        vm.YellowLamp = ReadDO(doRaw, "YellowLamp");
        vm.GreenLamp = ReadDO(doRaw, "GreenLamp");


        

        /* =========================
         * COUNTERS (32-bit)
         * ========================= */

        int cCount = _map.Counters.Values.Max() + 2;
        int[] cr = _plc.ReadHoldingRegisters(0, cCount);

        vm.RevolutionCount = ReadCounter(cr, "RevolutionCount");
        vm.EncoderActCount = ReadCounter(cr, "EncoderActCount");

        return vm;
    }

    /* =========================
     * HELPER METHODS
     * ========================= */

    private bool ReadDI(bool[] raw, string key)
    {
        int addr = GetAddr(_map.DigitalInputs, key);
        bool value = raw[addr];

        Console.WriteLine($"DI {key} @ {addr} = {value}");
        return value;
    }


    private bool ReadDO(bool[] raw, string key)
    {
        int addr = GetAddr(_map.DigitalOutputs, key);
        return raw[addr];
    }
    private long ReadCounter(int[] raw, string key)
    {
        int addr = GetAddr(_map.Counters, key);
        return raw[addr]; // 0–65535
    }


    private int GetAddr(Dictionary<string, int> dict, string key)
    {
        if (!dict.TryGetValue(key, out int addr))
            throw new Exception($"JSON mapping missing key: {key}");

        return addr;
    }
    /* =========================
 * DIGITAL OUTPUT WRITE
 * ========================= */

    public void WriteDigitalOutput(string key, bool value)
    {
        int addr = GetAddr(_map.DigitalOutputs, key);
        _plc.WriteSingleCoil(addr, value);

        Console.WriteLine($"UI → PLC: {key} = {value}");
    }



}

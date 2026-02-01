using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Integrations.Plc
{
    public interface IPlcClient
    {
        // DIGITAL
        bool[] ReadCoils(int start, int count);
        bool[] ReadDiscreteInputs(int start, int count);

        // ANALOG
        int[] ReadInputRegisters(int start, int count);
        int[] ReadHoldingRegisters(int start, int count);

        // WRITES
        void WriteSingleCoil(int address, bool value);
        void WriteSingleRegister(int address, int value);
    }

}

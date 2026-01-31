using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Services.Interfaces
{
    public interface IPlcSignalReader
    {
        decimal ReadDecimal(string key);
    }
}

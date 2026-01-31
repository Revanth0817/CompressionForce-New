using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Services.Interfaces
{
    public interface IPlcSignalWriter
    {
        Task WriteAsync(string key, object value);
    }
}

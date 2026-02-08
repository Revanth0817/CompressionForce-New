using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Domain.Abstractions
{
    public interface IEventPumpIntervalProvider
    {
        int GetIntervalMilliseconds(UpdateClass updateClass);
    }
}

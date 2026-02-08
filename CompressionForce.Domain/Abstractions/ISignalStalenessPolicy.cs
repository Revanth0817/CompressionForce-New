using System;
using CompressionForce.Domain.Enums;

namespace CompressionForce.Domain.Abstractions
{
    public interface ISignalStalenessPolicy
    {
        TimeSpan GetMaxAge(UpdateClass updateClass);
    }
}

using System;
using System.Collections.Generic;
using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Enums;

namespace CompressionForce.Domain.Services
{
    public sealed class ConfigurableSignalStalenessPolicy
        : ISignalStalenessPolicy
    {
        private readonly Dictionary<UpdateClass, TimeSpan> _map;

        public ConfigurableSignalStalenessPolicy(
            Dictionary<UpdateClass, TimeSpan> map)
        {
            _map = map;
        }

        public TimeSpan GetMaxAge(UpdateClass updateClass)
            => _map.TryGetValue(updateClass, out var ts)
                ? ts
                : TimeSpan.FromSeconds(10);
    }
}

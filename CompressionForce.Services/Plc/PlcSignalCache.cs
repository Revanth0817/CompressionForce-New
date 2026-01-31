using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Services.Plc
{
    public sealed class PlcSignalCache
    {
        private readonly ConcurrentDictionary<string, object> _values = new();

        public bool UpdateIfChanged(string key, object value)
        {
            if (_values.TryGetValue(key, out var existing) &&
                Equals(existing, value))
                return false;

            _values[key] = value;
            return true;
        }

        public bool TryGet(string key, out object value)
            => _values.TryGetValue(key, out value);
    }
}

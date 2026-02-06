using System.Collections.Generic;
using System.Linq;
using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Entities;
using CompressionForce.Domain.Validation;

namespace CompressionForce.Integrations.Registry
{
    public sealed class PlcSignalRegistry : IPlcSignalRegistry
    {
        private readonly Dictionary<string, PlcSignal> _primaries;
        private readonly Dictionary<string, string> _aliasToPrimary;

        private readonly Dictionary<string, (PlcSignal Primary, PlcSignalRole Role)> _roles;
        public PlcSignalRegistry(IEnumerable<PlcSignal> signals)
        {
            SignalConfigurationValidator.Validate(signals);

            _primaries = signals
                .Where(s => s.IsPrimary)
                .ToDictionary(s => s.SignalId);

            _aliasToPrimary = new Dictionary<string, string>();
            _roles = new Dictionary<string, (PlcSignal, PlcSignalRole)>();

            foreach (var primary in _primaries.Values)
            {
                // Aliases
                foreach (var alias in primary.Aliases)
                {
                    _aliasToPrimary[alias] = primary.SignalId;
                }

                // 🔹 Roles
                foreach (var role in primary.Roles)
                {
                    _roles[role.Key] = (primary, role.Value);
                }
            }
        }
        public PlcSignal Resolve(string signalIdOrAlias)
        {
            if (_primaries.TryGetValue(signalIdOrAlias, out var primary))
                return primary;

            if (_aliasToPrimary.TryGetValue(signalIdOrAlias, out var id))
                return _primaries[id];

            return null;
        }

        public IReadOnlyList<PlcSignal> GetAllPrimaries()
            => _primaries.Values.ToList();

        public IReadOnlyList<PlcSignal> GetByGroup(string group)
            => _primaries.Values
                .Where(s => s.Groups.Contains(group))
                .ToList();

        public PlcSignal ResolvePrimaryForRole(string roleName)
        {
            if (_roles.TryGetValue(roleName, out var entry))
                return entry.Primary;

            throw new KeyNotFoundException($"Role '{roleName}' not found.");
        }

        public PlcSignalRole ResolveRole(string roleName)
        {
            if (_roles.TryGetValue(roleName, out var entry))
                return entry.Role;

            throw new KeyNotFoundException($"Role '{roleName}' not found.");
        }

        public IReadOnlyList<string> GetRolesByGroup(string group)
        {
            return _roles
                .Where(r => r.Value.Role.Groups.Contains(group))
                .Select(r => r.Key)
                .ToList();
        }

        public SignalReference ResolveReference(string name)
        {
            // 1️⃣ Primary SignalId
            if (_primaries.TryGetValue(name, out var primary))
            {
                return new SignalReference
                {
                    Name = name,
                    Primary = primary
                };
            }

            // 2️⃣ Alias
            if (_aliasToPrimary.TryGetValue(name, out var primaryId))
            {
                return new SignalReference
                {
                    Name = name,
                    Primary = _primaries[primaryId]
                };
            }

            // 3️⃣ Role
            if (_roles.TryGetValue(name, out var roleEntry))
            {
                return new SignalReference
                {
                    Name = name,
                    Primary = roleEntry.Primary,
                    Role = roleEntry.Role
                };
            }

            throw new KeyNotFoundException(
                $"Signal reference '{name}' not found.");
        }

        public IReadOnlyList<SignalReference> GetReferencesByGroup(string group)
        {
            var results = new List<SignalReference>();

            // Primaries in group
            results.AddRange(
                _primaries.Values
                    .Where(p => p.Groups.Contains(group))
                    .Select(p => new SignalReference
                    {
                        Name = p.SignalId,
                        Primary = p
                    })
            );

            // Roles in group
            results.AddRange(
                _roles
                    .Where(r => r.Value.Role.Groups.Contains(group))
                    .Select(r => new SignalReference
                    {
                        Name = r.Key,
                        Primary = r.Value.Primary,
                        Role = r.Value.Role
                    })
            );

            return results;
        }
    }
}

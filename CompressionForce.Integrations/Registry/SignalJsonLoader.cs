using CompressionForce.Domain.Entities;
using CompressionForce.Domain.Enums;
using CompressionForce.Domain.ValueObjects;
using System.Text.Json;
using CompressionForce.SignalRules.Enums;

namespace CompressionForce.Integrations.Registry
{
    public sealed class SignalJsonLoader
    {
        public IReadOnlyList<PlcSignal> Load(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(
                    $"signals.json not found at {filePath}");

            var json = File.ReadAllText(filePath);
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("signals", out var signalsArray))
                throw new InvalidOperationException(
                    "signals.json must contain a top-level 'signals' array");

            var result = new List<PlcSignal>();

            foreach (var element in signalsArray.EnumerateArray())
            {
                // ---------------- Mandatory ----------------
                var signalId = element.GetProperty("signalId").GetString()
                    ?? throw new InvalidOperationException("signalId is required");

                var dataType = Enum.Parse<SignalDataType>(
                    element.GetProperty("dataType").GetString()!, true);

                var updateClass = Enum.Parse<UpdateClass>(
                    element.GetProperty("updateClass").GetString()!, true);

                // ---------------- Optional ----------------
                bool isPrimary;
                if (element.TryGetProperty("isPrimary", out var isPrimaryEl))
                {
                    isPrimary = isPrimaryEl.GetBoolean();
                }
                else
                {
                    isPrimary = element.TryGetProperty("address", out _);
                }

                PlcAddress? address = null;
                if (element.TryGetProperty("address", out var addr))
                {
                    address = PlcAddress.FromJson(addr);
                }

                var aliases = element.TryGetProperty("aliases", out var aliasesEl)
                    ? aliasesEl.EnumerateArray()
                        .Select(a => a.GetString()!)
                        .ToList()
                    : new List<string>();

                var groups = element.TryGetProperty("groups", out var groupsEl)
                    ? groupsEl.EnumerateArray()
                        .Select(g => g.GetString()!)
                        .ToList()
                    : new List<string>();

                var description = element.TryGetProperty("description", out var descEl)
                    ? descEl.GetString()
                    : null;

                var roles = element.TryGetProperty("roles", out var rolesEl)
                    ? LoadRoles(rolesEl)
                    : new Dictionary<string, PlcSignalRole>();

                result.Add(new PlcSignal
                {
                    SignalId = signalId,
                    IsPrimary = isPrimary,
                    DataType = dataType,
                    UpdateClass = updateClass,
                    Address = address,
                    Aliases = aliases,
                    Groups = groups,
                    Description = description,
                    Roles = roles
                });
            }

            return result;
        }

        // ------------------------------------------------------------
        // Roles loader
        // ------------------------------------------------------------
        private static Dictionary<string, PlcSignalRole> LoadRoles(
            JsonElement rolesElement)
        {
            var roles = new Dictionary<string, PlcSignalRole>();

            foreach (var roleProp in rolesElement.EnumerateObject())
            {
                var roleName = roleProp.Name;
                var roleEl = roleProp.Value;

                var groups = roleEl.TryGetProperty("groups", out var gEl)
                    ? gEl.EnumerateArray()
                        .Select(g => g.GetString()!)
                        .ToList()
                    : new List<string>();

                var description = roleEl.TryGetProperty("description", out var dEl)
                    ? dEl.GetString()
                    : null;

                roles[roleName] = new PlcSignalRole
                {
                    RoleName = roleName,
                    Groups = groups,
                    Description = description
                };
            }

            return roles;
        }
    }
}

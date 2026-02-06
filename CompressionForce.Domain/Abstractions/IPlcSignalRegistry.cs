using System.Collections.Generic;
using CompressionForce.Domain.Entities;

namespace CompressionForce.Domain.Abstractions
{
    public interface IPlcSignalRegistry
    {
        // Resolve by SignalId OR Alias
        PlcSignal Resolve(string signalIdOrAlias);

        // Only canonical / primary signals (used for polling)
        IReadOnlyList<PlcSignal> GetAllPrimaries();

        // Used by UI / SignalR / Pages
        IReadOnlyList<PlcSignal> GetByGroup(string group);

        // Role support
        PlcSignal ResolvePrimaryForRole(string roleName);

        PlcSignalRole ResolveRole(string roleName);

        IReadOnlyList<string> GetRolesByGroup(string group);

        /// <summary>
        /// Resolves a name that may be:
        /// - Primary signalId
        /// - Alias
        /// - Role name
        /// </summary>
        SignalReference ResolveReference(string name);

        /// <summary>
        /// Returns all references (primaries + roles) belonging to a group
        /// </summary>
        IReadOnlyList<SignalReference> GetReferencesByGroup(string group);

    }
}

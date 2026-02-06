using System.Collections.Generic;
using System.Linq;

namespace CompressionForce.SignalRules.Rules
{
    public static class RoleRules
    {
        // Finds duplicate role names (global)
        public static IReadOnlyList<string> FindDuplicateRoleNames(
            IEnumerable<string> roleNames)
        {
            return roleNames
                .GroupBy(r => r)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
        }

        // Finds role names that conflict with primary signal IDs
        public static IReadOnlyList<string> FindRolePrimaryConflicts(
            IEnumerable<string> primaryIds,
            IEnumerable<string> roleNames)
        {
            var primarySet = new HashSet<string>(primaryIds);

            return roleNames
                .Where(r => primarySet.Contains(r))
                .Distinct()
                .ToList();
        }
    }
}

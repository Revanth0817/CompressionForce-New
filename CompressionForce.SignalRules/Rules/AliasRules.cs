using System.Collections.Generic;
using System.Linq;

namespace CompressionForce.SignalRules.Rules
{
    public static class AliasRules
    {
        // Finds duplicate alias names (global)
        public static IReadOnlyList<string> FindDuplicateAliases(
            IEnumerable<string> aliases)
        {
            return aliases
                .GroupBy(a => a)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
        }

        // Finds aliases that conflict with primary signal IDs
        public static IReadOnlyList<string> FindPrimaryAliasConflicts(
            IEnumerable<string> primaryIds,
            IEnumerable<string> aliases)
        {
            var primarySet = new HashSet<string>(primaryIds);

            return aliases
                .Where(a => primarySet.Contains(a))
                .Distinct()
                .ToList();
        }
    }
}

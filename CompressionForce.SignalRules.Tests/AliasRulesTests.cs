using CompressionForce.SignalRules.Rules;
using Xunit;

namespace CompressionForce.SignalRules.Tests
{
    public sealed class AliasRulesTests
    {
        [Fact]
        public void FindDuplicateAliases_DetectsDuplicates()
        {
            var aliases = new[]
            {
                "Compression_Force",
                "Loadcell",
                "Compression_Force"
            };

            var duplicates = AliasRules.FindDuplicateAliases(aliases);

            Assert.Single(duplicates);
            Assert.Contains("Compression_Force", duplicates);
        }

        [Fact]
        public void FindPrimaryAliasConflicts_DetectsConflicts()
        {
            var primaryIds = new[] { "S1_Main_Loadcell", "Door_Interlock" };
            var aliases = new[] { "Compression_Force", "Door_Interlock" };

            var conflicts = AliasRules.FindPrimaryAliasConflicts(primaryIds, aliases);

            Assert.Single(conflicts);
            Assert.Contains("Door_Interlock", conflicts);
        }

        [Fact]
        public void NoConflicts_ReturnsEmpty()
        {
            var primaryIds = new[] { "S1_Main_Loadcell" };
            var aliases = new[] { "Compression_Force", "Loadcell" };

            var conflicts = AliasRules.FindPrimaryAliasConflicts(primaryIds, aliases);

            Assert.Empty(conflicts);
        }
    }
}

using CompressionForce.SignalRules.Rules;
using Xunit;

namespace CompressionForce.SignalRules.Tests
{
    public sealed class RoleRulesTests
    {
        [Fact]
        public void FindDuplicateRoleNames_DetectsDuplicates()
        {
            var roleNames = new[]
            {
                "AutoMode_Force",
                "Manual_Force",
                "AutoMode_Force"
            };

            var duplicates = RoleRules.FindDuplicateRoleNames(roleNames);

            Assert.Single(duplicates);
            Assert.Contains("AutoMode_Force", duplicates);
        }

        [Fact]
        public void FindRolePrimaryConflicts_DetectsConflicts()
        {
            var primaryIds = new[] { "S1_Main_Loadcell" };
            var roleNames = new[] { "AutoMode_Force", "S1_Main_Loadcell" };

            var conflicts = RoleRules.FindRolePrimaryConflicts(primaryIds, roleNames);

            Assert.Single(conflicts);
            Assert.Contains("S1_Main_Loadcell", conflicts);
        }

        [Fact]
        public void ValidRoles_ReturnEmpty()
        {
            var primaryIds = new[] { "S1_Main_Loadcell" };
            var roleNames = new[] { "AutoMode_Force", "Manual_Force" };

            var conflicts = RoleRules.FindRolePrimaryConflicts(primaryIds, roleNames);

            Assert.Empty(conflicts);
        }
    }
}

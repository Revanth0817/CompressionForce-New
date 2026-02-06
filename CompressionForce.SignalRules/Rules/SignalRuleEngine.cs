using System.Collections.Generic;
using System.Linq;
using CompressionForce.SignalRules.Diagnostics;
using CompressionForce.SignalRules.Enums;

namespace CompressionForce.SignalRules.Rules
{
    public static class SignalRuleEngine
    {
        public static IReadOnlyList<SignalRuleViolation> Evaluate(
            IReadOnlyList<Model.ISignalRuleContext> signals)
        {
            var violations = new List<SignalRuleViolation>();

            ValidateUniqueSignalIds(signals, violations);
            ValidatePrimaryAddressRules(signals, violations);
            ValidateAliases(signals, violations);
            ValidateRoles(signals, violations);
            ValidateDataTypeVsLength(signals, violations);

            return violations;
        }

        // ------------------------------------------------------------
        // STRUCTURAL RULES (belong here)
        // ------------------------------------------------------------

        private static void ValidateUniqueSignalIds(
            IReadOnlyList<Model.ISignalRuleContext> signals,
            List<SignalRuleViolation> violations)
        {
            foreach (var dup in signals.GroupBy(s => s.SignalId)
                                       .Where(g => g.Count() > 1))
            {
                violations.Add(new SignalRuleViolation(
                    RuleSeverity.Error,
                    $"Duplicate SignalId found: {dup.Key}"
                ));
            }
        }

        private static void ValidatePrimaryAddressRules(
            IReadOnlyList<Model.ISignalRuleContext> signals,
            List<SignalRuleViolation> violations)
        {
            var primaries = signals.Where(s => s.IsPrimary).ToList();

            foreach (var p in primaries.Where(p => p.AddressKey == null))
            {
                violations.Add(new SignalRuleViolation(
                    RuleSeverity.Error,
                    $"Primary signal '{p.SignalId}' must define an address"
                ));
            }

            foreach (var np in signals.Where(s => !s.IsPrimary && s.AddressKey != null))
            {
                violations.Add(new SignalRuleViolation(
                    RuleSeverity.Error,
                    $"Non-primary signal '{np.SignalId}' must not define an address"
                ));
            }

            foreach (var g in primaries
                .Where(p => p.AddressKey != null)
                .GroupBy(p => p.AddressKey)
                .Where(g => g.Count() > 1))
            {
                violations.Add(new SignalRuleViolation(
                    RuleSeverity.Error,
                    $"Multiple primary signals share PLC address: {g.Key}"
                ));
            }
        }

        // ------------------------------------------------------------
        // DELEGATED RULES (pure logic elsewhere)
        // ------------------------------------------------------------

        private static void ValidateAliases(
            IReadOnlyList<Model.ISignalRuleContext> signals,
            List<SignalRuleViolation> violations)
        {
            var primaryIds = signals
                .Where(s => s.IsPrimary)
                .Select(s => s.SignalId)
                .ToList();

            var aliases = signals
                .Where(s => s.IsPrimary)
                .SelectMany(s => s.Aliases)
                .ToList();

            foreach (var msg in AliasRules.FindDuplicateAliases(aliases)
                                          .Select(a => $"Duplicate alias defined: {a}"))
            {
                violations.Add(new SignalRuleViolation(RuleSeverity.Error, msg));
            }

            foreach (var msg in AliasRules.FindPrimaryAliasConflicts(primaryIds, aliases)
                                          .Select(a => $"Alias '{a}' conflicts with a primary SignalId"))
            {
                violations.Add(new SignalRuleViolation(RuleSeverity.Error, msg));
            }
        }

        private static void ValidateRoles(
            IReadOnlyList<Model.ISignalRuleContext> signals,
            List<SignalRuleViolation> violations)
        {
            var primaryIds = signals
                .Where(s => s.IsPrimary)
                .Select(s => s.SignalId)
                .ToList();

            var roleNames = signals
                .Where(s => s.IsPrimary)
                .SelectMany(s => s.Roles.Keys)
                .ToList();

            foreach (var msg in RoleRules.FindDuplicateRoleNames(roleNames)
                                         .Select(r => $"Duplicate role name detected: {r}"))
            {
                violations.Add(new SignalRuleViolation(RuleSeverity.Error, msg));
            }

            foreach (var msg in RoleRules.FindRolePrimaryConflicts(primaryIds, roleNames)
                                         .Select(r => $"Role name '{r}' conflicts with a primary SignalId"))
            {
                violations.Add(new SignalRuleViolation(RuleSeverity.Error, msg));
            }

            // Contextual warning (correctly stays here)
            foreach (var s in signals.Where(s => s.IsPrimary))
            {
                foreach (var role in s.Roles)
                {
                    if (role.Value == null || role.Value.Count == 0)
                    {
                        violations.Add(new SignalRuleViolation(
                            RuleSeverity.Warning,
                            $"Role '{role.Key}' has no groups defined"
                        ));
                    }
                }
            }
        }

        private static void ValidateDataTypeVsLength(
            IReadOnlyList<Model.ISignalRuleContext> signals,
            List<SignalRuleViolation> violations)
        {
            foreach (var s in signals.Where(s => s.AddressKey != null))
            {
                var expected = SignalTypeRules.ExpectedRegisterLength(s.DataType);

                if (expected == null && s.AddressLength != null)
                {
                    violations.Add(new SignalRuleViolation(
                        RuleSeverity.Error,
                        $"Bool signal '{s.SignalId}' must not define address.length"
                    ));
                }
                else if (expected != null && s.AddressLength != expected)
                {
                    violations.Add(new SignalRuleViolation(
                        RuleSeverity.Error,
                        $"Signal '{s.SignalId}' has invalid length for {s.DataType}. " +
                        $"Expected {expected}, got {s.AddressLength}"
                    ));
                }
            }
        }
    }
}

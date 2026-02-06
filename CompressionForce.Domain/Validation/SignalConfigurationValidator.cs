using System;
using System.Collections.Generic;
using System.Linq;
using CompressionForce.Domain.Entities;
using CompressionForce.SignalRules.Diagnostics;
using CompressionForce.SignalRules.Rules;

namespace CompressionForce.Domain.Validation
{
    public static class SignalConfigurationValidator
    {
        public static void Validate(IEnumerable<PlcSignal> signals)
        {
            var contexts = signals
                .Select(s => new PlcSignalRuleContext(s))
                .ToList();

            var violations = SignalRuleEngine.Evaluate(contexts);

            var errors = violations
                .Where(v => v.Severity == RuleSeverity.Error)
                .Select(v => v.Message)
                .ToList();

            if (errors.Any())
            {
                throw new InvalidOperationException(
                    "Signal configuration errors:\n" +
                    string.Join("\n", errors));
            }
        }
    }
}

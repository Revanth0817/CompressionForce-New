using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using CompressionForce.SignalGeneration.Diagnostics;
using CompressionForce.SignalRules.Diagnostics;
using CompressionForce.SignalRules.Rules;

namespace CompressionForce.SignalGeneration.Validation
{
    public static class SignalToolingValidator
    {
        public static IReadOnlyList<SignalDiagnostic> Validate(
            IReadOnlyList<JObject> signals,
            ToolingMode mode)
        {
            var contexts = signals
                .Select(s => new JsonSignalRuleContext(s))
                .ToList();

            var violations = SignalRuleEngine.Evaluate(contexts);

            var diagnostics = violations
                .Select(v => new SignalDiagnostic(
                    v.Severity == RuleSeverity.Error
                        ? DiagnosticSeverity.Error
                        : DiagnosticSeverity.Warning,
                    v.Message))
                .ToList();

            if (mode == ToolingMode.Strict &&
                diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                throw new InvalidOperationException(
                    "Signal configuration has blocking errors. See diagnostics.");
            }

            return diagnostics;
        }
    }
}

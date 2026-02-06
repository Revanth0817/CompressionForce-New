using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Microsoft.CodeAnalysis;

namespace CompressionForce.SignalGeneration.Parsing
{
    internal static class SignalModelReader
    {
        public static IReadOnlyList<JObject> ReadSignals(
            GeneratorExecutionContext context)
        {
            var file = context.AdditionalFiles
                .FirstOrDefault(f => f.Path.EndsWith("signals.json"));

            if (file == null)
                throw new System.InvalidOperationException(
                    "signals.json was not provided as an AdditionalFile");

            var text = file.GetText(context.CancellationToken)?.ToString();
            if (string.IsNullOrWhiteSpace(text))
                throw new System.InvalidOperationException(
                    "signals.json is empty");

            var root = JObject.Parse(text);

            if (root["signals"] is not JArray signals)
                throw new System.InvalidOperationException(
                    "signals.json must contain a top-level 'signals' array");

            return signals
                .OfType<JObject>()
                .ToList();
        }
    }
}

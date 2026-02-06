using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CompressionForce.SignalGeneration.Generators
{
    [Generator]
    public sealed class PlcSignalsGenerator : ISourceGenerator
    {
        // Very small, safe regex to extract "signalId": "XYZ"
        private static readonly Regex SignalIdRegex =
            new Regex("\"signalId\"\\s*:\\s*\"(?<id>[A-Za-z0-9_]+)\"",
                      RegexOptions.Compiled);

        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var signalsFile = context.AdditionalFiles
                .FirstOrDefault(f => Path.GetFileName(f.Path) == "signals.json");

            if (signalsFile is null)
                return;

            var text = signalsFile.GetText(context.CancellationToken);
            if (text is null)
                return;

            var matches = SignalIdRegex.Matches(text.ToString());
            if (matches.Count == 0)
                return;

            var sb = new StringBuilder();
            sb.AppendLine("namespace CompressionForce.Domain");
            sb.AppendLine("{");
            sb.AppendLine("    public static class PlcSignals");
            sb.AppendLine("    {");

            foreach (Match match in matches)
            {
                var id = match.Groups["id"].Value;
                sb.AppendLine($"        public const string {id} = \"{id}\";");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            context.AddSource(
                "PlcSignals.g.cs",
                SourceText.From(sb.ToString(), Encoding.UTF8));
        }
    }
}

using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using CompressionForce.SignalGeneration.Diagnostics;

namespace CompressionForce.SignalGeneration.Rendering
{
    public static class SignalMapRenderer
    {
        public static string Render(
            IReadOnlyList<JObject> signals,
            IReadOnlyList<SignalDiagnostic> diagnostics)
        {
            var sb = new StringBuilder();

            sb.AppendLine("# PLC Signal Map");
            sb.AppendLine($"Generated (UTC): {DateTime.UtcNow:O}");
            sb.AppendLine();

            WriteAddressView(sb, signals);
            WriteGroupView(sb, signals);
            WriteAliasView(sb, signals);
            WriteRoleView(sb, signals);
            WriteDiagnostics(sb, diagnostics);

            return sb.ToString();
        }

        // ------------------------------------------------------------

        private static void WriteAddressView(
            StringBuilder sb,
            IReadOnlyList<JObject> signals)
        {
            sb.AppendLine("## PLC Address Map\n");

            foreach (var s in signals.Where(s => s["address"] != null))
            {
                var id = s["signalId"]?.ToString();
                var addr = s["address"]!;

                sb.AppendLine(
                    $"- **{id}** → {addr["area"]} {addr["value"]}");
            }

            sb.AppendLine();
        }

        private static void WriteGroupView(
            StringBuilder sb,
            IReadOnlyList<JObject> signals)
        {
            sb.AppendLine("## Groups\n");

            var groups = signals
                .Where(s => s["groups"] is JArray)
                .SelectMany(s =>
                    s["groups"]!.Select(g => new
                    {
                        Group = g!.ToString(),
                        Signal = s["signalId"]!.ToString()
                    }))
                .GroupBy(x => x.Group);

            foreach (var g in groups)
            {
                sb.AppendLine($"### {g.Key}");
                foreach (var s in g)
                    sb.AppendLine($"- {s.Signal}");
                sb.AppendLine();
            }
        }

        private static void WriteAliasView(
            StringBuilder sb,
            IReadOnlyList<JObject> signals)
        {
            sb.AppendLine("## Aliases\n");

            foreach (var s in signals)
            {
                if (s["aliases"] is not JArray aliases)
                    continue;

                var primary = s["signalId"]!.ToString();
                foreach (var a in aliases)
                    sb.AppendLine($"- {a} → {primary}");
            }

            sb.AppendLine();
        }

        private static void WriteRoleView(
            StringBuilder sb,
            IReadOnlyList<JObject> signals)
        {
            sb.AppendLine("## Roles\n");

            foreach (var s in signals)
            {
                if (s["roles"] is not JObject roles)
                    continue;

                var primary = s["signalId"]!.ToString();

                foreach (var r in roles.Properties())
                {
                    sb.AppendLine($"### {r.Name}");
                    sb.AppendLine($"- Primary: {primary}");

                    if (r.Value["groups"] is JArray groups)
                    {
                        sb.AppendLine(
                            "- Groups: " +
                            string.Join(", ",
                                groups.Select(g => g!.ToString())));
                    }

                    sb.AppendLine();
                }
            }
        }

        private static void WriteDiagnostics(
            StringBuilder sb,
            IReadOnlyList<SignalDiagnostic> diagnostics)
        {
            if (!diagnostics.Any())
                return;

            sb.AppendLine("## ⚠ Diagnostics\n");

            foreach (var d in diagnostics)
                sb.AppendLine($"- [{d.Severity}] {d.Message}");

            sb.AppendLine();
        }
    }
}

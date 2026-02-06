using CompressionForce.SignalGeneration;
using CompressionForce.SignalGeneration.Parsing;
using CompressionForce.SignalGeneration.Rendering;
using CompressionForce.SignalGeneration.Validation;
using CompressionForce.SignalGeneration.Diagnostics;

using Newtonsoft.Json.Linq;
using System;
using System.IO;
using static System.Runtime.InteropServices.JavaScript.JSType;

internal static class Program
{
    static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 1;
        }

        var inputPath = args[0];
        var outputPath = args.Length > 1 ? args[1] : "SignalMap.md";
        var mode = args.Length > 2 && args[2].Equals("strict", StringComparison.OrdinalIgnoreCase)
            ? ToolingMode.Strict
            : ToolingMode.Preview;

        if (!File.Exists(inputPath))
        {
            Console.Error.WriteLine($"❌ File not found: {inputPath}");
            return 1;
        }

        try
        {
            var json = File.ReadAllText(inputPath);
            var root = JObject.Parse(json);

            if (root["signals"] is not JArray signalArray)
                throw new InvalidOperationException("Missing top-level 'signals' array");

            var signals = signalArray.Cast<JObject>().ToList();

            var diagnostics = SignalToolingValidator.Validate(signals, mode);
            var markdown = SignalMapRenderer.Render(signals, diagnostics);

            File.WriteAllText(outputPath, markdown);

            Console.WriteLine($"✅ Signal map generated: {outputPath}");

            var errors = diagnostics.Count(d => d.Severity == DiagnosticSeverity.Error);
            var warnings = diagnostics.Count(d => d.Severity == DiagnosticSeverity.Warning);

            Console.WriteLine($"ℹ️  Errors: {errors}, Warnings: {warnings}");

            return errors > 0 && mode == ToolingMode.Strict ? 2 : 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"❌ {ex.Message}");
            return 1;
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine("""
Usage:
  signaltool <signals.json> [output.md] [preview|strict]

Examples:
  signaltool signals.json
  signaltool signals.json SignalMap.md
  signaltool signals.json SignalMap.md strict
""");
    }
}

using Microsoft.CodeAnalysis;

namespace CompressionForce.SignalGeneration
{
    public enum ToolingMode
    {
        Strict,
        Preview
    }

    internal static class ToolingModeReader
    {
        public static ToolingMode Read(GeneratorExecutionContext context)
        {
            if (context.AnalyzerConfigOptions.GlobalOptions
                .TryGetValue("build_property.SignalToolingMode", out var value))
            {
                if (string.Equals(value, "preview", System.StringComparison.OrdinalIgnoreCase))
                    return ToolingMode.Preview;
            }

            return ToolingMode.Strict;
        }
    }
}

using CompressionForce.SignalGeneration.Diagnostics;
using CompressionForce.SignalGeneration.Parsing;
using CompressionForce.SignalGeneration.Rendering;
using CompressionForce.SignalGeneration.Validation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;


namespace CompressionForce.SignalGeneration.Generators
{
    [Generator]
    public sealed class SignalMapGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context) { }



public void Execute(GeneratorExecutionContext context)
    {
        var signals = SignalModelReader.ReadSignals(context);

        var diagnostics = SignalToolingValidator.Validate(
            signals,
            ToolingMode.Preview);

        var markdown = SignalMapRenderer.Render(signals, diagnostics);

        context.AddSource(
            "SignalMap.md.g.cs",
            SourceText.From(
                $"/*\n{markdown}\n*/",
                Encoding.UTF8));
    }
    // ------------------------------------------------------------

}
}

namespace CompressionForce.SignalGeneration.Diagnostics
{

    public sealed class SignalDiagnostic
    {
        public DiagnosticSeverity Severity { get; }
        public string Message { get; }

        public SignalDiagnostic(DiagnosticSeverity severity, string message)
        {
            Severity = severity;
            Message = message;
        }
    }
}

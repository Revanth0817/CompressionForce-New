namespace CompressionForce.SignalRules.Diagnostics
{
    public enum RuleSeverity
    {
        Error,
        Warning
    }

    public sealed class SignalRuleViolation
    {
        public RuleSeverity Severity { get; }
        public string Message { get; }

        public SignalRuleViolation(RuleSeverity severity, string message)
        {
            Severity = severity;
            Message = message;
        }
    }
}

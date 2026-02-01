namespace CompressionForce.Domain.Plc
{
    public sealed class PlcPollBatch
    {
        public PlcRegisterType RegisterType { get; }
        public PollingClass Polling { get; }
        public IReadOnlyList<int> Addresses { get; }

        public int IntervalMs => Polling switch
        {
            PollingClass.Critical => 50,
            PollingClass.Fast => 100,
            PollingClass.Normal => 250,
            PollingClass.Slow => 1000,
            _ => 250
        };

        public PlcPollBatch(
            PlcRegisterType registerType,
            PollingClass polling,
            IReadOnlyList<int> addresses)
        {
            RegisterType = registerType;
            Polling = polling;
            Addresses = addresses;
        }
    }
}

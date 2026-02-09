using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Entities;
using CompressionForce.Domain.Enums;
using CompressionForce.Integrations.Quality;

namespace CompressionForce.Integrations.AccessStrategies.Polling
{
    public sealed class PollingAccessStrategy
    {
        private readonly IPlcClient _plcClient;
        private readonly IPlcSignalCache _cache;
        private readonly IPollingIntervalProvider _intervalProvider;


        private readonly List<CancellationTokenSource> _tokens = new();

        public PollingAccessStrategy(
            IPlcClient plcClient,
            IPlcSignalCache cache,
            IPollingIntervalProvider intervalProvider)
        {
            _plcClient = plcClient;
            _cache = cache;
            _intervalProvider = intervalProvider;
        }

        public void Start(IEnumerable<PlcSignal> allSignals)
        {
            //Console.WriteLine("🚀 PollingAccessStrategy.Start() called");
            var groups = allSignals.GroupBy(s => s.UpdateClass);

            foreach (var group in groups)
            {
  
                var intervalMs = _intervalProvider.GetIntervalMilliseconds(group.Key);
                //Console.WriteLine($"⏱ Polling group {group.Key} every {intervalMs} ms, signals: {group.Count()}");
                var cts = new CancellationTokenSource();
                _tokens.Add(cts);

                var worker = new PollingWorker(
                    intervalMs,
                    group.ToList(),
                    _plcClient,
                    _cache
                );


                _ = worker.RunAsync(cts.Token);
            }
        }

        public void Stop()
        {
            foreach (var token in _tokens)
                token.Cancel();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Entities;
using CompressionForce.Domain.Events;
using CompressionForce.Integrations.Quality;

namespace CompressionForce.Integrations.Events
{
    public sealed class SignalEventPump : ISignalEventPump
    {
        private readonly IPlcSignalCache _cache;
        private readonly IPlcSignalRegistry _registry;
        private readonly SignalQualityEvaluator _qualityEvaluator;
        private readonly ISignalEventSink _sink;
        private readonly IEventPumpIntervalProvider _intervalProvider;

        public SignalEventPump(
            IPlcSignalCache cache,
            IPlcSignalRegistry registry,
            SignalQualityEvaluator qualityEvaluator,
            ISignalEventSink sink,
            IEventPumpIntervalProvider intervalProvider)
        {
            _cache = cache;
            _registry = registry;
            _qualityEvaluator = qualityEvaluator;
            _sink = sink;
            _intervalProvider = intervalProvider;
        }

        public Task StartAsync(IEnumerable<PlcSignal> signals, CancellationToken token)
        {
            //Console.WriteLine("📡 SignalEventPump.StartAsync() called");

            var groups = signals.GroupBy(s => s.UpdateClass);

            foreach (var group in groups)
            {

                var interval =
                    _intervalProvider.GetIntervalMilliseconds(group.Key);
                //Console.WriteLine($"📡 EventPump group {group.Key} every {interval} ms, signals: {group.Count()}");
                _ = RunGroupAsync(group.ToList(), interval, token);
            }

            return Task.CompletedTask;
        }

        private async Task RunGroupAsync(
            IReadOnlyList<PlcSignal> signals,
            int intervalMs,
            CancellationToken token)
        {
            using var timer =
                new PeriodicTimer(TimeSpan.FromMilliseconds(intervalMs));
            //Console.WriteLine("Custom PRINT");
            while (await timer.WaitForNextTickAsync(token))
            {
                //
                //Console.WriteLine($"⏰ Event tick {group.Key} @ {DateTime.UtcNow:HH:mm:ss.fff}");

                foreach (var signal in signals)
                {
                    var value = _cache.Get(signal.SignalId);
                    //Console.WriteLine($"📦 Cache read {signal.SignalId} => {(value == null ? "NULL" : value.Value)}");


                    var quality =
                        _qualityEvaluator.Evaluate(signal, value);

                    var evt = new SignalEvent(
                        signal.SignalId,
                        value?.Value,
                        quality,
                        value?.TimestampUtc ?? DateTime.UtcNow);

                    //Console.WriteLine($"[SignalEventPump] {signal.SignalId} = {value?.Value} ({quality}) : 2");
                    await _sink.PublishAsync(evt);
                    //Console.WriteLine( $"[EVENT] {evt.SignalId} = {evt.Value}, {evt.Quality}");
                }
            }
        }
    }
}

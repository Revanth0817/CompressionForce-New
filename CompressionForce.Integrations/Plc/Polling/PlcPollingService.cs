using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Plc;
using CompressionForce.Services.Interfaces;
using CompressionForce.Services.Plc;
using Microsoft.Extensions.Hosting;

namespace CompressionForce.Integrations.Plc.Polling
{
    public sealed class PlcPollingService : BackgroundService
    {

        private readonly PlcSignalCache _cache;
        private readonly IPlcRealtimeNotifier _notifier;
        private readonly PlcPollPlan _plan;
        private readonly IPlcBatchReader _batchReader;

        /*public PlcPollingService(
            IPlcClient plc,
            PlcSignalCache cache,
            IPlcRealtimeNotifier notifier,
            PlcPollPlan plan)
        {
            _plc = plc;
            _cache = cache;
            _notifier = notifier;
            _plan = plan;
        }
        */
        public PlcPollingService(
            IPlcBatchReader batchReader,
            PlcSignalCache cache,
            IPlcRealtimeNotifier notifier,
            PlcPollPlan plan)
        {
            _batchReader = batchReader;
            _cache = cache;
            _notifier = notifier;
            _plan = plan;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                foreach (var batch in _plan.Batches)
                {
                    //ExecuteBatch(batch);
                    //await Task.Delay(batch.IntervalMs, ct);
                    var values = _batchReader.ReadBatch(batch);

                    foreach (var (key, value) in values)
                    {
                        if (_cache.UpdateIfChanged(key, value))
                        {
                            await _notifier.NotifyAsync(key, value);
                        }
                    }

                }
                await Task.Delay(50, ct);
            }
        }

        /*private void Publish(string key, object value)
        {
            if (_cache.UpdateIfChanged(key, value))
                _notifier.NotifyAsync(key, value);
        }*/
    }
}

using CompressionForce.Domain.Plc;
using Microsoft.Extensions.Hosting;
using CompressionForce.Services.Plc;
using CompressionForce.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Integrations.Plc.Polling
{
    public sealed class PlcPollingService : BackgroundService
    {
        private readonly IPlcClient _plc;
        private readonly PlcSignalCache _cache;
        private readonly IPlcRealtimeNotifier _notifier;
        private readonly PlcPollPlan _plan;

        public PlcPollingService(
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

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                foreach (var batch in _plan.Batches)
                {
                    ExecuteBatch(batch);
                    await Task.Delay(batch.IntervalMs, ct);
                }
            }
        }

        private void ExecuteBatch(PlcPollBatch batch)
        {
            foreach (var address in batch.Addresses)
            {
                if (batch.RegisterType == PlcRegisterType.Coil)
                {
                    var v = _plc.ReadCoils(address, 1)[0];
                    Publish($"COIL_{address}", v);
                }
                else
                {
                    var v = _plc.ReadHoldingRegisters(address, 1)[0];
                    Publish($"HR_{address}", v);
                }
            }
        }

        private void Publish(string key, object value)
        {
            if (_cache.UpdateIfChanged(key, value))
                _notifier.NotifyAsync(key, value);
        }
    }
}

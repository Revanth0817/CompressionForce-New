using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Domain.Plc
{
    public sealed class PlcPollPlan
    {
        public IReadOnlyList<PlcPollBatch> Batches { get; }

        private PlcPollPlan(List<PlcPollBatch> batches)
        {
            Batches = batches;
        }

        public static PlcPollPlan Create(IEnumerable<PlcSignalDefinition> signals)
        {
            var batches = signals
                .GroupBy(s => new { s.Type, s.Polling })
                .Select(g =>
                    new PlcPollBatch(
                        g.Key.Type,
                        g.Key.Polling,
                        g.Select(x => x.Address).Distinct().OrderBy(x => x).ToList()
                    )
                )
                .ToList();

            return new PlcPollPlan(batches);
        }
    }
}

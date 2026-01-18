using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Services.DTOs.Batch
{
    public class BatchSummaryDTO
    {
        public string BatchCode { get; set; }
        public int BatchQty { get; set; }
        public int GoodQty { get; set; }
        public int RejectedQty { get; set; }
        public string BatchStatus { get; set; }
        public int TabletQty { get; set; }
    }
}

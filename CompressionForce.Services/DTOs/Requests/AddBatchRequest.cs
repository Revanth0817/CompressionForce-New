using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Services.DTOs.Requests
{
    public class AddBatchRequest
    {
        public string RecipeCode { get; set; }
        public string BatchCode { get; set; }
        public int BatchQty { get; set; }
        public int TabletQty { get; set; }
    }
}

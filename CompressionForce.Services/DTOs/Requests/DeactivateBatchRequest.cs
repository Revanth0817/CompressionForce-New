using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Services.DTOs.Requests
{
    public class DeactivateBatchRequest
    {
        public string BatchCode { get; set; } = default!;
    }
}



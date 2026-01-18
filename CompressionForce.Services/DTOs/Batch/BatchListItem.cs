using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Services.DTOs.Batch
{
    /// <summary>
    /// Lightweight DTO for batch dropdowns and selection lists
    /// </summary>
    public class BatchListItem
    {
        public string BatchCode { get; set; }
        public string BatchStatus { get; set; }
    }
}

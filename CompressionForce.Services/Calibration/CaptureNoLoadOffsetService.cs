using CompressionForce.Domain.Calibration;
using CompressionForce.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Services.Calibration
{
    public sealed class CaptureNoLoadOffsetService
    {
        private readonly IPlcSignalReader _reader;
        private readonly IPlcWriteConfirmService _confirm;

        public CaptureNoLoadOffsetService(
            IPlcSignalReader reader,
            IPlcWriteConfirmService confirm)
        {
            _reader = reader;
            _confirm = confirm;
        }

        public async Task CaptureAsync()
        {
            var force = _reader.ReadDecimal("LOADCELL_NO_LOAD_FORCE");
            var offset = new NoLoadOffset(force);

            await _confirm.WriteAndConfirmAsync(
                "LOADCELL_OFFSET",
                offset.Value
            );
        }
    }
}

using CompressionForce.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Domain.ValueObjects
{
    public sealed class PlcAddress
    {
        public AddressKind Kind { get; init; }
        public string Value { get; init; }
        public int? Length { get; init; }
    }
}

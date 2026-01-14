using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Domain.Entities
{
    /// <summary>
    /// Represents a single recipe parameter.
    /// Supports numeric, text, enum and multi-enum values.
    /// </summary>
    public class RecipeParameter
    {
        /// <summary>
        /// Logical name of the parameter (e.g. MaxTurretRpm, ToolType)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Parameter type: numeric | text | enum | multi-enum
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Value can be decimal, string, or string[]
        /// </summary>
        public object Value { get; set; }
    }
}

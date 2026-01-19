using CompressionForce.Domain.Entities;
using CompressionForce.Services.DTOs.Batch;
using CompressionForce.Web.Models.Batches;
using System.Reflection;
using System.Text.Json;
using CompressionForce.Services.DTOs.Requests;

namespace CompressionForce.Web.Mapping
{
    public static class BatchPageMapper
    {
        // --------------------------------------------------
        // Add Batch Modal
        // --------------------------------------------------
        public static AddBatchRequest ToDto(AddBatchVM vm)
        {
            if (vm == null) return null;

            return new AddBatchRequest
            {
                RecipeCode = vm.RecipeCode,
                BatchCode = vm.BatchCode,
                BatchQty = vm.BatchQty,
                TabletQty = vm.TabletQty
            };
        }
        // --------------------------------------------------
        // Edit Batch Modal
        // --------------------------------------------------
        public static EditBatchRequest ToDto(EditBatchVM vm)
        {
            if (vm == null) return null;

            return new EditBatchRequest
            {
                BatchCode = vm.BatchCode,
                BatchQty = vm.BatchQty
            };
        }

        // --------------------------------------------------
        // Deactivate Batch Modal
        // --------------------------------------------------
        public static DeactivateBatchRequest ToDto(DeactivateBatchVM vm)
        {
            if (vm == null) return null;

            return new DeactivateBatchRequest
            {
                BatchCode = vm.BatchCode
            };
        }

        // --------------------------------------------------
        // Batch Summary
        // --------------------------------------------------
        public static BatchSummaryVM ToSummaryVM(BatchDetails dto)
        {
            return new BatchSummaryVM
            {
                BatchCode = dto.BatchCode,
                BatchQty = dto.BatchQty,
                GoodQty = dto.GoodQty,
                RejectedQty = dto.RejectedQty,
                BatchStatus = dto.BatchStatus,
                TabletQty = dto.TabletQty
            };
        }

        // --------------------------------------------------
        // Batch Parameters mapping
        // --------------------------------------------------
        public static BatchRecipeParametersVM ToParametersVM(BatchDetails dto)
        {
            /*
            return new BatchRecipeParametersVM
            {
                ToolType = dto.ToolType,
                AwcArType = dto.AwcArType,
                RecipeType = dto.RecipeType,
                RecipeCode = dto.RecipeCode,

                ProductName = dto.ProductName,
                Shape = dto.Shape,
                Size = dto.Size,

                TableThickness = dto.TableThickness,
                TableHardness = dto.TableHardness,
                TableWeight = dto.TableWeight,
                MaxTurretRpm = dto.MaxTurretRpm,

                // S1
                ForceFeederRatioS1 = dto.ForceFeederRatioS1,
                FillDepthS1 = dto.FillDepthS1,
                MainPenetrationPositionS1 = dto.MainPenetrationPositionS1,
                MainThicknessPositionS1 = dto.MainThicknessPositionS1,
                PrePenetrationPositionS1 = dto.PrePenetrationPositionS1,
                PreThicknessPositionS1 = dto.PreThicknessPositionS1,
                SampleIntervalS1 = dto.SampleIntervalS1,
                SampleRevolutionQtyS1 = dto.SampleRevolutionQtyS1,
                MaxMainCompressionForceS1 = dto.MaxMainCompressionForceS1,
                MaxPreCompressionForceS1 = dto.MaxPreCompressionForceS1,
                MaxEjectionCompressionForceS1 = dto.MaxEjectionCompressionForceS1,

                // S2
                ForceFeederRatioS2 = dto.ForceFeederRatioS2,
                FillDepthS2 = dto.FillDepthS2,
                MainPenetrationPositionS2 = dto.MainPenetrationPositionS2,
                MainThicknessPositionS2 = dto.MainThicknessPositionS2,
                PrePenetrationPositionS2 = dto.PrePenetrationPositionS2,
                PreThicknessPositionS2 = dto.PreThicknessPositionS2,
                SampleIntervalS2 = dto.SampleIntervalS2,
                SampleRevolutionQtyS2 = dto.SampleRevolutionQtyS2,
                MaxMainCompressionForceS2 = dto.MaxMainCompressionForceS2,
                MaxPreCompressionForceS2 = dto.MaxPreCompressionForceS2,
                MaxEjectionCompressionForceS2 = dto.MaxEjectionCompressionForceS2,

                // AWC / AR
                RejectionLimitMaxS1 = dto.RejectionLimitMaxS1,
                AwcLimitMaxS1 = dto.AwcLimitMaxS1,
                AwcSetPointS1 = dto.AwcSetPointS1,
                AwcLimitMinS1 = dto.AwcLimitMinS1,
                RejectionLimitMinS1 = dto.RejectionLimitMinS1,

                RejectionLimitMaxS2 = dto.RejectionLimitMaxS2,
                AwcLimitMaxS2 = dto.AwcLimitMaxS2,
                AwcSetPointS2 = dto.AwcSetPointS2,
                AwcLimitMinS2 = dto.AwcLimitMinS2,
                RejectionLimitMinS2 = dto.RejectionLimitMinS2
            };
            */
            if (dto == null)
                return null;

            var vm = new BatchRecipeParametersVM
            {
                RecipeCode = dto.RecipeCode,
                ProductName = dto.RecipeName
            };

            if (string.IsNullOrWhiteSpace(dto.ParametersJson))
                return vm;

            // Deserialize JSON parameters
            var parameters = JsonSerializer.Deserialize<List<RecipeParameter>>(
                dto.ParametersJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (parameters == null || parameters.Count == 0)
                return vm;

            // Mapping dictionary for special cases
            var paramNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "AWC&AR", "AwcArType" },
        { "Recipe", "RecipeType" },
        { "TabletThickness", "TableThickness" },
        { "TabletHardness", "TableHardness" },
        { "TabletWeight", "TableWeight" },
        { "SampleRevolutionQuantityS1", "SampleRevolutionQtyS1" },
        { "MaxMainCompForceS1", "MaxMainCompressionForceS1" },
        { "MaxPreCompForceS1", "MaxPreCompressionForceS1" },
        { "MaxEjectionForceS1", "MaxEjectionCompressionForceS1" },
        { "SampleRevolutionQuantityS2", "SampleRevolutionQtyS2" },
        { "MaxMainCompForceS2", "MaxMainCompressionForceS2" },
        { "MaxPreCompForceS2", "MaxPreCompressionForceS2" },
        { "MaxEjectionForceS2", "MaxEjectionCompressionForceS2" },
    
        // Add more custom mappings here if needed
    };

            var vmType = typeof(BatchRecipeParametersVM);

            foreach (var param in parameters)
            {
                // Resolve mapped property name
                var propName = paramNameMap.ContainsKey(param.Name) ? paramNameMap[param.Name] : param.Name;
                var prop = vmType.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                // Debug log
                var actualType = param.Value?.GetType().Name ?? "null";
                Console.WriteLine($"[DEBUG] Parameter Name: {param.Name}, Type: {param.Type}, Value: {param.Value}, ActualValueType: {actualType}");

                if (prop == null || !prop.CanWrite)
                {
                    Console.WriteLine($"[BatchPageMapper] No matching VM property for parameter '{param.Name}'");
                    continue;
                }

                try
                {
                    var convertedValue = ConvertJsonValue(param.Value, prop.PropertyType);
                    prop.SetValue(vm, convertedValue);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BatchPageMapper] Failed to map parameter '{param.Name}': {ex.Message}");
                }
            }

            return vm;
        }

        // --------------------------------------------------
        // Recipe List
        // --------------------------------------------------
        public static RecipeVM ToVM(RecipeListItem dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            return new RecipeVM
            {
                RecipeCode = dto.RecipeCode,
                RecipeName = dto.RecipeName
            };
        }

        // --------------------------------------------------
        // Batch List
        // --------------------------------------------------
        public static BatchVM ToVM(BatchListItem dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            return new BatchVM
            {
                BatchCode = dto.BatchCode,
                BatchName = dto.BatchCode
            };
        }

        // --------------------------------------------------
        // Recipe Parameters (DTO → VM)
        // --------------------------------------------------
        public static BatchRecipeParametersVM ToVM(RecipeDetailsDto dto)
        {
            if (dto == null)
                return null;

            var vm = new BatchRecipeParametersVM
            {
                RecipeCode = dto.RecipeCode,
                ProductName = dto.RecipeName
            };

            if (string.IsNullOrWhiteSpace(dto.ParametersJson))
                return vm;

            // Deserialize JSON parameters
            var parameters = JsonSerializer.Deserialize<List<RecipeParameter>>(
                dto.ParametersJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (parameters == null || parameters.Count == 0)
                return vm;

            // Mapping dictionary for special cases
            var paramNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "AWC&AR", "AwcArType" },
        { "Recipe", "RecipeType" },
        { "TabletThickness", "TableThickness" },
        { "TabletHardness", "TableHardness" },
        { "TabletWeight", "TableWeight" },
        { "SampleRevolutionQuantityS1", "SampleRevolutionQtyS1" },
        { "MaxMainCompForceS1", "MaxMainCompressionForceS1" },
        { "MaxPreCompForceS1", "MaxPreCompressionForceS1" },
        { "MaxEjectionForceS1", "MaxEjectionCompressionForceS1" },
        { "SampleRevolutionQuantityS2", "SampleRevolutionQtyS2" },
        { "MaxMainCompForceS2", "MaxMainCompressionForceS2" },
        { "MaxPreCompForceS2", "MaxPreCompressionForceS2" },
        { "MaxEjectionForceS2", "MaxEjectionCompressionForceS2" },
    
        // Add more custom mappings here if needed
    };

            var vmType = typeof(BatchRecipeParametersVM);

            foreach (var param in parameters)
            {
                // Resolve mapped property name
                var propName = paramNameMap.ContainsKey(param.Name) ? paramNameMap[param.Name] : param.Name;
                var prop = vmType.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                // Debug log
                var actualType = param.Value?.GetType().Name ?? "null";
                Console.WriteLine($"[DEBUG] Parameter Name: {param.Name}, Type: {param.Type}, Value: {param.Value}, ActualValueType: {actualType}");

                if (prop == null || !prop.CanWrite)
                {
                    Console.WriteLine($"[BatchPageMapper] No matching VM property for parameter '{param.Name}'");
                    continue;
                }

                try
                {
                    var convertedValue = ConvertJsonValue(param.Value, prop.PropertyType);
                    prop.SetValue(vm, convertedValue);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BatchPageMapper] Failed to map parameter '{param.Name}': {ex.Message}");
                }
            }

            return vm;
        }

        // -----------------------------------------
        // Helper: Safe conversion of JSON value
        // -----------------------------------------
        private static object? ConvertJsonValue(object value, Type targetType)
        {
            if (value == null)
                return null;

            // -----------------------------
            // Handle JsonElement correctly
            // -----------------------------
            if (value is JsonElement json)
            {
                // 1️⃣ STRING JSON VALUES
                if (json.ValueKind == JsonValueKind.String)
                {
                    var str = json.GetString();

                    if (targetType == typeof(string))
                        return str;

                    if (targetType == typeof(decimal) && decimal.TryParse(str, out var dec))
                        return dec;

                    if (targetType == typeof(double) && double.TryParse(str, out var dbl))
                        return dbl;

                    if (targetType == typeof(int) && int.TryParse(str, out var i))
                        return i;

                    return str; // fallback
                }

                // 2️⃣ NUMBER JSON VALUES
                if (json.ValueKind == JsonValueKind.Number)
                {
                    if (targetType == typeof(decimal))
                        return json.GetDecimal();

                    if (targetType == typeof(double))
                        return json.GetDouble();

                    if (targetType == typeof(int))
                        return json.GetInt32();

                    if (targetType == typeof(long))
                        return json.GetInt64();
                }

                // 3️⃣ BOOLEAN JSON VALUES
                if (json.ValueKind == JsonValueKind.True || json.ValueKind == JsonValueKind.False)
                {
                    if (targetType == typeof(bool))
                        return json.GetBoolean();
                }
            }

            // -----------------------------
            // Handle non-JSON values
            // -----------------------------
            if (value is string s)
            {
                if (targetType == typeof(decimal) && decimal.TryParse(s, out var d))
                    return d;

                if (targetType == typeof(double) && double.TryParse(s, out var db))
                    return db;

                if (targetType == typeof(int) && int.TryParse(s, out var i))
                    return i;
            }

            return Convert.ChangeType(value, targetType);
        }

    }
}
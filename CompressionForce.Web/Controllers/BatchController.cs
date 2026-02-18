//using CompressionForce.Services.DTOs.Requests;
using CompressionForce.Domain.Exceptions;
using CompressionForce.Services.Interfaces;
using CompressionForce.Web.Mapping;
using CompressionForce.Web.Models.Batches;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Text.Encodings.Web;


namespace CompressionForce.Web.Controllers
{
    internal sealed class NullView : IView
    {
        public static readonly NullView Instance = new();
        public string Path => string.Empty;
        public Task RenderAsync(ViewContext context) => Task.CompletedTask;
    }
    public class BatchController : Controller
    {
        private readonly IBatchQueryService _batchqueryService;
        private readonly IBatchApplicationService _batchapplicationService;
        private readonly IViewComponentHelper _viewComponentHelper;

        public BatchController(
            IBatchQueryService queryService,
            IBatchApplicationService applicationService,
            IViewComponentHelper viewComponentHelper)
        {
            _batchqueryService = queryService;
            _batchapplicationService = applicationService;
            _viewComponentHelper = viewComponentHelper;
        }


        public async Task<IActionResult> Batch()
        {
            var vm = new BatchPageVM();

            // ---------------- Recipes ----------------
            var recipeDtos = await _batchqueryService.GetRecipesfromRecipe();

            if (recipeDtos == null || !recipeDtos.Any())
                return View(vm);

            vm.Recipes = recipeDtos
                .Select(BatchPageMapper.ToVM)
                .ToList();

            vm.SelectedRecipeCode = vm.Recipes.First().RecipeCode;

            // ---------------- Batches ----------------
            var batchDtos = await _batchqueryService.GetBatchesByRecipe(vm.SelectedRecipeCode);

            // CASE 1: No batches → recipe parameters only
            if (batchDtos == null || !batchDtos.Any())
            {
                var recipeDetails = await _batchqueryService
                    .GetRecipeDetailsAsync(vm.SelectedRecipeCode);

                vm.Parameters = BatchPageMapper.ToVM(recipeDetails);
                return View(vm);
            }

            // CASE 2: Batches exist
            vm.Batches = batchDtos
                .Select(BatchPageMapper.ToVM)
                .ToList();

            vm.SelectedBatchCode = vm.Batches.First().BatchCode;

            // ---------------- Batch Details ----------------
            var batchDetails = await _batchqueryService.GetBatchDetails(vm.SelectedBatchCode);
            if (batchDetails == null)
                return View(vm);

            vm.Summary = BatchPageMapper.ToSummaryVM(batchDetails);


            // PARAMETERS MUST COME FROM BATCH
            vm.Parameters = BatchPageMapper.ToParametersVM(batchDetails);
            return View(vm);
        }

        // --------------------------------------------------
        // AJAX endpoints
        // --------------------------------------------------


        [HttpGet]
        public async Task<IActionResult> GetActiveBatchesByRecipe(string recipeCode)
        {
            var batches = await _batchqueryService.GetActiveBatchesByRecipe(recipeCode);
            return Json(batches);
        }

        [HttpGet]
        public async Task<IActionResult> GetBatchesByRecipe(string recipeCode)
        {
            var batches = await _batchqueryService.GetBatchesByRecipe(recipeCode);
            return Json(batches);
        }

        [HttpGet]
        public async Task<IActionResult> GetBatchDetails(string batchCode)
        {
            var details = await _batchqueryService.GetBatchDetails(batchCode);
            return Json(details);
        }

        [HttpGet]
        public async Task<IActionResult> GetBatchSummaryAndParameters(string batchCode)
        {
            if (string.IsNullOrWhiteSpace(batchCode))
                return BadRequest();

            var batchDetails = await _batchqueryService.GetBatchDetails(batchCode);
            if (batchDetails == null)
                return NotFound();

            //var headerVm = BatchPageMapper.ToHeaderVM(batchDetails);
            var summaryVm = BatchPageMapper.ToSummaryVM(batchDetails);
            var parametersVm = BatchPageMapper.ToParametersVM(batchDetails);

            return Json(new
            {
                summaryHtml = await this.RenderViewComponentAsync(
                    "BatchSummary", summaryVm),

                parametersHtml = await this.RenderViewComponentAsync(
                    "RecipeParameters", parametersVm)
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetBatchHeader(string recipeCode)
        {
            var batchDtos = await _batchqueryService.GetBatchesByRecipe(recipeCode);

            var vm = new BatchHeaderVM
            {
                hasRecipes = !string.IsNullOrWhiteSpace(recipeCode),
                hasBatches = batchDtos != null && batchDtos.Any()
            };

            return ViewComponent("BatchHeader", vm);
        }


        //recipe-level parameters endpoint(for no-batch case)
        [HttpGet]
        public async Task<IActionResult> GetRecipeParameters(string recipeCode)
        {
            var recipe = await _batchqueryService.GetRecipeDetailsAsync(recipeCode);
            if (recipe == null)
                return NotFound();

            return Json(BatchPageMapper.ToVM(recipe));
        }


        [HttpGet]
        public async Task<IActionResult> GetRecipeParametersHtml(string recipeCode)
        {
            if (string.IsNullOrWhiteSpace(recipeCode))
                return BadRequest();

            var recipeDetails = await _batchqueryService
                .GetRecipeDetailsAsync(recipeCode);

            if (recipeDetails == null)
                return NotFound();

            var parametersVm = BatchPageMapper.ToVM(recipeDetails);

            return ViewComponent("RecipeParameters", parametersVm);
        }

        // Helper method to render a view component to string - used in AJAX endpoints
        private async Task<string> RenderViewComponentAsync(string componentName, object model)
        {
            using var writer = new StringWriter();

            var viewContext = new ViewContext(
                ControllerContext,
                NullView.Instance,
                ViewData,
                TempData,
                writer,
                new HtmlHelperOptions()
            );

            // IMPORTANT: contextualize the helper
            (_viewComponentHelper as IViewContextAware)
                ?.Contextualize(viewContext);

            var result = await _viewComponentHelper
                .InvokeAsync(componentName, model);

            result.WriteTo(writer, HtmlEncoder.Default);

            return writer.ToString();
        }

        // --------------------------------------------------
        // Commands
        // --------------------------------------------------

        [HttpPost]
        public async Task<IActionResult> AddBatch(AddBatchVM vm)
        {
            BatchPageMapper.ToDto(vm);
            try
            {
                await _batchapplicationService.AddBatchAsync(BatchPageMapper.ToDto(vm));
            }
            catch (DomainException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> EditBatch(EditBatchVM vm)
        {
            await _batchapplicationService.EditBatchAsync(BatchPageMapper.ToDto(vm));
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeactivateBatch(DeactivateBatchVM vm)
        {
            await _batchapplicationService.DeactivateBatchAsync(BatchPageMapper.ToDto(vm));
            return Ok();
        }
    }
}

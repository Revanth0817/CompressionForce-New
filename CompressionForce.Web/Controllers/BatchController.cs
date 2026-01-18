namespace CompressionForce.Web.Controllers
{
    using CompressionForce.Services.Interfaces;
    using CompressionForce.Web.Mapping;
    using CompressionForce.Web.Models.Batch;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    public class BatchController : Controller
    {
        private readonly IBatchQueryService _queryService;
        private readonly IBatchApplicationService _applicationService;

        private readonly IBatchService _batchService;
        /*
        public BatchController(IBatchService batchService)
        {
            _batchService = batchService;
        }*/
        public BatchController(
            IBatchQueryService queryService,
            IBatchApplicationService applicationService)
        {
            _queryService = queryService;
            _applicationService = applicationService;
        }

        public async Task<IActionResult> Index(string recipeCode)
        {
            var data = await _queryService.GetBatchPageDataAsync(recipeCode);
            var vm = BatchPageMapper.ToVM(data);
            return View(vm);
        }

        [HttpPost]
        public IActionResult AddBatch(AddBatchVM vm)
        {
            _batchService.AddBatch(
                vm.RecipeCode,
                vm.BatchCode,
                vm.BatchQty,
                vm.TabletQty);

            return Ok();
        }

        [HttpPost]
        public IActionResult EditBatch(EditBatchVM vm)
        {
            _batchService.UpdateBatchQty(vm.BatchCode, vm.BatchQty);
            return Ok();
        }

        [HttpPost]
        public IActionResult DeactivateBatch(DeactivateBatchVM vm)
        {
            _batchService.DeactivateBatch(vm.BatchCode);
            return Ok();
        }
    }
}

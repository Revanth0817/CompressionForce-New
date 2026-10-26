using CompressionForce.Web.Authorization;

using CompressionForce.Data;
using CompressionForce.Domain.Entities;
using CompressionForce.Domain.Exceptions;
using CompressionForce.Domain.Validation;
using CompressionForce.Services.DTOs;
using CompressionForce.Services.Lookups;
using CompressionForce.Services.Recipes;
using CompressionForce.Services.Validation;
using CompressionForce.Web.Models.Recipes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CompressionForce.Web.Controllers
{
    [RequirePrivilege("Recipe")]
    public class RecipeController : Controller
    {
        private readonly IRecipeService _recipeService;
        private readonly ILookupService _lookupService;
        private readonly IRecipeValidationConfigProvider _cfgProvider;
        private readonly ILogger<RecipeController> _logger;
        private readonly ApplicationDbContext _db;

        public RecipeController(
            IRecipeService recipeService,
            ILookupService lookupService,
            IRecipeValidationConfigProvider cfgProvider,
            ILogger<RecipeController> logger,
            ApplicationDbContext db)
        {
            _db = db;
            _recipeService = recipeService;
            _lookupService = lookupService;
            _cfgProvider = cfgProvider;
            _logger = logger;
        }

        // Authentication is session based (see AccountController), not Identity based
        private string CurrentUser => HttpContext.Session.GetString("UserName") ?? "system";

        private async Task LoadLookupsAsync(AddEditRecipeVm vm)
        {
            vm.ToolTypes = (await _lookupService.GetCodesAsync("ToolType")).ToList();
            vm.Treatments = (await _lookupService.GetCodesAsync("Treatment")).ToList();
            vm.AWC_ARTypes = (await _lookupService.GetCodesAsync("AWC&AR")).ToList();
            vm.ForceFeederRatioS1Types = (await _lookupService.GetCodesAsync("ForceFeederRatioS1")).ToList();
            vm.ForceFeederRatioS2Types = (await _lookupService.GetCodesAsync("ForceFeederRatioS2")).ToList();
            vm.RecipeTypes = (await _lookupService.GetCodesAsync("Recipe")).ToList();
        }

        // 1. List codes + first recipe + lookups
        [HttpGet]
        public async Task<IActionResult> RecipeParameter()
        {
            var codes = (await _recipeService.GetRecipeCodesAsync()).ToList();
            var selected = codes.FirstOrDefault();

            Recipe? recipe = null;
            if (!string.IsNullOrWhiteSpace(selected))
                recipe = await _recipeService.GetByCodeAsync(selected);

            var toolTypes = (await _lookupService.GetCodesAsync("ToolType")).ToList();
            var treatments = (await _lookupService.GetCodesAsync("Treatment")).ToList(); // optional multi-enum demo
            var awc_arTypes = (await _lookupService.GetCodesAsync("AWC&AR")).ToList();
            var forceFeederRatioS1Types = (await _lookupService.GetCodesAsync("ForceFeederRatioS1")).ToList();
            var forceFeederRatioS2Types = (await _lookupService.GetCodesAsync("ForceFeederRatioS2")).ToList();

            var vm = new RecipeParameterPageVm
            {
                RecipeCodes = codes,
                SelectedCode = selected,
                Recipe = recipe != null ? RecipeViewModelsMapping.ToDto(recipe) : null,
                ToolTypes = toolTypes,
                Treatments = treatments,
                AWC_ARTypes = awc_arTypes,
                ForceFeederRatioS1Types = forceFeederRatioS1Types,
                ForceFeederRatioS2Types = forceFeederRatioS2Types
            };

            return View(vm);

        }

        // Records tab: who added / changed / removed a recipe and when (newest first, capped)
        [HttpGet]
        public async Task<IActionResult> History(string? code)
        {
            var query = _db.RecipeHistories.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(code))
                query = query.Where(h => h.RecipeCode == code);

            var rows = await query
                .OrderByDescending(h => h.ChangedAt)
                .Take(200)
                .Select(h => new { h.RecipeCode, h.Action, h.ChangedBy, h.ChangedAt })
                .ToListAsync();

            return Json(rows.Select(h => new
            {
                h.RecipeCode,
                h.Action,
                h.ChangedBy,
                changedAt = h.ChangedAt.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss")
            }));
        }

        // 2. Return recipe content (partial)
        [HttpGet]
        [RequirePrivilege("Recipe", "Batch")]
        public async Task<IActionResult> GetTheRecipe(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest("Recipe code is required.");

            var recipe = await _recipeService.GetByCodeAsync(code);
            if (recipe == null)
                return NotFound($"Recipe code '{code}' not found.");

            var dto = RecipeViewModelsMapping.ToDto(recipe);
            return PartialView("_RecipeParametersIndex", dto);
        }


        // 3. Check if code exists (Add modal)
        [HttpGet]
        public async Task<IActionResult> CheckCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Json(new { exists = false, message = "Recipe code is required." });

            var exists = await _recipeService.ExistsByCodeAsync(code);
            return Json(new { exists, message = exists ? "Recipe code already exists." : "Recipe code is available." });
        }

        // ADD
        [HttpGet]
        public async Task<IActionResult> AddRecipe(string code)
        {
            if (string.IsNullOrWhiteSpace(WebUtility.UrlDecode(code)))
            {
                return RedirectToAction(nameof(RecipeParameter));
            }
            if (await _recipeService.ExistsByCodeAsync(WebUtility.UrlDecode(code)))
            {
                TempData["Error"] = $"Recipe code '{WebUtility.UrlDecode(code)}' already exists.";
                return RedirectToAction(nameof(RecipeParameter));
            }

            var toolTypes = (await _lookupService.GetCodesAsync("ToolType")).ToList();
            var treatments = (await _lookupService.GetCodesAsync("Treatment")).ToList(); // optional multi-enum demo
            var awc_arTypes = (await _lookupService.GetCodesAsync("AWC&AR")).ToList();
            var forceFeederRatioS1Types = (await _lookupService.GetCodesAsync("ForceFeederRatioS1")).ToList();
            var forceFeederRatioS2Types = (await _lookupService.GetCodesAsync("ForceFeederRatioS2")).ToList();
            var recipeTypes = (await _lookupService.GetCodesAsync("Recipe")).ToList();

            // Pull rules from config
            var rules = _cfgProvider.Get().Parameters;

            // Build parameters from rules so types match config
            var parameters = new List<RecipeParameter>();
            foreach (var rule in rules)
            {
                switch (rule.Type.ToLowerInvariant())
                {
                    case "enum":
                        // default to first lookup code if required; else empty
                        //var first = rule.LookupCategory == "ToolType" ? toolTypes.FirstOrDefault() : "";
                        parameters.Add(new RecipeParameter
                        {
                            Name = rule.Name,
                            Type = "enum",
                            //Value = first ?? ""
                            Value = ""
                        });
                        break;
                    case "multi-enum":
                        parameters.Add(new RecipeParameter
                        {
                            Name = rule.Name,
                            Type = "multi-enum",
                            Value = Array.Empty<string>()
                        });
                        break;

                    case "numeric":
                        // choose a sensible default inside [min,max] or 0/1
                        var defaultNum = rule.Min ?? 1m;
                        parameters.Add(new RecipeParameter
                        {
                            Name = rule.Name,
                            Type = "numeric",
                            Value = defaultNum
                        });
                        break;

                    default: // text
                        parameters.Add(new RecipeParameter
                        {
                            Name = rule.Name,
                            Type = "text",
                            Value = rule.Name
                        });
                        break;
                }
            }

            var vm = new AddEditRecipeVm
            {
                Code = code,
                ToolTypes = toolTypes,
                Treatments = treatments,
                AWC_ARTypes = awc_arTypes,
                ForceFeederRatioS1Types = forceFeederRatioS1Types,
                ForceFeederRatioS2Types = forceFeederRatioS2Types,
                RecipeTypes = recipeTypes,
                Parameters = parameters
            };
            ViewBag.ValidationRules = rules;

            return View(vm);
        }


        // 4 + 5 + 6. Add (client + server validation)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRecipe(AddEditRecipeVm vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadLookupsAsync(vm);
                ViewBag.ValidationRules = _cfgProvider.Get().Parameters;
                return View(vm);
            }

            if (await _recipeService.ExistsByCodeAsync(vm.Code))
                ModelState.AddModelError(nameof(vm.Code), $"Recipe code '{vm.Code}' already exists.");

            if (await _recipeService.ExistsByNameAsync(vm.Name))
                ModelState.AddModelError(nameof(vm.Name), $"Recipe name '{vm.Name}' already exists.");

            if (!ModelState.IsValid)
            {
                await LoadLookupsAsync(vm);
                ViewBag.ValidationRules = _cfgProvider.Get().Parameters;
                return View(vm);
            }

            try
            {
                var domain = RecipeViewModelsMapping.ToDomain(vm);
                await _recipeService.AddAsync(domain, user: CurrentUser);
                TempData["Success"] = "Recipe added successfully.";
                return RedirectToAction(nameof(RecipeParameter));
            }
            catch (DomainException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            await LoadLookupsAsync(vm);
            ViewBag.ValidationRules = _cfgProvider.Get().Parameters;
            return View(vm);
        }

        // EDIT
        [HttpGet]
        public async Task<IActionResult> EditRecipe(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return RedirectToAction(nameof(RecipeParameter));

            var recipe = await _recipeService.GetByCodeAsync(code);
            if (recipe == null)
            {
                TempData["Error"] = $"Recipe code '{code}' not found.";
                return RedirectToAction(nameof(RecipeParameter));
            }

            var toolTypes = (await _lookupService.GetCodesAsync("ToolType")).ToList();
            var treatments = (await _lookupService.GetCodesAsync("Treatment")).ToList();
            var awc_arTypes = (await _lookupService.GetCodesAsync("AWC&AR")).ToList();
            var forceFeederRatioS1Types = (await _lookupService.GetCodesAsync("ForceFeederRatioS1")).ToList();
            var forceFeederRatioS2Types = (await _lookupService.GetCodesAsync("ForceFeederRatioS2")).ToList();
            var recipeTypes = (await _lookupService.GetCodesAsync("Recipe")).ToList();

            var vm = RecipeViewModelsMapping.ToAddEditVm(recipe, toolTypes, treatments, awc_arTypes, forceFeederRatioS1Types, forceFeederRatioS2Types, recipeTypes);
            ViewBag.ValidationRules = _cfgProvider.Get().Parameters;
            return View(vm);
        }

        // 7 + 8 + 9. Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRecipe(AddEditRecipeVm vm)
        {

            if (!ModelState.IsValid)
            {
                await LoadLookupsAsync(vm);
                ViewBag.ValidationRules = _cfgProvider.Get().Parameters;
                return View(vm);
            }

            var current = await _recipeService.GetByCodeAsync(vm.Code);
            if (current == null)
                ModelState.AddModelError(nameof(vm.Code), $"Recipe code '{vm.Code}' does not exist.");

            if (await _recipeService.ExistsByNameAsync(vm.Name))
            {
                if (!string.Equals(current?.Name, vm.Name, StringComparison.OrdinalIgnoreCase))
                    ModelState.AddModelError(nameof(vm.Name), $"Recipe name '{vm.Name}' already exists.");
            }

            if (!ModelState.IsValid)
            {
                await LoadLookupsAsync(vm);
                ViewBag.ValidationRules = _cfgProvider.Get().Parameters;
                return View(vm);
            }

            try
            {
                var domain = RecipeViewModelsMapping.ToDomain(vm);
                await _recipeService.UpdateAsync(domain, user: CurrentUser);
                TempData["Success"] = "Recipe updated successfully.";
                return RedirectToAction(nameof(RecipeParameter));
            }
            catch (DomainException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            await LoadLookupsAsync(vm);
            ViewBag.ValidationRules = _cfgProvider.Get().Parameters;
            return View(vm);
        }


        // DELETE
        [HttpGet]
        public async Task<IActionResult> RemoveRecipe(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return RedirectToAction(nameof(RecipeParameter));

            var recipe = await _recipeService.GetByCodeAsync(code);
            if (recipe == null)
            {
                TempData["Error"] = $"Recipe code '{code}' not found.";
                return RedirectToAction(nameof(RecipeParameter));
            }

            var vm = RecipeViewModelsMapping.ToDeleteVm(recipe);
            return View(vm);
        }

        // 10 + 11. DeleteConfirmed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string code, string name)
        {
            if (!await _recipeService.ExistsByNameAsync(name))
            {
                TempData["Error"] = $"Recipe name '{name}' does not exist.";
                return RedirectToAction(nameof(RecipeParameter));
            }

            try
            {
                await _recipeService.DeleteAsync(code, user: CurrentUser);
                TempData["Success"] = "Recipe deleted successfully.";
            }
            catch (DomainException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(RecipeParameter));
        }

        // 12 + 13. Print
        [HttpGet]
        public async Task<IActionResult> Print(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return RedirectToAction(nameof(RecipeParameter));

            var recipe = await _recipeService.GetByCodeAsync(code);
            if (recipe == null)
            {
                TempData["Error"] = $"Recipe code '{code}' not found.";
                return RedirectToAction(nameof(RecipeParameter));
            }

            var existsByName = await _recipeService.ExistsByNameAsync(recipe.Name);
            if (!existsByName)
            {
                TempData["Error"] = $"Recipe name '{recipe.Name}' does not exist.";
                return RedirectToAction(nameof(RecipeParameter));
            }

            var dto = RecipeViewModelsMapping.ToDto(recipe);
            return View("Print", dto);
        }

    }
}

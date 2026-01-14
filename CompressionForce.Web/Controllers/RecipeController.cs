
using CompressionForce.Domain.Entities;
using CompressionForce.Domain.Exceptions;
using CompressionForce.Domain.Validation;
using CompressionForce.Services.DTOs;
using CompressionForce.Services.Lookups;
using CompressionForce.Services.Recipes;
using CompressionForce.Services.Validation;
using CompressionForce.Web.Models.Recipes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Web.Controllers
{
    public class RecipeController : Controller
    {
        private readonly IRecipeService _recipeService;
        private readonly ILookupService _lookupService;
        private readonly IRecipeValidationConfigProvider _cfgProvider;
        private readonly ILogger<RecipeController> _logger;

        public RecipeController(
            IRecipeService recipeService,
            ILookupService lookupService,
            IRecipeValidationConfigProvider cfgProvider,
            ILogger<RecipeController> logger)
        {
            _recipeService = recipeService;
            _lookupService = lookupService;
            _cfgProvider = cfgProvider;
            _logger = logger;
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
                ForceFeederRatioS2Types = forceFeederRatioS1Types
            };

            return View(vm);

        }

        // 2. Return recipe content (partial)
        [HttpGet]
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


        // 2. Return recipe content (partial)
        [HttpGet]
        public async Task<IActionResult> GetRecipe(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest("Recipe code is required.");

            var recipe = await _recipeService.GetByCodeAsync(code);
            if (recipe == null)
                return NotFound($"Recipe code '{code}' not found.");

            var dto = RecipeViewModelsMapping.ToDto(recipe);
            return PartialView("_RecipeParametersReadOnly", dto);
        }


        // 1. List codes + first recipe + lookups
        [HttpGet]
        public async Task<IActionResult> Index()
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
                ForceFeederRatioS2Types = forceFeederRatioS1Types
            };

            return View(vm);
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
        public async Task<IActionResult> Add(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return RedirectToAction(nameof(Index));

            if (await _recipeService.ExistsByCodeAsync(code))
            {
                TempData["Error"] = $"Recipe code '{code}' already exists.";
                return RedirectToAction(nameof(Index));
            }

            var toolTypes = (await _lookupService.GetCodesAsync("ToolType")).ToList();
            var treatments = (await _lookupService.GetCodesAsync("Treatment")).ToList(); // optional multi-enum demo
            var awc_arTypes = (await _lookupService.GetCodesAsync("AWC&AR")).ToList();
            var forceFeederRatioS1Types = (await _lookupService.GetCodesAsync("ForceFeederRatioS1")).ToList();
            var forceFeederRatioS2Types = (await _lookupService.GetCodesAsync("ForceFeederRatioS2")).ToList();
            var recipeTypes = (await _lookupService.GetCodesAsync("Recipe")).ToList();

            // Pull rules from config
            var rules = _cfgProvider.Get().Parameters;
            ViewBag.ValidationRules = rules;

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
                Name = code + "Name",
                ToolTypes = toolTypes,
                Treatments = treatments,
                AWC_ARTypes = awc_arTypes,
                ForceFeederRatioS1Types = forceFeederRatioS1Types,
                ForceFeederRatioS2Types = forceFeederRatioS1Types,
                RecipeTypes = recipeTypes,
                Parameters = parameters
            };

            return View(vm);
        }


        // 4 + 5 + 6. Add (client + server validation)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddEditRecipeVm vm)
        {
            //vm.Parameters = NormalizeParametersFromRequest(Request.Form, vm.Parameters);

            if (!ModelState.IsValid)
            {
                vm.ToolTypes = (await _lookupService.GetCodesAsync("ToolType")).ToList();
                vm.Treatments = (await _lookupService.GetCodesAsync("Treatment")).ToList();
                ViewBag.ValidationRules = _cfgProvider.Get().Parameters;
                return View(vm);
            }

            if (await _recipeService.ExistsByCodeAsync(vm.Code))
                ModelState.AddModelError(nameof(vm.Code), $"Recipe code '{vm.Code}' already exists.");

            if (await _recipeService.ExistsByNameAsync(vm.Name))
                ModelState.AddModelError(nameof(vm.Name), $"Recipe name '{vm.Name}' already exists.");

            if (!ModelState.IsValid)
            {
                vm.ToolTypes = (await _lookupService.GetCodesAsync("ToolType")).ToList();
                vm.Treatments = (await _lookupService.GetCodesAsync("Treatment")).ToList();
                ViewBag.ValidationRules = _cfgProvider.Get().Parameters;
                return View(vm);
            }

            try
            {
                var domain = RecipeViewModelsMapping.ToDomain(vm);
                await _recipeService.AddAsync(domain, user: User?.Identity?.Name ?? "system");
                TempData["Success"] = "Recipe added successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DomainException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            vm.ToolTypes = (await _lookupService.GetCodesAsync("ToolType")).ToList();
            vm.Treatments = (await _lookupService.GetCodesAsync("Treatment")).ToList();
            ViewBag.ValidationRules = _cfgProvider.Get().Parameters;
            return View(vm);
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
            Console.WriteLine(rules.Count);
            Console.WriteLine("----------------------------------rules.Count in AddRecipe Get----------------------------------");

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

            Console.WriteLine(parameters.Count);
            Console.WriteLine("----------------------------------parameters.Count in AddRecipe Get----------------------------------");
            var vm = new AddEditRecipeVm
            {
                Code = code,
                ToolTypes = toolTypes,
                Treatments = treatments,
                AWC_ARTypes = awc_arTypes,
                ForceFeederRatioS1Types = forceFeederRatioS1Types,
                ForceFeederRatioS2Types = forceFeederRatioS1Types,
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
            Console.WriteLine("----------------------------------Hey Im in AddRecipe post----------------------------------");
            //vm.Parameters = NormalizeParametersFromRequest(Request.Form, vm.Parameters);
            Console.WriteLine(vm.Code);
            Console.WriteLine("----------------------------------vm.code----------------------------------");
            Console.WriteLine(vm.Parameters.Count);
            Console.WriteLine("----------------------------------Parameters.Count in AddRecipe Post----------------------------------");
            foreach (var item in vm.Parameters)
                {
                    Console.WriteLine(item.Name + ": "+item.Value);
                }
            Console.WriteLine("----------------------------------Parameter.Values----------------------------------");
            if (!ModelState.IsValid)
            {
            Console.WriteLine("----------------------------------Printing Modelstate Errors----------------------------------");

                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }
                vm.ToolTypes = (await _lookupService.GetCodesAsync("ToolType")).ToList();
                vm.Treatments = (await _lookupService.GetCodesAsync("Treatment")).ToList();
                vm.AWC_ARTypes = (await _lookupService.GetCodesAsync("AWC&AR")).ToList();
                vm.ForceFeederRatioS1Types = (await _lookupService.GetCodesAsync("ForceFeederRatioS1")).ToList();
                vm.ForceFeederRatioS2Types = (await _lookupService.GetCodesAsync("ForceFeederRatioS2")).ToList();
                vm.RecipeTypes = (await _lookupService.GetCodesAsync("Recipe")).ToList();
                ViewBag.ValidationRules = _cfgProvider.Get().Parameters;
                return View(vm);
            }

            if (await _recipeService.ExistsByCodeAsync(vm.Code))
                ModelState.AddModelError(nameof(vm.Code), $"Recipe code '{vm.Code}' already exists.");

            if (await _recipeService.ExistsByNameAsync(vm.Name))
                ModelState.AddModelError(nameof(vm.Name), $"Recipe name '{vm.Name}' already exists.");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("----------------------------------Printing Modelstate Errors 2ndTime----------------------------------");

                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }
                vm.ToolTypes = (await _lookupService.GetCodesAsync("ToolType")).ToList();
                vm.Treatments = (await _lookupService.GetCodesAsync("Treatment")).ToList();
                vm.AWC_ARTypes = (await _lookupService.GetCodesAsync("AWC&AR")).ToList();
                vm.ForceFeederRatioS1Types = (await _lookupService.GetCodesAsync("ForceFeederRatioS1")).ToList();
                vm.ForceFeederRatioS2Types = (await _lookupService.GetCodesAsync("ForceFeederRatioS2")).ToList();
                vm.RecipeTypes = (await _lookupService.GetCodesAsync("Recipe")).ToList();
                ViewBag.ValidationRules = _cfgProvider.Get().Parameters;
                return View(vm);
            }

            try
            {
                Console.WriteLine("----------------------------------Im trying to AddRecipe----------------------------------");
                var domain = RecipeViewModelsMapping.ToDomain(vm);
                await _recipeService.AddAsync(domain, user: User?.Identity?.Name ?? "system");
                TempData["Success"] = "Recipe added successfully.";
                return RedirectToAction(nameof(RecipeParameter));
            }
            catch (DomainException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            vm.ToolTypes = (await _lookupService.GetCodesAsync("ToolType")).ToList();
            vm.Treatments = (await _lookupService.GetCodesAsync("Treatment")).ToList();
            vm.AWC_ARTypes = (await _lookupService.GetCodesAsync("AWC&AR")).ToList();
            vm.ForceFeederRatioS1Types = (await _lookupService.GetCodesAsync("ForceFeederRatioS1")).ToList();
            vm.ForceFeederRatioS2Types = (await _lookupService.GetCodesAsync("ForceFeederRatioS2")).ToList();
            vm.RecipeTypes = (await _lookupService.GetCodesAsync("Recipe")).ToList();
            ViewBag.ValidationRules = _cfgProvider.Get().Parameters;
            return View(vm);
        }

        // EDIT
        [HttpGet]
        public async Task<IActionResult> Edit(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return RedirectToAction(nameof(Index));

            var recipe = await _recipeService.GetByCodeAsync(code);
            if (recipe == null)
            {
                TempData["Error"] = $"Recipe code '{code}' not found.";
                return RedirectToAction(nameof(Index));
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
        public async Task<IActionResult> Edit(AddEditRecipeVm vm)
        {
            //vm.Parameters = NormalizeParametersFromRequest(Request.Form, vm.Parameters);

            if (!ModelState.IsValid)
            {
                vm.ToolTypes = (await _lookupService.GetCodesAsync("ToolType")).ToList();
                vm.Treatments = (await _lookupService.GetCodesAsync("Treatment")).ToList();
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
                vm.ToolTypes = (await _lookupService.GetCodesAsync("ToolType")).ToList();
                vm.Treatments = (await _lookupService.GetCodesAsync("Treatment")).ToList();
                ViewBag.ValidationRules = _cfgProvider.Get().Parameters;
                return View(vm);
            }

            try
            {
                var domain = RecipeViewModelsMapping.ToDomain(vm);
                await _recipeService.UpdateAsync(domain, user: User?.Identity?.Name ?? "system");
                TempData["Success"] = "Recipe updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DomainException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            vm.ToolTypes = (await _lookupService.GetCodesAsync("ToolType")).ToList();
            vm.Treatments = (await _lookupService.GetCodesAsync("Treatment")).ToList();
            ViewBag.ValidationRules = _cfgProvider.Get().Parameters;
            return View(vm);
        }

        // EDIT
        [HttpGet]
        public async Task<IActionResult> EditRecipe(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return RedirectToAction(nameof(Index));

            var recipe = await _recipeService.GetByCodeAsync(code);
            if (recipe == null)
            {
                TempData["Error"] = $"Recipe code '{code}' not found.";
                return RedirectToAction(nameof(Index));
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
            //vm.Parameters = NormalizeParametersFromRequest(Request.Form, vm.Parameters);

            if (!ModelState.IsValid)
            {
                vm.ToolTypes = (await _lookupService.GetCodesAsync("ToolType")).ToList();
                vm.Treatments = (await _lookupService.GetCodesAsync("Treatment")).ToList();
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
                vm.ToolTypes = (await _lookupService.GetCodesAsync("ToolType")).ToList();
                vm.Treatments = (await _lookupService.GetCodesAsync("Treatment")).ToList();
                ViewBag.ValidationRules = _cfgProvider.Get().Parameters;
                return View(vm);
            }

            try
            {
                var domain = RecipeViewModelsMapping.ToDomain(vm);
                await _recipeService.UpdateAsync(domain, user: User?.Identity?.Name ?? "system");
                TempData["Success"] = "Recipe updated successfully.";
                return RedirectToAction(nameof(RecipeParameter));
            }
            catch (DomainException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            vm.ToolTypes = (await _lookupService.GetCodesAsync("ToolType")).ToList();
            vm.Treatments = (await _lookupService.GetCodesAsync("Treatment")).ToList();
            ViewBag.ValidationRules = _cfgProvider.Get().Parameters;
            return View(vm);
        }


        // DELETE
        [HttpGet]
        public async Task<IActionResult> Delete(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return RedirectToAction(nameof(Index));

            var recipe = await _recipeService.GetByCodeAsync(code);
            if (recipe == null)
            {
                TempData["Error"] = $"Recipe code '{code}' not found.";
                return RedirectToAction(nameof(Index));
            }

            var vm = RecipeViewModelsMapping.ToDeleteVm(recipe);
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
        public async Task<IActionResult> DeleteIsConfirmed(string code, string name)
        {
            if (!await _recipeService.ExistsByNameAsync(name))
            {
                TempData["Error"] = $"Recipe name '{name}' does not exist.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _recipeService.DeleteAsync(code, user: User?.Identity?.Name ?? "system");
                TempData["Success"] = "Recipe deleted successfully.";
            }
            catch (DomainException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
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
                await _recipeService.DeleteAsync(code, user: User?.Identity?.Name ?? "system");
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

        /// <summary>
        /// Converts dynamic Parameter.Value to the expected type based on Parameter.Type.
        /// - numeric -> decimal
        /// - enum    -> string
        /// - multi-enum -> string[]
        /// - text    -> string
        /// </summary>
        private static List<RecipeParameter> NormalizeParametersFromRequest(
            Microsoft.AspNetCore.Http.IFormCollection form,
            List<RecipeParameter> parameters)
        {
            var normalized = new List<RecipeParameter>();

            for (var i = 0; i < parameters.Count; i++)
            {
                var name = parameters[i].Name;
                var type = parameters[i].Type?.ToLowerInvariant() ?? "text";

                if (type == "multi-enum")
                {
                    var key = $"Parameters[{i}].Value";
                    var values = form[key];
                    normalized.Add(new RecipeParameter
                    {
                        Name = name,
                        Type = "multi-enum",
                        Value = values.ToArray()
                    });
                    continue;
                }

                var singleKey = $"Parameters[{i}].Value";
                var raw = form[singleKey].FirstOrDefault() ?? parameters[i].Value?.ToString() ?? string.Empty;

                if (type == "numeric")
                {
                    if (decimal.TryParse(raw, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out var d))
                    {
                        normalized.Add(new RecipeParameter { Name = name, Type = "numeric", Value = d });
                    }
                    else
                    {
                        normalized.Add(new RecipeParameter { Name = name, Type = "numeric", Value = raw });
                    }
                }
                else if (type == "enum")
                {
                    normalized.Add(new RecipeParameter { Name = name, Type = "enum", Value = raw });
                }
                else
                {
                    normalized.Add(new RecipeParameter { Name = name, Type = "text", Value = raw });
                }
            }

            return normalized;
        }
    }
}

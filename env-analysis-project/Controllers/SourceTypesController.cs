using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using env_analysis_project.Data;
using env_analysis_project.Models;
using env_analysis_project.Validators;
using env_analysis_project.Services;

namespace env_analysis_project.Controllers
{
    public class SourceTypesController : Controller
    {
        private readonly env_analysis_projectContext _context;
        private readonly IUserActivityLogger _activityLogger;

        public SourceTypesController(env_analysis_projectContext context, IUserActivityLogger activityLogger)
        {
            _context = context;
            _activityLogger = activityLogger;
        }

        // ========== Views ==========
        public async Task<IActionResult> Index()
        {
            var sourceTypes = await ActiveSourceTypes()
                .Select(st => new SourceTypeDto
                {
                    SourceTypeID = st.SourceTypeID,
                    SourceTypeName = st.SourceTypeName,
                    Description = st.Description,
                    IsActive = st.IsActive,
                    CreatedAt = st.CreatedAt,
                    UpdatedAt = st.UpdatedAt,
                    EmissionSourceCount = _context.EmissionSource.Count(es => es.SourceTypeID == st.SourceTypeID && !es.IsDeleted)
                })
                .ToListAsync();

            ViewBag.SourceTypes = sourceTypes;
            return View(sourceTypes);
        }

        public IActionResult Create() => View();

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sourceType = await ActiveSourceTypes().FirstOrDefaultAsync(m => m.SourceTypeID == id);
            if (sourceType == null)
            {
                return NotFound();
            }

            return View(sourceType);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var sourceType = await ActiveSourceTypes().FirstOrDefaultAsync(m => m.SourceTypeID == id);
            if (sourceType == null) return NotFound();

            return View(sourceType);
        }

        // ========== JSON APIs ==========
        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            var sourceTypes = await ActiveSourceTypes()
                .Select(st => new SourceTypeDto
                {
                    SourceTypeID = st.SourceTypeID,
                    SourceTypeName = st.SourceTypeName,
                    Description = st.Description,
                    IsActive = st.IsActive,
                    CreatedAt = st.CreatedAt,
                    UpdatedAt = st.UpdatedAt,
                    EmissionSourceCount = _context.EmissionSource.Count(es => es.SourceTypeID == st.SourceTypeID && !es.IsDeleted)
                })
                .ToListAsync();

            return Ok(ApiResponse.Success(sourceTypes));
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var dto = await ActiveSourceTypes()
                .Where(s => s.SourceTypeID == id)
                .Select(s => new SourceTypeDto
                {
                    SourceTypeID = s.SourceTypeID,
                    SourceTypeName = s.SourceTypeName,
                    Description = s.Description,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    EmissionSourceCount = _context.EmissionSource.Count(es => es.SourceTypeID == s.SourceTypeID && !es.IsDeleted)
                })
                .FirstOrDefaultAsync();

            if (dto == null)
            {
                return NotFound(ApiResponse.Fail<SourceTypeDto>("Source type not found."));
            }

            return Ok(ApiResponse.Success(dto));
        }

        // ========== Mutations ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SourceType sourceType)
        {
            var validationErrors = SourceTypeValidator.Validate(sourceType).ToList();
            if (!ModelState.IsValid)
            {
                validationErrors.AddRange(GetModelErrors());
            }

            if (validationErrors.Count > 0)
            {
                if (IsAjaxRequest())
                {
                    return BadRequest(ApiResponse.Fail<SourceTypeDto>("Validation failed.", validationErrors));
                }

                foreach (var error in validationErrors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }

                return View(sourceType);
            }

            sourceType.CreatedAt = DateTime.Now;
            sourceType.UpdatedAt = DateTime.Now;

            _context.SourceType.Add(sourceType);
            await _context.SaveChangesAsync();
            await LogAsync("SourceType.Create", sourceType.SourceTypeID.ToString(), $"Created source type {sourceType.SourceTypeName}");

            if (IsAjaxRequest())
            {
                return Ok(ApiResponse.Success(ToDto(sourceType), "Source type created successfully."));
            }

            TempData["Success"] = "Source type created successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SourceTypeID,SourceTypeName,Description,IsActive,CreatedAt,UpdatedAt")] SourceType model)
        {
            if (id != model.SourceTypeID)
            {
                return BadRequest(ApiResponse.Fail<SourceTypeDto>("Invalid source type identifier."));
            }

            var validationErrors = SourceTypeValidator.Validate(model).ToList();
            if (!ModelState.IsValid)
            {
                validationErrors.AddRange(GetModelErrors());
            }

            if (validationErrors.Count > 0)
            {
                return BadRequest(ApiResponse.Fail<SourceTypeDto>("Validation failed.", validationErrors));
            }

            var existing = await _context.SourceType.FindAsync(id);
            if (existing == null || existing.IsDeleted)
            {
                return NotFound(ApiResponse.Fail<SourceTypeDto>("Source type not found."));
            }

            existing.SourceTypeName = model.SourceTypeName?.Trim();
            existing.Description = model.Description?.Trim();
            existing.IsActive = model.IsActive;
            existing.UpdatedAt = DateTime.Now;

            _context.Update(existing);
            await _context.SaveChangesAsync();
            await LogAsync("SourceType.Update", existing.SourceTypeID.ToString(), $"Updated source type {existing.SourceTypeName}");

            return Ok(ApiResponse.Success(ToDto(existing), "Source type updated successfully."));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sourceType = await _context.SourceType.FindAsync(id);
            if (sourceType != null && !sourceType.IsDeleted)
            {
                sourceType.IsDeleted = true;
                sourceType.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                await LogAsync("SourceType.Delete", sourceType.SourceTypeID.ToString(), $"Deleted source type {sourceType.SourceTypeName}");
            }

            if (IsAjaxRequest())
            {
                return Ok(ApiResponse.Success<object?>(null, "Source type deleted successfully."));
            }

            return RedirectToAction(nameof(Index));
        }

        // ========== Helpers ==========
        private bool IsAjaxRequest() =>
            string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

        private IReadOnlyCollection<string> GetModelErrors()
        {
            return ModelState
                .Where(entry => entry.Value?.Errors?.Count > 0)
                .SelectMany(entry => entry.Value!.Errors.Select(error =>
                    string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? $"Invalid value for {entry.Key}"
                        : error.ErrorMessage))
                .ToArray();
        }

        private static SourceTypeDto ToDto(SourceType sourceType, int? emissionSourceCount = null)
        {
            return new SourceTypeDto
            {
                SourceTypeID = sourceType.SourceTypeID,
                SourceTypeName = sourceType.SourceTypeName,
                Description = sourceType.Description,
                IsActive = sourceType.IsActive,
                CreatedAt = sourceType.CreatedAt,
                UpdatedAt = sourceType.UpdatedAt,
                EmissionSourceCount = emissionSourceCount
            };
        }

        private IQueryable<SourceType> ActiveSourceTypes() =>
            _context.SourceType.Where(st => !st.IsDeleted);

        private bool SourceTypeExists(int id) =>
            ActiveSourceTypes().Any(e => e.SourceTypeID == id);

        public sealed class SourceTypeDto
        {
            public int SourceTypeID { get; set; }
            public string SourceTypeName { get; set; } = string.Empty;
            public string? Description { get; set; }
            public bool IsActive { get; set; }
            public DateTime? CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public int? EmissionSourceCount { get; set; }
        }

        private Task LogAsync(string action, string entityId, string description) =>
            _activityLogger.LogAsync(action, "SourceType", entityId, description);
    }
}

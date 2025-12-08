using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using env_analysis_project.Data;
using env_analysis_project.Models;
using env_analysis_project.Validators;

namespace env_analysis_project.Controllers
{
    public class EmissionSourcesController : Controller
    {
        private readonly env_analysis_projectContext _context;

        public EmissionSourcesController(env_analysis_projectContext context)
        {
            _context = context;
        }

        // =============================
        //  LIST VIEW
        // =============================
        public async Task<IActionResult> Index()
        {
            // Load emission sources and their types
            var emissionSources = await _context.EmissionSource
                .Include(e => e.SourceType)
                .OrderBy(e => e.SourceName)
                .ToListAsync();

            ViewBag.SourceTypes = await _context.SourceType
                .OrderBy(t => t.SourceTypeName)
                .ToListAsync();

            return View(emissionSources);
        }

        // =============================
        //  DETAIL (AJAX)
        // =============================
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var source = await _context.EmissionSource
                .Include(e => e.SourceType)
                .FirstOrDefaultAsync(e => e.EmissionSourceID == id);

            if (source == null)
                return NotFound(ApiResponse.Fail<EmissionSourceResponse>("Emission source not found."));

            return Ok(ApiResponse.Success(ToDto(source)));
        }

        // =============================
        //  CREATE
        // =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] EmissionSource model)
        {
            var validationErrors = EmissionSourceValidator.Validate(model).ToList();
            if (!ModelState.IsValid)
            {
                validationErrors.AddRange(GetModelErrors());
            }

            if (validationErrors.Count > 0)
            {
                return BadRequest(ApiResponse.Fail<EmissionSourceResponse>("Validation failed.", validationErrors));
            }

            model.CreatedAt = DateTime.Now;
            model.Location = string.IsNullOrWhiteSpace(model.Location) ? null : model.Location;
            model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description;

            _context.EmissionSource.Add(model);
            await _context.SaveChangesAsync();
            await _context.Entry(model).Reference(e => e.SourceType).LoadAsync();

            return Ok(ApiResponse.Success(ToDto(model), "Emission source created successfully!"));
        }

        // =============================
        //  EDIT
        // =============================
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [FromForm] EmissionSource model)
        {
            if (id != model.EmissionSourceID)
                return BadRequest(ApiResponse.Fail<EmissionSourceResponse>("Invalid emission source identifier."));

            var existing = await _context.EmissionSource.FindAsync(id);
            if (existing == null)
                return NotFound(ApiResponse.Fail<EmissionSourceResponse>("Emission source not found."));

            var validationErrors = EmissionSourceValidator.Validate(model).ToList();
            if (!ModelState.IsValid)
            {
                validationErrors.AddRange(GetModelErrors());
            }

            if (validationErrors.Count > 0)
                return BadRequest(ApiResponse.Fail<EmissionSourceResponse>("Validation failed.", validationErrors));

            existing.SourceCode = model.SourceCode;
            existing.SourceName = model.SourceName;
            existing.SourceTypeID = model.SourceTypeID;
            existing.Location = string.IsNullOrWhiteSpace(model.Location) ? null : model.Location;
            existing.Latitude = model.Latitude;
            existing.Longitude = model.Longitude;
            existing.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description;
            existing.IsActive = model.IsActive;
            existing.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            await _context.Entry(existing).Reference(e => e.SourceType).LoadAsync();

            return Ok(ApiResponse.Success(ToDto(existing), "Emission source updated successfully!"));
        }

        // =============================
        //  DELETE
        // =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromBody] DeleteEmissionSourceRequest request)
        {
            var validationErrors = EmissionSourceValidator.ValidateDelete(request).ToList();
            if (validationErrors.Count > 0)
            {
                return BadRequest(ApiResponse.Fail<object?>("Invalid emission source identifier.", validationErrors));
            }

            var emissionSource = await _context.EmissionSource.FindAsync(request.Id);
            if (emissionSource == null)
            {
                return NotFound(ApiResponse.Fail<object?>("Emission source not found."));
            }

            _context.EmissionSource.Remove(emissionSource);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.Success(new { request.Id }, "Emission source deleted successfully!"));
        }


        // =============================
        //  HELPER
        // =============================
        private bool EmissionSourceExists(int id)
        {
            return _context.EmissionSource.Any(e => e.EmissionSourceID == id);
        }

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

        private static EmissionSourceResponse ToDto(EmissionSource source)
        {
            return new EmissionSourceResponse
            {
                EmissionSourceID = source.EmissionSourceID,
                SourceCode = source.SourceCode,
                SourceName = source.SourceName,
                Description = source.Description,
                Location = source.Location,
                Latitude = source.Latitude,
                Longitude = source.Longitude,
                IsActive = source.IsActive,
                CreatedAt = source.CreatedAt,
                UpdatedAt = source.UpdatedAt,
                SourceTypeID = source.SourceTypeID,
                SourceTypeName = source.SourceType?.SourceTypeName
            };
        }

        private sealed class EmissionSourceResponse
        {
            public int EmissionSourceID { get; set; }
            public string SourceCode { get; set; } = string.Empty;
            public string SourceName { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string? Location { get; set; }
            public double? Latitude { get; set; }
            public double? Longitude { get; set; }
            public bool IsActive { get; set; }
            public DateTime? CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public int SourceTypeID { get; set; }
            public string? SourceTypeName { get; set; }
        }

        public sealed class DeleteEmissionSourceRequest
        {
            public int Id { get; set; }
        }
    }
}

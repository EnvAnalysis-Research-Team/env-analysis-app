using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using env_analysis_project.Data;
using env_analysis_project.Models;

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
        // LIST VIEW
        // =============================
        public async Task<IActionResult> Index()
        {
            var emissionSources = _context.EmissionSource
                .Include(e => e.SourceType)
                .OrderBy(e => e.SourceName);

            ViewBag.SourceTypes = await _context.SourceType
                .OrderBy(t => t.SourceTypeName)
                .ToListAsync();

            return View(await emissionSources.ToListAsync());
        }

        // =============================
        // GET DETAIL (AJAX)
        // =============================
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var source = await _context.EmissionSource
                .Include(e => e.SourceType)
                .FirstOrDefaultAsync(e => e.EmissionSourceID == id);

            if (source == null)
                return NotFound();

            // Dựng DTO trả về cho JS
            var result = new
            {
                source.EmissionSourceID,
                source.SourceCode,
                source.SourceName,
                source.Description,
                source.Location,
                source.Latitude,
                source.Longitude,
                source.IsActive,
                source.CreatedAt,
                source.UpdatedAt,
                source.SourceTypeID,
                SourceTypeName = source.SourceType?.SourceTypeName
            };

            return Json(result);
        }


        // =============================
        // CREATE (FORM)
        // =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SourceCode,SourceName,SourceTypeID,Location,Latitude,Longitude,Description,IsActive")] EmissionSource emissionSource)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { error = "Invalid data" });

            emissionSource.CreatedAt = DateTime.Now;
            emissionSource.UpdatedAt = DateTime.Now;

            _context.EmissionSource.Add(emissionSource);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // =============================
        // EDIT (AJAX)
        // =============================
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [FromForm] EmissionSource emissionSource)
        {
            if (id != emissionSource.EmissionSourceID)
                return BadRequest(new { error = "Mismatched ID" });

            var existing = await _context.EmissionSource.FindAsync(id);
            if (existing == null)
                return NotFound(new { error = "Emission Source not found" });

            if (!ModelState.IsValid)
                return BadRequest(new { error = "Invalid data" });

            // Update fields manually (tránh overwrite CreatedAt)
            existing.SourceCode = emissionSource.SourceCode;
            existing.SourceName = emissionSource.SourceName;
            existing.SourceTypeID = emissionSource.SourceTypeID;
            existing.Location = emissionSource.Location;
            existing.Latitude = emissionSource.Latitude;
            existing.Longitude = emissionSource.Longitude;
            existing.Description = emissionSource.Description;
            existing.IsActive = emissionSource.IsActive;
            existing.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // =============================
        // DELETE (AJAX)
        // =============================
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var emissionSource = await _context.EmissionSource.FindAsync(id);
            if (emissionSource == null)
                return NotFound(new { error = "Emission Source not found" });

            _context.EmissionSource.Remove(emissionSource);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // =============================
        // HELPER
        // =============================
        private bool EmissionSourceExists(int id)
        {
            return _context.EmissionSource.Any(e => e.EmissionSourceID == id);
        }
    }
}

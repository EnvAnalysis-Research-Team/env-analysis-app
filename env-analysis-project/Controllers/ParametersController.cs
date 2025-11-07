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
    public class ParametersController : Controller
    {
        private readonly env_analysis_projectContext _context;

        public ParametersController(env_analysis_projectContext context)
        {
            _context = context;
        }

        // GET: Parameters
        public async Task<IActionResult> Index()
        {
            return View(await _context.Parameter.ToListAsync());
        }

        // GET: Parameters/Manage
        [HttpGet]
        public IActionResult Manage()
        {
            return View();
        }

        // GET: Parameters/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parameter = await _context.Parameter
                .FirstOrDefaultAsync(m => m.ParameterCode == id);
            if (parameter == null)
            {
                return NotFound();
            }

            return View(parameter);
        }

        // GET: Parameters/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Parameters/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ParameterCode,ParameterName,Unit,StandardValue,Description,CreatedAt,UpdatedAt")] Parameter parameter)
        {
            if (ModelState.IsValid)
            {
                _context.Add(parameter);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(parameter);
        }

        // GET: Parameters/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parameter = await _context.Parameter.FindAsync(id);
            if (parameter == null)
            {
                return NotFound();
            }
            return View(parameter);
        }

        // POST: Parameters/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ParameterCode,ParameterName,Unit,StandardValue,Description,CreatedAt,UpdatedAt")] Parameter parameter)
        {
            if (id != parameter.ParameterCode)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(parameter);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ParameterExists(parameter.ParameterCode))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(parameter);
        }

        // GET: Parameters/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parameter = await _context.Parameter
                .FirstOrDefaultAsync(m => m.ParameterCode == id);
            if (parameter == null)
            {
                return NotFound();
            }

            return View(parameter);
        }

        // POST: Parameters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var parameter = await _context.Parameter.FindAsync(id);
            if (parameter != null)
            {
                _context.Parameter.Remove(parameter);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // =============================
        //  AJAX ENDPOINTS
        // =============================
        [HttpGet]
        public async Task<IActionResult> ListData()
        {
            var parameters = await _context.Parameter
                .OrderBy(p => p.ParameterName)
                .Select(p => ToDto(p))
                .ToListAsync();

            return Json(parameters);
        }

        [HttpGet]
        public async Task<IActionResult> DetailData(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new { success = false, error = "Parameter code is required." });
            }

            var parameter = await _context.Parameter.FindAsync(id);
            if (parameter == null)
            {
                return NotFound(new { success = false, error = "Parameter not found." });
            }

            return Json(ToDto(parameter));
        }

        [HttpPost]
        public async Task<IActionResult> CreateAjax([FromBody] ParameterDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.ParameterCode) || string.IsNullOrWhiteSpace(dto.ParameterName))
            {
                return BadRequest(new { success = false, error = "Parameter code and name are required." });
            }

            var code = dto.ParameterCode.Trim();
            if (await _context.Parameter.AnyAsync(p => p.ParameterCode == code))
            {
                return Conflict(new { success = false, error = $"Parameter with code '{code}' already exists." });
            }

            var entity = new Parameter
            {
                ParameterCode = code,
                ParameterName = dto.ParameterName.Trim(),
                Unit = string.IsNullOrWhiteSpace(dto.Unit) ? null : dto.Unit.Trim(),
                StandardValue = dto.StandardValue,
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Parameter.Add(entity);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Parameter created successfully.", data = ToDto(entity) });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAjax(string id, [FromBody] ParameterDto dto)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new { success = false, error = "Parameter code is required." });
            }

            var parameter = await _context.Parameter.FindAsync(id);
            if (parameter == null)
            {
                return NotFound(new { success = false, error = "Parameter not found." });
            }

            if (dto == null || string.IsNullOrWhiteSpace(dto.ParameterName))
            {
                return BadRequest(new { success = false, error = "Parameter name is required." });
            }

            parameter.ParameterName = dto.ParameterName.Trim();
            parameter.Unit = string.IsNullOrWhiteSpace(dto.Unit) ? null : dto.Unit.Trim();
            parameter.StandardValue = dto.StandardValue;
            parameter.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
            parameter.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Parameter updated successfully.", data = ToDto(parameter) });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAjax(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new { success = false, error = "Parameter code is required." });
            }

            var parameter = await _context.Parameter.FindAsync(id);
            if (parameter == null)
            {
                return NotFound(new { success = false, error = "Parameter not found." });
            }

            _context.Parameter.Remove(parameter);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Parameter deleted successfully." });
        }

        private bool ParameterExists(string id)
        {
            return _context.Parameter.Any(e => e.ParameterCode == id);
        }

        private static ParameterDto ToDto(Parameter parameter)
        {
            return new ParameterDto
            {
                ParameterCode = parameter.ParameterCode,
                ParameterName = parameter.ParameterName,
                Unit = parameter.Unit,
                StandardValue = parameter.StandardValue,
                Description = parameter.Description,
                CreatedAt = parameter.CreatedAt,
                UpdatedAt = parameter.UpdatedAt
            };
        }

        public sealed class ParameterDto
        {
            public string ParameterCode { get; set; }
            public string ParameterName { get; set; }
            public string Unit { get; set; }
            public double? StandardValue { get; set; }
            public string Description { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
        }
    }
}

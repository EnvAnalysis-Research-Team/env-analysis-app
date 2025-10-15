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
    public class MeasurementResultsController : Controller
    {
        private readonly env_analysis_projectContext _context;

        public MeasurementResultsController(env_analysis_projectContext context)
        {
            _context = context;
        }

        // GET: MeasurementResults
        public async Task<IActionResult> Index()
        {
            var env_analysis_projectContext = _context.MeasurementResult.Include(m => m.EmissionSource).Include(m => m.Parameter);
            return View(await env_analysis_projectContext.ToListAsync());
        }

        // GET: MeasurementResults/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var measurementResult = await _context.MeasurementResult
                .Include(m => m.EmissionSource)
                .Include(m => m.Parameter)
                .FirstOrDefaultAsync(m => m.ResultID == id);
            if (measurementResult == null)
            {
                return NotFound();
            }

            return View(measurementResult);
        }

        // GET: MeasurementResults/Create
        public IActionResult Create()
        {
            ViewData["EmissionSourceID"] = new SelectList(_context.EmissionSource, "EmissionSourceID", "SourceCode");
            ViewData["ParameterCode"] = new SelectList(_context.Set<Parameter>(), "ParameterCode", "ParameterCode");
            return View();
        }

        // POST: MeasurementResults/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ResultID,EmissionSourceID,ParameterCode,MeasurementDate,Value,Unit,EntryDate,Remark,IsApproved,ApprovedAt")] MeasurementResult measurementResult)
        {
            if (ModelState.IsValid)
            {
                _context.Add(measurementResult);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmissionSourceID"] = new SelectList(_context.EmissionSource, "EmissionSourceID", "SourceCode", measurementResult.EmissionSourceID);
            ViewData["ParameterCode"] = new SelectList(_context.Set<Parameter>(), "ParameterCode", "ParameterCode", measurementResult.ParameterCode);
            return View(measurementResult);
        }

        // GET: MeasurementResults/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var measurementResult = await _context.MeasurementResult.FindAsync(id);
            if (measurementResult == null)
            {
                return NotFound();
            }
            ViewData["EmissionSourceID"] = new SelectList(_context.EmissionSource, "EmissionSourceID", "SourceCode", measurementResult.EmissionSourceID);
            ViewData["ParameterCode"] = new SelectList(_context.Set<Parameter>(), "ParameterCode", "ParameterCode", measurementResult.ParameterCode);
            return View(measurementResult);
        }

        // POST: MeasurementResults/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ResultID,EmissionSourceID,ParameterCode,MeasurementDate,Value,Unit,EntryDate,Remark,IsApproved,ApprovedAt")] MeasurementResult measurementResult)
        {
            if (id != measurementResult.ResultID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(measurementResult);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MeasurementResultExists(measurementResult.ResultID))
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
            ViewData["EmissionSourceID"] = new SelectList(_context.EmissionSource, "EmissionSourceID", "SourceCode", measurementResult.EmissionSourceID);
            ViewData["ParameterCode"] = new SelectList(_context.Set<Parameter>(), "ParameterCode", "ParameterCode", measurementResult.ParameterCode);
            return View(measurementResult);
        }

        // GET: MeasurementResults/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var measurementResult = await _context.MeasurementResult
                .Include(m => m.EmissionSource)
                .Include(m => m.Parameter)
                .FirstOrDefaultAsync(m => m.ResultID == id);
            if (measurementResult == null)
            {
                return NotFound();
            }

            return View(measurementResult);
        }

        // POST: MeasurementResults/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var measurementResult = await _context.MeasurementResult.FindAsync(id);
            if (measurementResult != null)
            {
                _context.MeasurementResult.Remove(measurementResult);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MeasurementResultExists(int id)
        {
            return _context.MeasurementResult.Any(e => e.ResultID == id);
        }
    }
}

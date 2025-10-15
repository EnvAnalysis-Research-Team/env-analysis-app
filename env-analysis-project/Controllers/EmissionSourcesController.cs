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

        // GET: EmissionSources
        public async Task<IActionResult> Index()
        {
            var env_analysis_projectContext = _context.EmissionSource.Include(e => e.SourceType);
            return View(await env_analysis_projectContext.ToListAsync());
        }

        // GET: EmissionSources/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var emissionSource = await _context.EmissionSource
                .Include(e => e.SourceType)
                .FirstOrDefaultAsync(m => m.EmissionSourceID == id);
            if (emissionSource == null)
            {
                return NotFound();
            }

            return View(emissionSource);
        }

        // GET: EmissionSources/Create
        public IActionResult Create()
        {
            ViewData["SourceTypeID"] = new SelectList(_context.Set<SourceType>(), "SourceTypeID", "SourceTypeName");
            return View();
        }

        // POST: EmissionSources/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EmissionSourceID,SourceCode,SourceName,SourceTypeID,Location,Latitude,Longitude,Description,IsActive,CreatedAt,UpdatedAt")] EmissionSource emissionSource)
        {
            if (ModelState.IsValid)
            {
                _context.Add(emissionSource);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SourceTypeID"] = new SelectList(_context.Set<SourceType>(), "SourceTypeID", "SourceTypeName", emissionSource.SourceTypeID);
            return View(emissionSource);
        }

        // GET: EmissionSources/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var emissionSource = await _context.EmissionSource.FindAsync(id);
            if (emissionSource == null)
            {
                return NotFound();
            }
            ViewData["SourceTypeID"] = new SelectList(_context.Set<SourceType>(), "SourceTypeID", "SourceTypeName", emissionSource.SourceTypeID);
            return View(emissionSource);
        }

        // POST: EmissionSources/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EmissionSourceID,SourceCode,SourceName,SourceTypeID,Location,Latitude,Longitude,Description,IsActive,CreatedAt,UpdatedAt")] EmissionSource emissionSource)
        {
            if (id != emissionSource.EmissionSourceID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(emissionSource);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmissionSourceExists(emissionSource.EmissionSourceID))
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
            ViewData["SourceTypeID"] = new SelectList(_context.Set<SourceType>(), "SourceTypeID", "SourceTypeName", emissionSource.SourceTypeID);
            return View(emissionSource);
        }

        // GET: EmissionSources/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var emissionSource = await _context.EmissionSource
                .Include(e => e.SourceType)
                .FirstOrDefaultAsync(m => m.EmissionSourceID == id);
            if (emissionSource == null)
            {
                return NotFound();
            }

            return View(emissionSource);
        }

        // POST: EmissionSources/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var emissionSource = await _context.EmissionSource.FindAsync(id);
            if (emissionSource != null)
            {
                _context.EmissionSource.Remove(emissionSource);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmissionSourceExists(int id)
        {
            return _context.EmissionSource.Any(e => e.EmissionSourceID == id);
        }
    }
}

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
    public class SourceTypesController : Controller
    {
        private readonly env_analysis_projectContext _context;

        public SourceTypesController(env_analysis_projectContext context)
        {
            _context = context;
        }

        // GET: SourceTypes
        public async Task<IActionResult> Index()
        {
            return View(await _context.SourceType.ToListAsync());
        }

        // GET: SourceTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sourceType = await _context.SourceType
                .FirstOrDefaultAsync(m => m.SourceTypeID == id);
            if (sourceType == null)
            {
                return NotFound();
            }

            return View(sourceType);
        }

        // GET: SourceTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SourceTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SourceTypeID,SourceTypeName,Description,IsActive,CreatedAt,UpdatedAt")] SourceType sourceType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sourceType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sourceType);
        }

        // GET: SourceTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sourceType = await _context.SourceType.FindAsync(id);
            if (sourceType == null)
            {
                return NotFound();
            }
            return View(sourceType);
        }

        // POST: SourceTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SourceTypeID,SourceTypeName,Description,IsActive,CreatedAt,UpdatedAt")] SourceType sourceType)
        {
            if (id != sourceType.SourceTypeID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sourceType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SourceTypeExists(sourceType.SourceTypeID))
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
            return View(sourceType);
        }

        // GET: SourceTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sourceType = await _context.SourceType
                .FirstOrDefaultAsync(m => m.SourceTypeID == id);
            if (sourceType == null)
            {
                return NotFound();
            }

            return View(sourceType);
        }

        // POST: SourceTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sourceType = await _context.SourceType.FindAsync(id);
            if (sourceType != null)
            {
                _context.SourceType.Remove(sourceType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SourceTypeExists(int id)
        {
            return _context.SourceType.Any(e => e.SourceTypeID == id);
        }
    }
}

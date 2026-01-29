using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using env_analysis_project.Data;
using env_analysis_project.Models;

namespace env_analysis_project.Controllers
{
    public class AlertsForecastsController : Controller
    {
        private readonly env_analysis_projectContext _context;

        public AlertsForecastsController(env_analysis_projectContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Alerts & Forecasts";
            var emissionSources = await _context.EmissionSource
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.SourceName)
                .Select(s => new
                {
                    Id = s.EmissionSourceID,
                    Label = s.SourceName
                })
                .ToListAsync();

            var parameters = await _context.Parameter
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.ParameterName)
                .Select(p => new
                {
                    Code = p.ParameterCode,
                    Label = p.ParameterName,
                    Unit = p.Unit,
                    StandardValue = p.StandardValue,
                    Type = ParameterTypeHelper.Normalize(p.Type)
                })
                .ToListAsync();

            ViewBag.EmissionSources = emissionSources;
            ViewBag.Parameters = parameters;

            return View();
        }
    }
}

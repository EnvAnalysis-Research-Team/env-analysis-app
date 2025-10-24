using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using env_analysis_project.Data;

namespace env_analysis_project.Controllers
{
    public class SourceManagementController : Controller
    {
        private readonly env_analysis_projectContext _context;

        public SourceManagementController(env_analysis_projectContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Serve Manage view with emission sources model and source types (used by the sidebar)
        public async Task<IActionResult> Manage()
        {
            var sources = await _context.EmissionSource
                .Include(e => e.SourceType)
                .OrderBy(e => e.SourceName)
                .ToListAsync();

            var sourceTypes = await _context.SourceType
                .Select(st => new
                {
                    st.SourceTypeID,
                    st.SourceTypeName,
                    st.Description,
                    st.IsActive,
                    st.CreatedAt,
                    st.UpdatedAt,
                    Count = _context.EmissionSource.Count(es => es.SourceTypeID == st.SourceTypeID)
                })
                .ToListAsync();

            ViewBag.SourceTypes = sourceTypes;
            return View("Manage", sources);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using env_analysis_project.Data;
using env_analysis_project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace env_analysis_project.Controllers
{
    [Authorize]
    public class SystemLogController : Controller
    {
        private const int DefaultPageSize = 25;
        private const int MaxPageSize = 100;
        private readonly env_analysis_projectContext _context;

        public SystemLogController(env_analysis_projectContext context)
        {
            _context = context;
        }

        [HttpGet]
        public Task<IActionResult> Index(string? search, string? actionType, DateTime? from, DateTime? to, int page = 1, int pageSize = DefaultPageSize) =>
            Manage(search, actionType, from, to, page, pageSize);

        [HttpGet]
        public async Task<IActionResult> Manage(string? search, string? actionType, DateTime? from, DateTime? to, int page = 1, int pageSize = DefaultPageSize)
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Home", new { accessDenied = 1 });
            }

            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 10, MaxPageSize);

            var query = BuildFilteredQuery(search, actionType, from, to);

            var totalItems = await query.CountAsync();
            var totalPages = Math.Max((int)Math.Ceiling(totalItems / (double)pageSize), 1);
            if (page > totalPages)
            {
                page = totalPages;
            }

            var items = await query
                .OrderByDescending(log => log.OccurredAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(log => new SystemLogRow
                {
                    Id = log.Id,
                    OccurredAt = log.OccurredAt,
                    UserDisplayName = log.User != null
                        ? (!string.IsNullOrEmpty(log.User.FullName)
                            ? log.User.FullName
                            : (log.User.Email ?? log.User.Id))
                        : "Unknown",
                    UserEmail = log.User != null ? log.User.Email : null,
                    ActionType = log.ActionType,
                    EntityName = log.EntityName,
                    EntityId = log.EntityId,
                    Description = log.Description,
                    MetadataJson = log.MetadataJson
                })
                .ToListAsync();

            var actionOptions = await _context.UserActivityLogs
                .Select(log => log.ActionType)
                .Distinct()
                .OrderBy(x => x)
                .Take(100)
                .ToListAsync();

            var model = new SystemLogViewModel
            {
                Items = items,
                Search = search,
                ActionType = actionType,
                From = from,
                To = to,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            ViewBag.ActionOptions = actionOptions;
            return View("Manage", model);
        }

        [HttpGet]
        public async Task<IActionResult> Export(string? search, string? actionType, DateTime? from, DateTime? to)
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Home", new { accessDenied = 1 });
            }

            var query = BuildFilteredQuery(search, actionType, from, to);

            var logs = await query
                .OrderByDescending(log => log.OccurredAt)
                .Select(log => new
                {
                    log.OccurredAt,
                    UserName = log.User != null
                        ? (!string.IsNullOrEmpty(log.User.FullName)
                            ? log.User.FullName
                            : (log.User.Email ?? log.User.Id))
                        : "Unknown",
                    log.UserId,
                    log.ActionType,
                    log.EntityName,
                    log.EntityId,
                    log.Description,
                    log.MetadataJson
                })
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Occurred At,User,User Id,Action,Entity Name,Entity Id,Description,Metadata");

            foreach (var log in logs)
            {
                csv.AppendLine(string.Join(',',
                    EscapeCsv(log.OccurredAt.ToString("u")),
                    EscapeCsv(log.UserName),
                    EscapeCsv(log.UserId),
                    EscapeCsv(log.ActionType),
                    EscapeCsv(log.EntityName),
                    EscapeCsv(log.EntityId),
                    EscapeCsv(log.Description),
                    EscapeCsv(log.MetadataJson)));
            }

            var fileName = $"system-log-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", fileName);
        }

        private IQueryable<UserActivityLog> BuildFilteredQuery(string? search, string? actionType, DateTime? from, DateTime? to)
        {
            var query = _context.UserActivityLogs
                .Include(log => log.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();
                query = query.Where(log =>
                    (log.User != null && (
                        (!string.IsNullOrEmpty(log.User.FullName) && log.User.FullName.Contains(keyword)) ||
                        (!string.IsNullOrEmpty(log.User.Email) && log.User.Email.Contains(keyword)))) ||
                    (!string.IsNullOrEmpty(log.ActionType) && log.ActionType.Contains(keyword)) ||
                    (!string.IsNullOrEmpty(log.Description) && log.Description.Contains(keyword)) ||
                    (!string.IsNullOrEmpty(log.EntityName) && log.EntityName.Contains(keyword)) ||
                    (!string.IsNullOrEmpty(log.EntityId) && log.EntityId.Contains(keyword)));
            }

            if (!string.IsNullOrWhiteSpace(actionType))
            {
                var normalizedAction = actionType.Trim();
                query = query.Where(log => log.ActionType == normalizedAction);
            }

            if (from.HasValue)
            {
                query = query.Where(log => log.OccurredAt >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(log => log.OccurredAt <= to.Value);
            }

            return query;
        }

        private static string EscapeCsv(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "\"\"";
            }

            var sanitized = value.Replace("\"", "\"\"");
            return $"\"{sanitized}\"";
        }

        public sealed class SystemLogViewModel
        {
            public IReadOnlyList<SystemLogRow> Items { get; set; } = Array.Empty<SystemLogRow>();
            public string? Search { get; set; }
            public string? ActionType { get; set; }
            public DateTime? From { get; set; }
            public DateTime? To { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public int TotalItems { get; set; }
            public int StartItem => TotalItems == 0 ? 0 : (Page - 1) * PageSize + 1;
            public int EndItem => TotalItems == 0 ? 0 : Math.Min(Page * PageSize, TotalItems);
        }

        public sealed class SystemLogRow
        {
            public int Id { get; set; }
            public DateTime OccurredAt { get; set; }
            public string UserDisplayName { get; set; } = string.Empty;
            public string? UserEmail { get; set; }
            public string ActionType { get; set; } = string.Empty;
            public string? EntityName { get; set; }
            public string? EntityId { get; set; }
            public string? Description { get; set; }
            public string? MetadataJson { get; set; }
        }
    }
}

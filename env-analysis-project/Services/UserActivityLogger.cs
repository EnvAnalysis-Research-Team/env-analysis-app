using System;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using env_analysis_project.Data;
using env_analysis_project.Models;
using Microsoft.AspNetCore.Http;

namespace env_analysis_project.Services
{
    public interface IUserActivityLogger
    {
        Task LogAsync(string actionType, string? entityName = null, string? entityId = null, string? description = null, object? metadata = null, string? userId = null);
    }

    public sealed class UserActivityLogger : IUserActivityLogger
    {
        private readonly env_analysis_projectContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserActivityLogger(env_analysis_projectContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string actionType, string? entityName = null, string? entityId = null, string? description = null, object? metadata = null, string? userId = null)
        {
            var resolvedUserId = userId ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(resolvedUserId))
            {
                return;
            }

            var logEntry = new UserActivityLog
            {
                UserId = resolvedUserId,
                ActionType = actionType,
                EntityName = entityName,
                EntityId = entityId,
                Description = description,
                OccurredAt = DateTime.UtcNow,
                MetadataJson = metadata == null ? null : JsonSerializer.Serialize(metadata)
            };

            await _context.UserActivityLogs.AddAsync(logEntry);
            await _context.SaveChangesAsync();
        }
    }
}

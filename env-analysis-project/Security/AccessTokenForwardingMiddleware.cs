using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace env_analysis_project.Security
{
    /// <summary>
    /// Ensures downstream components see the JWT token even when it is stored in cookies.
    /// The middleware copies the access token cookie into the Authorization header if it is missing.
    /// </summary>
    public sealed class AccessTokenForwardingMiddleware
    {
        private readonly RequestDelegate _next;

        public AccessTokenForwardingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.ContainsKey("Authorization") &&
                context.Request.Cookies.TryGetValue(JwtDefaults.AccessTokenCookieName, out var token) &&
                !string.IsNullOrWhiteSpace(token))
            {
                context.Request.Headers["Authorization"] = $"Bearer {token}";
            }

            await _next(context);
        }
    }
}

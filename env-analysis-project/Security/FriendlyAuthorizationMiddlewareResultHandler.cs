using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace env_analysis_project.Security
{
    /// <summary>
    /// Redirects forbidden HTML requests back to the previous page (or home) with a query flag instead of showing a blank 403 page.
    /// </summary>
    public sealed class FriendlyAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

        public async Task HandleAsync(
            RequestDelegate next,
            HttpContext context,
            AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult)
        {
            if (authorizeResult.Forbidden && IsHtmlRequest(context.Request))
            {
                var redirectUrl = ResolveRedirectUrl(context);
                context.Response.Redirect(redirectUrl);
                return;
            }

            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }

        private static bool IsHtmlRequest(HttpRequest request)
        {
            if (!HttpMethods.IsGet(request.Method) && !HttpMethods.IsHead(request.Method))
            {
                return false;
            }

            var acceptHeaders = request.GetTypedHeaders().Accept;
            if (acceptHeaders == null || acceptHeaders.Count == 0)
            {
                return true;
            }

            return acceptHeaders.Any(header =>
            {
                var mediaType = header.MediaType.Value;
                return !string.IsNullOrWhiteSpace(mediaType) &&
                       mediaType.IndexOf("html", StringComparison.OrdinalIgnoreCase) >= 0;
            });
        }

        private static string ResolveRedirectUrl(HttpContext context)
        {
            var referer = context.Request.Headers["Referer"].ToString();
            if (Uri.TryCreate(referer, UriKind.Absolute, out var refererUri) &&
                string.Equals(refererUri.Host, context.Request.Host.Host, StringComparison.OrdinalIgnoreCase))
            {
                var baseUrl = refererUri.GetLeftPart(UriPartial.Path) + refererUri.Query;
                return AppendAccessDeniedFlag(baseUrl);
            }

            return AppendAccessDeniedFlag("/");
        }

        private static string AppendAccessDeniedFlag(string url)
        {
            if (url.Contains("accessDenied=1", StringComparison.OrdinalIgnoreCase))
            {
                return url;
            }

            var separator = url.Contains('?') ? '&' : '?';
            return $"{url}{separator}accessDenied=1";
        }
    }
}

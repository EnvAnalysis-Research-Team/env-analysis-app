// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using env_analysis_project.Models;
using env_analysis_project.Security;
using env_analysis_project.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace env_analysis_project.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly IUserActivityLogger _activityLogger;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger, IUserActivityLogger activityLogger)
        {
            _signInManager = signInManager;
            _logger = logger;
            _activityLogger = activityLogger;
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User?.Identity?.Name;
            await _signInManager.SignOutAsync();
            Response.Cookies.Delete(JwtDefaults.AccessTokenCookieName);
            _logger.LogInformation("User logged out.");
            if (!string.IsNullOrWhiteSpace(userId))
            {
                await _activityLogger.LogAsync("Auth.Logout", "Identity", userId, $"User {userName ?? userId} logged out.", null, userId);
            }
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                // This needs to be a redirect so that the browser performs a new
                // request and the identity for the user gets updated.
                return RedirectToPage();
            }
        }
    }
}

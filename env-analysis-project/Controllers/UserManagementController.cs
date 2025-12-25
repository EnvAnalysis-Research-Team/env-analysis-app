using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using env_analysis_project.Models;
using env_analysis_project.Validators;

namespace env_analysis_project.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index(string? searchString, string? roleFilter, string? sortOption, int page = 1, int pageSize = 10)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 5, 100);

            var query = BuildUserQuery(searchString, roleFilter, sortOption);
            var totalItems = query.Count();
            var totalPages = totalItems == 0 ? 1 : (int)Math.Ceiling(totalItems / (double)pageSize);
            if (totalItems > 0 && page > totalPages)
            {
                page = totalPages;
            }
            else if (totalItems == 0)
            {
                page = 1;
                totalPages = 1;
            }

            var users = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.AvailableRoles = _roleManager.Roles
                .Select(role => role.Name ?? string.Empty)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .OrderBy(name => name)
                .ToList();
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = Math.Max(totalPages, 1);

            return View("Manage", users);
        }

        [HttpGet]
        public IActionResult Export(string? searchString, string? roleFilter, string? sortOption)
        {
            var users = BuildUserQuery(searchString, roleFilter, sortOption).ToList();
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Full Name,Email,Role,Created At,Updated At");

            foreach (var user in users)
            {
                csvBuilder.Append(EscapeCsv(user.FullName));
                csvBuilder.Append(',');
                csvBuilder.Append(EscapeCsv(user.Email));
                csvBuilder.Append(',');
                csvBuilder.Append(EscapeCsv(user.Role));
                csvBuilder.Append(',');
                csvBuilder.Append(EscapeCsv(user.CreatedAt?.ToString("u")));
                csvBuilder.Append(',');
                csvBuilder.AppendLine(EscapeCsv(user.UpdatedAt?.ToString("u")));
            }

            var fileName = $"users-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", fileName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            if (request == null)
            {
                return HandleFailure(new[] { "Request payload is required." }, "Invalid request.");
            }

            var validationErrors = UserValidator.Validate(ToApplicationUser(request)).ToList();
            if (!ModelState.IsValid)
            {
                validationErrors.AddRange(GetModelErrors());
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                validationErrors.Add("Password is required.");
            }

            if (validationErrors.Count > 0)
            {
                return HandleFailure(validationErrors, "Validation failed.");
            }

            var user = new ApplicationUser
            {
                Email = request.Email?.Trim(),
                UserName = request.Email?.Trim(),
                FullName = request.FullName?.Trim(),
                Role = string.IsNullOrWhiteSpace(request.Role) ? null : request.Role.Trim(),
                PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, request.Password!);
            if (!createResult.Succeeded)
            {
                var identityErrors = createResult.Errors.Select(error => error.Description).ToList();
                return HandleFailure(identityErrors, "Failed to create user.");
            }

            if (!string.IsNullOrEmpty(user.Role))
            {
                await EnsureRoleAssignmentAsync(user, user.Role);
            }

            return HandleSuccess(ToDto(user), "User created successfully.");
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(ApiResponse.Fail<UserResponse>("User identifier is required."));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse.Fail<UserResponse>("User not found."));
            }

            return Ok(ApiResponse.Success(ToDto(user)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([FromBody] UpdateUserRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Id))
            {
                return HandleFailure(new[] { "User identifier is required." }, "Invalid request.");
            }

            var validationErrors = UserValidator.ValidateForUpdate(ToApplicationUser(request)).ToList();
            if (!ModelState.IsValid)
            {
                validationErrors.AddRange(GetModelErrors());
            }

            if (validationErrors.Count > 0)
            {
                return HandleFailure(validationErrors, "Validation failed.");
            }

            var user = await _userManager.FindByIdAsync(request.Id!);
            if (user == null)
            {
                return HandleNotFound();
            }

            user.Email = request.Email?.Trim();
            user.UserName = request.Email?.Trim();
            user.FullName = request.FullName?.Trim();
            user.Role = string.IsNullOrWhiteSpace(request.Role) ? null : request.Role.Trim();
            user.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = updateResult.Errors.Select(error => error.Description).ToList();
                return HandleFailure(errors, "Failed to update user.");
            }

            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                await EnsureRoleAssignmentAsync(user, request.Role);
            }
            else
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                }
            }

            return HandleSuccess(ToDto(user), "User updated successfully.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromBody] DeleteUserRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Id))
            {
                return HandleFailure(new[] { "User identifier is required." }, "Invalid request.");
            }

            var user = await _userManager.FindByIdAsync(request.Id);
            if (user == null)
            {
                return HandleNotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(error => error.Description).ToList();
                return HandleFailure(errors, "Failed to delete user.");
            }

            return HandleSuccess(new { request.Id }, "User deleted successfully.");
        }

        private bool IsAjaxRequest()
        {
            return string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
        }

        private IReadOnlyCollection<string> GetModelErrors()
        {
            return ModelState
                .Where(entry => entry.Value?.Errors?.Count > 0)
                .SelectMany(entry => entry.Value!.Errors.Select(error =>
                    string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? $"Invalid value for {entry.Key}"
                        : error.ErrorMessage))
                .ToArray();
        }

        private async Task EnsureRoleAssignmentAsync(ApplicationUser user, string roleName)
        {
            var normalizedRole = roleName.Trim();
            if (!await _roleManager.RoleExistsAsync(normalizedRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(normalizedRole));
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            await _userManager.AddToRoleAsync(user, normalizedRole);
        }

        private IActionResult HandleSuccess<T>(T? payload, string message)
        {
            if (IsAjaxRequest())
            {
                return Ok(ApiResponse.Success(payload, message));
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        private IActionResult HandleFailure(IEnumerable<string> errors, string message)
        {
            var errorList = errors.Where(error => !string.IsNullOrWhiteSpace(error)).ToArray();
            if (IsAjaxRequest())
            {
                return BadRequest(ApiResponse.Fail<object?>(message, errorList));
            }

            TempData["Error"] = string.Join(Environment.NewLine, errorList);
            return RedirectToAction(nameof(Index));
        }

        private IActionResult HandleNotFound()
        {
            if (IsAjaxRequest())
            {
                return NotFound(ApiResponse.Fail<object?>("User not found."));
            }

            TempData["Error"] = "User not found.";
            return RedirectToAction(nameof(Index));
        }

        private IQueryable<ApplicationUser> BuildUserQuery(string? searchString, string? roleFilter, string? sortOption)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var keyword = searchString.Trim();
                query = query.Where(user =>
                    (!string.IsNullOrEmpty(user.Email) && user.Email.Contains(keyword)) ||
                    (!string.IsNullOrEmpty(user.FullName) && user.FullName.Contains(keyword)));
            }

            if (!string.IsNullOrWhiteSpace(roleFilter))
            {
                query = query.Where(user => user.Role == roleFilter);
            }

            return sortOption switch
            {
                "date_asc" => query.OrderBy(user => user.CreatedAt),
                "name_asc" => query.OrderBy(user => user.FullName),
                "name_desc" => query.OrderByDescending(user => user.FullName),
                _ => query.OrderByDescending(user => user.CreatedAt)
            };
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

        private static ApplicationUser ToApplicationUser(CreateUserRequest request) =>
            new()
            {
                Email = request.Email,
                FullName = request.FullName,
                Role = request.Role
            };

        private static ApplicationUser ToApplicationUser(UpdateUserRequest request) =>
            new()
            {
                Id = request.Id ?? string.Empty,
                Email = request.Email,
                FullName = request.FullName,
                Role = request.Role
            };

        private static UserResponse ToDto(ApplicationUser user) =>
            new()
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName ?? string.Empty,
                Role = user.Role,
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

        public sealed class UserResponse
        {
            public string Id { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public string? Role { get; set; }
            public string? PhoneNumber { get; set; }
            public DateTime? CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
        }

        public sealed class CreateUserRequest
        {
            public string? Email { get; set; }
            public string? FullName { get; set; }
            public string? Role { get; set; }
            public string? Password { get; set; }
            public string? PhoneNumber { get; set; }
        }

        public sealed class UpdateUserRequest
        {
            public string? Id { get; set; }
            public string? Email { get; set; }
            public string? FullName { get; set; }
            public string? Role { get; set; }
        }

        public sealed class DeleteUserRequest
        {
            public string? Id { get; set; }
        }
    }
}

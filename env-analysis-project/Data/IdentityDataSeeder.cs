using System;
using System.Linq;
using Bogus;
using env_analysis_project.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace env_analysis_project.Data
{
    public static class IdentityDataSeeder
    {
        private const string DefaultAdminEmail = "admin@gmail.com";
        private const string DefaultAdminPassword = "12345678";

        public static async Task SeedAsync(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var logger = scope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(IdentityDataSeeder));

            var defaultRoles = new[] { "Admin", "User" };
            foreach (var roleName in defaultRoles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var createRoleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                    if (!createRoleResult.Succeeded)
                    {
                        var error = string.Join(", ", createRoleResult.Errors.Select(e => e.Description));
                        logger.LogError("Failed to seed role {Role}: {Error}", roleName, error);
                        return;
                    }
                }
            }

            var existingUser = await userManager.FindByEmailAsync(DefaultAdminEmail);
            if (existingUser != null)
            {
                var needsUpdate = !string.Equals(existingUser.Role, "Admin", StringComparison.OrdinalIgnoreCase);
                if (needsUpdate)
                {
                    existingUser.Role = "Admin";
                    await userManager.UpdateAsync(existingUser);
                }

                if (!await userManager.IsInRoleAsync(existingUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(existingUser, "Admin");
                }

                logger.LogInformation("Default admin user already exists. Ensured Admin role is applied.");
                return;
            }

            var faker = new Faker("vi");
            var adminUser = new ApplicationUser
            {
                Email = DefaultAdminEmail,
                UserName = DefaultAdminEmail,
                EmailConfirmed = true,
                FullName = faker.Name.FullName(),
                PhoneNumber = faker.Phone.PhoneNumber("09########"),
                Role = "Admin",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(adminUser, DefaultAdminPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(error => error.Description));
                logger.LogError("Failed to seed default admin user: {Errors}", errors);
                return;
            }

            await userManager.AddToRoleAsync(adminUser, "Admin");
            logger.LogInformation("Seeded default admin account {Email} with Bogus profile data.", DefaultAdminEmail);
        }
    }
}

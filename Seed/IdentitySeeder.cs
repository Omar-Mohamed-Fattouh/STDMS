using Microsoft.AspNetCore.Identity;
using STDMS.Models;

namespace STDMS.Seed
{
    public static class IdentitySeeder
    {
        private static readonly string[] Roles =
        [
            RoleNames.Administrator,
            RoleNames.AcademicAdministrator,
            RoleNames.Instructor,
            RoleNames.ReadOnlyStaff
        ];

        public static async Task InitializeAsync(
            IServiceProvider services,
            IConfiguration configuration,
            ILogger logger)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (var role in Roles)
            {
                if (await roleManager.RoleExistsAsync(role))
                {
                    continue;
                }

                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Could not create the required '{role}' role: {string.Join("; ", result.Errors.Select(error => error.Description))}");
                }
            }

            var email = configuration["BootstrapAdmin:Email"];
            var password = configuration["BootstrapAdmin:Password"];
            if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(password))
            {
                logger.LogWarning(
                    "No bootstrap administrator was configured. Set BootstrapAdmin:Email and BootstrapAdmin:Password before deployment.");
                return;
            }

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    "Both BootstrapAdmin:Email and BootstrapAdmin:Password must be configured together.");
            }

            var administrator = await userManager.FindByEmailAsync(email);
            if (administrator is null)
            {
                administrator = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(administrator, password);
                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Could not create the bootstrap administrator: {string.Join("; ", createResult.Errors.Select(error => error.Description))}");
                }
            }

            if (!await userManager.IsInRoleAsync(administrator, RoleNames.Administrator))
            {
                var roleResult = await userManager.AddToRoleAsync(administrator, RoleNames.Administrator);
                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Could not assign the bootstrap administrator role: {string.Join("; ", roleResult.Errors.Select(error => error.Description))}");
                }
            }
        }
    }
}

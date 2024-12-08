using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace PalletOptimization.Utilities
{
    public static class IdentitySeeder
    {
        public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
        {
            //We use roleManager to create roles like Admin
            //We use this to check if users exist, create new users etc.
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>(); 
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var adminRole = "Admin"; //The name for the role
            var adminEmail = Environment.GetEnvironmentVariable("Admin_Email");
            var adminPassword = Environment.GetEnvironmentVariable("Admin_Password");

            // Ensure the role exists
            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            }

            // Create the admin user
            var adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, adminRole);
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    Debug.WriteLine($"- {error.Description}"); 
                }
            }
        }
    }
}
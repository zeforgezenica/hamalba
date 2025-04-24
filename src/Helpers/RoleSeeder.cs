using Microsoft.AspNetCore.Identity;
using hamalba.Models;

namespace hamalba.Helpers
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAndAdmin(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<Korisnik>>();

            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new IdentityRole("User"));

            string adminEmail = "admin@hamalba.com";
            string adminPassword = "Admin123!";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var user = new Korisnik
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Ime = "Admin",
                    Prezime = "Account",
                    Verifikovan = true,
                    DatumRegistracije = DateTime.UtcNow,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }

            string testEmail = "user@hamalba.com";
            string testPassword = "User123!";

            var testUser = await userManager.FindByEmailAsync(testEmail);
            if (testUser == null)
            {
                var user = new Korisnik
                {
                    UserName = testEmail,
                    Email = testEmail,
                    Ime = "Test",
                    Prezime = "User",
                    Verifikovan = true,
                    DatumRegistracije = DateTime.UtcNow,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, testPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "User");
                }
            }
        }
    }
}

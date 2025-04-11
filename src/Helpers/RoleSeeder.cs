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

            // 1. Ensure "Admin" role exists
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // 2. Seed a test admin user (OPTIONAL)
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
                    DatumRegistracije = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(user, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }
    }
}

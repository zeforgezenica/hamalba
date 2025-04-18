using hamalba.DataBase;
using hamalba.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace hamalba.Controllers
{
    [Authorize(Roles = "Admin")] 
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Korisnik> _userManager;
        public AdminController(ApplicationDbContext context, UserManager<Korisnik> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        [HttpPost]
        public async Task<IActionResult> ToggleVerifikacija(string id)
        {
            var user = await _context.Korisnici.FindAsync(id);
            if (user == null)
                return NotFound();

            user.Verifikovan = !user.Verifikovan;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> BanUser(string id, DateTime? banTrajanje, string? banRazlog)
        {
            var user = await _context.Korisnici.FindAsync(id);
            if (user == null)
                return NotFound();

            user.BanTrajanje = banTrajanje;
            user.BanRazlog = banRazlog;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> PromijeniLozinku(string id, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "Lozinke se ne podudaraju ili su prazne.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Korisnik nije pronađen.";
                return RedirectToAction("Index");
            }

            // Provjeri da li korisnik već ima lozinku
            var hasPassword = await _userManager.HasPasswordAsync(user);

            IdentityResult result;

            if (hasPassword)
            {
                // Generiši token i resetuj
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            }
            else
            {
                // Ako korisnik nikad nije imao lozinku (eksterna registracija npr.), dodaj je
                result = await _userManager.AddPasswordAsync(user, newPassword);
            }

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Lozinka uspješno promijenjena.";
            }
            else
            {
                TempData["ErrorMessage"] = string.Join("; ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index(string search)
        {
            var korisnici = _context.Korisnici.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                korisnici = korisnici.Where(k =>
                    k.Ime.Contains(search) ||
                    k.Prezime.Contains(search) ||
                    k.Email.Contains(search));
            }

            var lista = await korisnici.OrderBy(k => k.DatumRegistracije).ToListAsync();
            return View(lista);
        }

        [HttpPost]
        public async Task<IActionResult> Arhiviraj(string id)
        {
            var user = await _context.Korisnici.FindAsync(id);
            if (user == null)
                return NotFound();

            if (user.Arhiviran == 0) // samo ako već nije arhiviran
            {
                user.Arhiviran = 1;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }



    }
}

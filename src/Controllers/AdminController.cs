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
        public async Task<IActionResult> BanUser(string id, DateTime? banTrajanje)
        {
            var user = await _context.Korisnici.FindAsync(id);
            if (user == null)
                return NotFound();

            user.BanTrajanje = banTrajanje;
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

            // Uklanjamo staru lozinku i postavljamo novu
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

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

    }
}

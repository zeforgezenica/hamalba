using hamalba.DataBase;
using hamalba.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hamalba.Controllers
{
    [Authorize(Roles = "Admin")] 
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
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

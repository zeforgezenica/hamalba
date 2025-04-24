using hamalba.Models;
using hamalba.ViewModels;
using hamalba.DataBase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace hamalba.Controllers
{
    [Authorize]
    public class ProfilController : Controller
    {
        private readonly UserManager<Korisnik> _userManager;
        private readonly ApplicationDbContext _context;

        public ProfilController(UserManager<Korisnik> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var korisnik = await _userManager.GetUserAsync(User);

            if (korisnik == null)
                return NotFound();

            // Učitavanje mojih oglasa
            var mojiOglasi = await _context.Oglasi
                .Where(o => o.UserId == korisnik.Id && !o.Arhiviran)
                .ToListAsync();

            // Učitavanje prijavljenih oglasa sa statusom
            var prijavljeniOglasi = await _context.KorisnikOglasi
                .Where(ko => ko.UserId == korisnik.Id)
                .Include(ko => ko.Oglas) // Uključujemo podatke o oglasima
                .ToListAsync();

            var viewModel = new ProfilViewModel
            {
                Korisnik = korisnik,
                MojiOglasi = mojiOglasi,
                PrijavljeniOglasi = prijavljeniOglasi // Ovdje šaljemo cijeli objekat KorisnikOglas
            };

            return View(viewModel);
        }


    }
}

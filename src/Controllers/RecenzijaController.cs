using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using hamalba.Models;
using System;
using hamalba.DataBase;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace hamalba.Controllers
{
    public class RecenzijeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RecenzijeController> _logger;
        private readonly UserManager<Korisnik> _userManager;

        public RecenzijeController(ApplicationDbContext context, ILogger<RecenzijeController> logger, UserManager<Korisnik> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        // GET: Recenzije/Create
        [HttpGet]
        public async Task<IActionResult> Create(int oglasId, string primaocId, RecenzijaTip tip)
        {
            // Provjera da li je oglas obavljen
            var oglas = await _context.Oglasi.FindAsync(oglasId);
            if (oglas == null || oglas.Status != OglasStatus.Obavljen)
            {
                TempData["Error"] = "Recenziju možete dati samo za obavljene poslove.";
                return RedirectToAction("Detalji", "Oglasi", new { id = oglasId });
            }

            // Provjera da li je korisnik već dao recenziju
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var postojecaRecenzija = await _context.Recenzije
                .AnyAsync(r => r.OglasId == oglasId && r.AutorId == user.Id && r.PrimaocId == primaocId);

            if (postojecaRecenzija)
            {
                TempData["Error"] = "Već ste dali recenziju za ovaj posao.";
                return RedirectToAction("Detalji", "Oglasi", new { id = oglasId });
            }

            // Provjera da li je korisnik poslodavac ili radnik
            var prijava = await _context.KorisnikOglasi
                .FirstOrDefaultAsync(ko => ko.OglasId == oglasId && ko.UserId == primaocId && ko.Status == 1);

            var isPoslodavac = oglas.UserId == user.Id;
            var isRadnik = prijava != null && user.Id == primaocId;

            if ((tip == RecenzijaTip.ZaRadnika && !isPoslodavac) ||
                (tip == RecenzijaTip.ZaPoslodavca && !isRadnik))
            {
                TempData["Error"] = "Nemate pravo dati recenziju.";
                return RedirectToAction("Detalji", "Oglasi", new { id = oglasId });
            }

            var primaoc = await _userManager.FindByIdAsync(primaocId);
            ViewBag.PrimaocIme = $"{primaoc.Ime} {primaoc.Prezime}";
            ViewBag.OglasNaslov = oglas.Naslov;

            var model = new RecenzijaViewModel
            {
                OglasId = oglasId,
                PrimaocId = primaocId,
                Tip = tip
            };

            return View(model);
        }

        // POST: Recenzije/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RecenzijaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var oglasDetails = await _context.Oglasi.FindAsync(model.OglasId);
                var primaoc = await _userManager.FindByIdAsync(model.PrimaocId);
                ViewBag.PrimaocIme = $"{primaoc.Ime} {primaoc.Prezime}";
                ViewBag.OglasNaslov = oglasDetails.Naslov;
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // Provjera da li je oglas obavljen
            var oglasEntity = await _context.Oglasi.FindAsync(model.OglasId);
            if (oglasEntity == null || oglasEntity.Status != OglasStatus.Obavljen)
            {
                TempData["Error"] = "Recenziju možete dati samo za obavljene poslove.";
                return RedirectToAction("Detalji", "Oglasi", new { id = model.OglasId });
            }

            // Provjera da li je već dao recenziju
            var postojecaRecenzija = await _context.Recenzije
                .AnyAsync(r => r.OglasId == model.OglasId && r.AutorId == user.Id && r.PrimaocId == model.PrimaocId);

            if (postojecaRecenzija)
            {
                TempData["Error"] = "Već ste dali recenziju za ovaj posao.";
                return RedirectToAction("Detalji", "Oglasi", new { id = model.OglasId });
            }

            var recenzija = new Recenzija
            {
                OglasId = model.OglasId,
                AutorId = user.Id,
                PrimaocId = model.PrimaocId,
                Ocjena = model.Ocjena,
                Komentar = model.Komentar,
                DatumKreiranja = DateTime.Now,
                Tip = model.Tip
            };

            _context.Recenzije.Add(recenzija);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Hvala na vašoj recenziji!";
            return RedirectToAction("Detalji", "Oglasi", new { id = model.OglasId });
        }

        // GET: Recenzije/KorisnikRecenzije
        [HttpGet]
        public async Task<IActionResult> KorisnikRecenzije(string userId = null)
        {
            string korisnikId;
            if (string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Challenge();
                }
                korisnikId = user.Id;
            }
            else
            {
                korisnikId = userId;
            }

            var korisnik = await _userManager.FindByIdAsync(korisnikId);
            if (korisnik == null)
            {
                return NotFound();
            }

            var recenzije = await _context.Recenzije
                .Where(r => r.PrimaocId == korisnikId)
                .Include(r => r.Autor)
                .Include(r => r.Oglas)
                .OrderByDescending(r => r.DatumKreiranja)
                .ToListAsync();

            var prosjecnaOcjena = recenzije.Any() ? recenzije.Average(r => r.Ocjena) : 0;

            ViewBag.Korisnik = korisnik;
            ViewBag.ProsjecnaOcjena = prosjecnaOcjena;
            ViewBag.BrojRecenzija = recenzije.Count;

            return View(recenzije);
        }
    }
}
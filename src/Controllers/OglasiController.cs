// Controllers/OglasiController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using hamalba.Models;
using System;
using hamalba.DataBase;
using Microsoft.EntityFrameworkCore;
namespace hamalba.Controllers
{
    public class OglasiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OglasiController> _logger;
        private readonly UserManager<Korisnik> _userManager;

        public OglasiController(ApplicationDbContext context, ILogger<OglasiController> logger, UserManager<Korisnik> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult CreateOglas()
        {
            return View(new OglasViewModel());
        }
        //Prikaz svih oglasa koji su objavljeni
        [HttpGet]
        public async Task<IActionResult> SviOglasi()
        {
            _logger.LogInformation("Fetching sve oglasi from database");

            try
            {
                var oglasi = await _context.Oglasi
                    .Include(o => o.User)
                    .ToListAsync();

                var user = await _userManager.GetUserAsync(User);

                var prijavljeniOglasi = new List<int>();

                if (user != null)
                {
                    prijavljeniOglasi = await _context.KorisnikOglasi
                        .Where(p => p.UserId == user.Id)
                        .Select(p => p.OglasId)
                        .ToListAsync();
                }

                ViewBag.PrijavljeniOglasi = prijavljeniOglasi;

                return View(oglasi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching oglasi");
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
            }
        }


        //Kontroler za prijavu na neki oglas

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PrijaviSe(int oglasId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            //Onemogucavanje prijave kreatoru oglasa
            var oglas = await _context.Oglasi.FindAsync(oglasId);
            if (oglas == null)
            {
                return NotFound();
            }

            if (oglas.UserId == user.Id)
            {
                TempData["Message"] = "Ne možete se prijaviti na oglas koji ste vi objavili.";
                return RedirectToAction("SviOglasi");
            }

            var prijava = new KorisnikOglas
            {
                UserId = user.Id,
                OglasId = oglasId
            };

            _context.KorisnikOglasi.Add(prijava);
            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = "Uspješno ste se prijavili na oglas!";
            TempData["ToastType"] = "success";
            return RedirectToAction("SviOglasi");
        }

        //Single View oglasa
        [HttpGet]
        public async Task<IActionResult> Detalji(int id)
        {
            try
            {
                var oglas = await _context.Oglasi
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.OglasId == id);

                if (oglas == null)
                {
                    return NotFound();
                }

                return View(oglas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška prilikom dohvaćanja detalja oglasa");
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
            }
        }
        //Pregled prijavljenih kandidata
        [HttpGet]
        public async Task<IActionResult> PregledKandidata(int id)
        {
            var oglas = await _context.Oglasi
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OglasId == id);

            if (oglas == null)
            {
                return NotFound();
            }

            var prijave = await _context.KorisnikOglasi
                .Where(ko => ko.OglasId == id)
                .Include(ko => ko.User)
                .ToListAsync();

            ViewBag.OglasNaslov = oglas.Naslov;
            ViewBag.OglasId = oglas.OglasId;

            return View(prijave);
        }

        //Prihvatanje kandidata za posao
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PrihvatiKandidata(int oglasId, string kandidatId)
        {
            var oglas = await _context.Oglasi.FirstOrDefaultAsync(o => o.OglasId == oglasId);
            var currentUser = await _userManager.GetUserAsync(User);

            if (oglas == null || currentUser == null || oglas.UserId != currentUser.Id)
            {
                return Forbid();
            }

            var prijava = await _context.KorisnikOglasi
                .FirstOrDefaultAsync(p => p.OglasId == oglasId && p.UserId == kandidatId);

            if (prijava == null)
            {
                return NotFound();
            }

            prijava.Status = 1;
            await _context.SaveChangesAsync();

            TempData["Message"] = "Kandidat je prihvaćen.";
            return RedirectToAction("PregledKandidata", new { id = oglasId });
        }
        
        //Odbijanje kandidata za posao
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OdbijKandidata(int oglasId, string kandidatId)
        {
            var oglas = await _context.Oglasi.FirstOrDefaultAsync(o => o.OglasId == oglasId);
            var currentUser = await _userManager.GetUserAsync(User);

            if (oglas == null || currentUser == null || oglas.UserId != currentUser.Id)
            {
                return Forbid();
            }

            var prijava = await _context.KorisnikOglasi
                .FirstOrDefaultAsync(p => p.OglasId == oglasId && p.UserId == kandidatId);

            if (prijava == null)
            {
                return NotFound();
            }

            prijava.Status = 0;
            await _context.SaveChangesAsync();

            TempData["Message"] = "Kandidat je odbijen.";
            return RedirectToAction("PregledKandidata", new { id = oglasId });
        }
        //Ponistavanje odluke za odabir kandidata/undo dugme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PonistiOdluku(int oglasId, string kandidatId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var oglas = await _context.Oglasi.FindAsync(oglasId);
            if (oglas == null || oglas.UserId != user.Id) return Forbid();

            var prijava = await _context.KorisnikOglasi
                .FirstOrDefaultAsync(ko => ko.OglasId == oglasId && ko.UserId == kandidatId);

            if (prijava == null) return NotFound();

            prijava.Status = -1;
            await _context.SaveChangesAsync();

            return RedirectToAction("PregledKandidata", new { id = oglasId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOglas(OglasViewModel viewModel)
        {
            _logger.LogInformation("CreateOglas POST started");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model invalid: {@Errors}", ModelState);
                return View(viewModel);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogWarning("Unauthorized access attempt");
                    return Challenge();
                }

                var oglas = new Oglas
                {
                    Naslov = viewModel.Naslov,
                    Opis = viewModel.Opis,
                    Rok = viewModel.Rok,
                    Kontakt = viewModel.Kontakt,
                    Cijena = viewModel.Cijena,
                    Lokacija = viewModel.Lokacija,
                    Status = viewModel.Status,
                    Datum = DateTime.Now,
                    UserId = user.Id,
                    User = user
                };

                _context.Oglasi.Add(oglas);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Oglas created successfully. ID: {OglasId}", oglas.OglasId);
                return RedirectToAction("Index", "Home");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error");
                ModelState.AddModelError("", "Error saving to database. Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error");
                ModelState.AddModelError("", "An unexpected error occurred.");
            }

            return View(viewModel);
        }
    }
}
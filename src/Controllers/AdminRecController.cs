using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hamalba.DataBase;
using hamalba.Models;

namespace hamalba.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminRecController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminRecController(ApplicationDbContext context)
        {
            _context = context;
        }

        // PRIKAZ AKTIVNIH RECENZIJA
        public async Task<IActionResult> Index()
        {
            var recenzije = await _context.Recenzije
                .Include(r => r.Oglas)
                .Include(r => r.Autor)
                .Include(r => r.Primaoc)
                .Where(r => !r.Arhivirana)
                .OrderByDescending(r => r.DatumKreiranja)
                .ToListAsync();

            return View(recenzije);
        }

        // PRIKAZ ARHIVIRANIH RECENZIJA
        public async Task<IActionResult> Arhivirane()
        {
            var recenzije = await _context.Recenzije
                .Include(r => r.Oglas)
                .Include(r => r.Autor)
                .Include(r => r.Primaoc)
                .Where(r => r.Arhivirana)
                .OrderByDescending(r => r.DatumKreiranja)
                .ToListAsync();

            return View(recenzije);
        }

        // CREATE GET
        public IActionResult Create()
        {
            return View();
        }

        // CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Recenzija recenzija)
        {
            if (ModelState.IsValid)
            {
                recenzija.DatumKreiranja = DateTime.UtcNow;
                _context.Recenzije.Add(recenzija);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(recenzija);
        }

        // EDIT GET
        public async Task<IActionResult> Edit(int id)
        {
            var recenzija = await _context.Recenzije.FindAsync(id);
            if (recenzija == null)
                return NotFound();

            return View(recenzija);
        }

        // EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Recenzija recenzija)
        {
            if (id != recenzija.RecenzijaId)
                return BadRequest();

            if (ModelState.IsValid)
            {
                _context.Update(recenzija);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(recenzija);
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var rec = await _context.Recenzije.FindAsync(id);
            if (rec == null) return NotFound();
            return Json(rec);
        }

        // ARCHIVE (SOFT DELETE)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int id)
        {
            var recenzija = await _context.Recenzije.FindAsync(id);
            if (recenzija == null)
                return NotFound();

            recenzija.Arhivirana = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> PretraziKorisnike(string term)
        {
            var korisnici = await _context.Users
                .Where(k => k.Ime.Contains(term) || k.Prezime.Contains(term) || k.Email.Contains(term))
                .Select(k => new
                {
                    label = $"{k.Ime} {k.Prezime} ({k.Email})",
                    value = k.Id
                })
                .Take(10)
                .ToListAsync();

            return Json(korisnici);
        }

        [HttpGet]
        public async Task<IActionResult> PretraziOglase(string term)
        {
            var oglasi = await _context.Oglasi
                .Where(o => o.Naslov.Contains(term))
                .Select(o => new
                {
                    label = $"{o.Naslov} ({o.OglasId})",
                    value = o.OglasId
                })
                .Take(10)
                .ToListAsync();

            return Json(oglasi);
        }

    }
}

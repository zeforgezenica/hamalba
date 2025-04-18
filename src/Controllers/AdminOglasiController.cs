using hamalba.DataBase;
using hamalba.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace hamalba.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminOglasiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminOglasiController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchIme, string searchEmail, DateTime? startDate, DateTime? endDate, int page = 1, int pageSize = 10)
        {
            var oglasiQuery = _context.Oglasi
                .Include(o => o.User)
                .OrderByDescending(o => o.Datum)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchIme))
            {
                oglasiQuery = oglasiQuery.Where(o => o.User != null && o.User.Ime.Contains(searchIme));
            }

            if (!string.IsNullOrWhiteSpace(searchEmail))
            {
                oglasiQuery = oglasiQuery.Where(o => o.User != null && o.User.Email.Contains(searchEmail));
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                oglasiQuery = oglasiQuery.Where(o => o.Datum.Date >= startDate.Value.Date && o.Datum.Date <= endDate.Value.Date);
            }
            else if (startDate.HasValue)
            {
                oglasiQuery = oglasiQuery.Where(o => o.Datum.Date == startDate.Value.Date);
            }

            var totalOglasi = await oglasiQuery.CountAsync();
            var oglasi = await oglasiQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalOglasi / pageSize);
            ViewBag.SearchIme = searchIme;
            ViewBag.SearchEmail = searchEmail;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View("~/Views/Admin/OglasiIndex.cshtml", oglasi);
        }



        [HttpPost]
        public async Task<IActionResult> ArchiveOglas(int id)
        {
            var oglas = await _context.Oglasi.FindAsync(id);
            if (oglas == null)
                return NotFound();

            oglas.Arhiviran = true;  // Oznaka da je oglas arhiviran
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Oglas je uspješno arhiviran.";
            return RedirectToAction("Index");
        }



        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var oglas = await _context.Oglasi.Include(o => o.User).FirstOrDefaultAsync(o => o.OglasId == id);
            if (oglas == null)
                return NotFound();

            var viewModel = new OglasViewModel
            {
                Naslov = oglas.Naslov,
                Opis = oglas.Opis,
                Rok = oglas.Rok,
                Kontakt = oglas.Kontakt,
                Cijena = oglas.Cijena,
                Lokacija = oglas.Lokacija,
                Status = oglas.Status
            };

            ViewBag.OglasId = id;
            return View("~/Views/Admin/OglasiEdit.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OglasViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.OglasId = id;
                return View("~/Views/Admin/OglasiEdit.cshtml", viewModel);
            }

            var oglas = await _context.Oglasi.FindAsync(id);
            if (oglas == null)
                return NotFound();

            oglas.Naslov = viewModel.Naslov;
            oglas.Opis = viewModel.Opis;
            oglas.Rok = viewModel.Rok;
            oglas.Kontakt = viewModel.Kontakt;
            oglas.Cijena = viewModel.Cijena;
            oglas.Lokacija = viewModel.Lokacija;
            oglas.Status = viewModel.Status;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Izmjene su sačuvane.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var oglas = await _context.Oglasi.FindAsync(id);
            if (oglas == null)
                return NotFound();

            if (oglas.Status == OglasStatus.Zavrsen)
            {
                TempData["ErrorMessage"] = "Ne možete obrisati oglas koji je već završen.";
                return RedirectToAction("Index");
            }
            _context.Oglasi.Remove(oglas);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Oglas je uspješno obrisan.";
            return RedirectToAction("Index");
        }

    }
}

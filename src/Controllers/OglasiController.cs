﻿// Controllers/OglasiController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using hamalba.Models;
using System;
using hamalba.DataBase;
using Microsoft.EntityFrameworkCore;
using System.Linq;

using hamalba.ViewModels;
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
                var currentDateTime = DateTime.Now;

                var oglasi = await _context.Oglasi
                 .Include(o => o.User)
                 .Where(o => o.Status == OglasStatus.Aktivan || o.Status == OglasStatus.InProces || o.Status == OglasStatus.Obavljen)
                 .ToListAsync();


                var user = await _userManager.GetUserAsync(User);

                var prijavljeniOglasi = new List<int>();
                var prihvaceniOglasiZaRecenziju = new Dictionary<int, bool>();

                if (user != null)
                {
                    prijavljeniOglasi = await _context.KorisnikOglasi
                        .Where(p => p.UserId == user.Id)
                        .Select(p => p.OglasId)
                        .ToListAsync();

                    // Pronađi oglase gde je trenutni korisnik prihvaćeni radnik i gdje je oglas obavljen
                    var prihvaceniOglasi = await _context.KorisnikOglasi
                        .Where(p => p.UserId == user.Id && p.Status == 1)
                        .Select(p => p.OglasId)
                        .ToListAsync();

                    // Dohvati sve oglase koji su obavljeni
                    var obavljeniOglasi = oglasi
                        .Where(o => o.Status == OglasStatus.Obavljen && prihvaceniOglasi.Contains(o.OglasId))
                        .ToList();

                    // Za svaki obavljeni oglas provjeri je li već data recenzija
                    foreach (var oglas in obavljeniOglasi)
                    {
                        var daRecenzija = await _context.Recenzije
                            .AnyAsync(r => r.OglasId == oglas.OglasId &&
                                           r.AutorId == user.Id &&
                                           r.PrimaocId == oglas.UserId &&
                                           r.Tip == RecenzijaTip.ZaPoslodavca);

                        prihvaceniOglasiZaRecenziju.Add(oglas.OglasId, daRecenzija);
                    }
                }

                ViewBag.PrijavljeniOglasi = prijavljeniOglasi;
                ViewBag.PrihvaceniOglasiZaRecenziju = prihvaceniOglasiZaRecenziju;

                return View(oglasi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching oglasi");
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
            }
        }
        //Filtracija
        [HttpGet]
        public IActionResult Index()
        {
            var model = new OglasFilterViewModel
            {
                Rezultati = _context.Oglasi.ToList()
            };

            return View(model); 
        }


        [HttpPost]
        public IActionResult Index(OglasFilterViewModel filter, string akcija)
        {
            var query = _context.Oglasi.AsQueryable();

            if (akcija == "filtriraj")
            {
                if (!string.IsNullOrEmpty(filter.NazivPosla))
                    query = query.Where(o => o.Naslov.Contains(filter.NazivPosla));

                if (!string.IsNullOrEmpty(filter.Lokacija))
                    query = query.Where(o => o.Lokacija.Contains(filter.Lokacija));

                if (filter.MinimalnaCijena.HasValue)
                    query = query.Where(o => o.Cijena >= filter.MinimalnaCijena);

                if (filter.MaksimalnaCijena.HasValue)
                    query = query.Where(o => o.Cijena <= filter.MaksimalnaCijena);
            }

            if (akcija == "sortiraj")
            {
                switch (filter.SortOpcija)
                {
                    case SortOpcije.CijenaUzlazno:
                        query = query.OrderBy(o => o.Cijena);
                        break;
                    case SortOpcije.CijenaSilazno:
                        query = query.OrderByDescending(o => o.Cijena);
                        break;
                    case SortOpcije.NazivPoslaAZ:
                        query = query.OrderBy(o => o.Naslov);
                        break;
                    case SortOpcije.NazivPoslaZA:
                        query = query.OrderByDescending(o => o.Naslov);
                        break;
                }
            }

            filter.Rezultati = query.ToList();

            return View(filter); // ista stranica: sviOglasi 
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

            var detaljniLog = $"[DETAIL] [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] IP: {HttpContext.Connection.RemoteIpAddress} | Email: {user.Email} | Prijavio se na oglas: \"{oglas.Naslov}\" | Opis: \"{oglas.Opis}\" | Lokacija: \"{oglas.Lokacija}\" | Rok: {oglas.Rok:yyyy-MM-dd} | Cijena: {oglas.Cijena}";
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", $"activity-log-{DateTime.Now:yyyy-MM-dd}.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(logPath));
            await System.IO.File.AppendAllTextAsync(logPath, detaljniLog + Environment.NewLine);

            return RedirectToAction("SviOglasi");
        }
        // Detalji oglasa  
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

                var currentUser = await _userManager.GetUserAsync(User);
                var prihvaceniRadnik = await _context.KorisnikOglasi
                    .Include(ko => ko.User)
                    .FirstOrDefaultAsync(ko => ko.OglasId == id && ko.Status == 1);

                ViewBag.PrihvaceniRadnik = prihvaceniRadnik;

                // Check if employer has already left a review for the worker
                bool poslodavacDaoRecenziju = false;
                if (currentUser != null && prihvaceniRadnik != null &&
                    oglas.Status == OglasStatus.Obavljen && oglas.UserId == currentUser.Id)
                {
                    poslodavacDaoRecenziju = await _context.Recenzije
                        .AnyAsync(r => r.OglasId == id &&
                                  r.AutorId == currentUser.Id &&
                                  r.PrimaocId == prihvaceniRadnik.UserId &&
                                  r.Tip == RecenzijaTip.ZaRadnika);
                }

                ViewBag.PoslodavacDaoRecenziju = poslodavacDaoRecenziju;

                // DODATI OVAJ KOD: Provjera je li trenutni korisnik radnik i je li već dao recenziju poslodavcu
                bool radnikDaoRecenziju = false;
                if (currentUser != null && prihvaceniRadnik != null &&
                    oglas.Status == OglasStatus.Obavljen && currentUser.Id == prihvaceniRadnik.UserId)
                {
                    radnikDaoRecenziju = await _context.Recenzije
                        .AnyAsync(r => r.OglasId == id &&
                                  r.AutorId == currentUser.Id &&
                                  r.PrimaocId == oglas.UserId &&
                                  r.Tip == RecenzijaTip.ZaPoslodavca);
                }

                ViewBag.RadnikDaoRecenziju = radnikDaoRecenziju;

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
            ViewBag.OglasStatus = oglas.Status;

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

            // Update oglasa u InProces
            oglas.Status = OglasStatus.InProces;

            await _context.SaveChangesAsync();

            TempData["Message"] = "Kandidat je prihvaćen.";

            var detaljniLog = $"[DETAIL] [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] IP: {HttpContext.Connection.RemoteIpAddress} | Email: {currentUser.Email} | Prihvatio kandidata: {kandidatId} | Oglas: \"{oglas.Naslov}\" | Opis: \"{oglas.Opis}\" | Lokacija: \"{oglas.Lokacija}\" | Rok: {oglas.Rok:yyyy-MM-dd} | Cijena: {oglas.Cijena}";
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", $"activity-log-{DateTime.Now:yyyy-MM-dd}.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(logPath));
            await System.IO.File.AppendAllTextAsync(logPath, detaljniLog + Environment.NewLine);

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

            var detaljniLog = $"[DETAIL] [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] IP: {HttpContext.Connection.RemoteIpAddress} | Email: {currentUser.Email} | Odbijen kandidat: {kandidatId} | Objavio oglas: \"{oglas.Naslov}\" | Opis: \"{oglas.Opis}\" | Lokacija: \"{oglas.Lokacija}\" | Rok: {oglas.Rok:yyyy-MM-dd} | Cijena: {oglas.Cijena}";
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", $"activity-log-{DateTime.Now:yyyy-MM-dd}.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(logPath));
            await System.IO.File.AppendAllTextAsync(logPath, detaljniLog + Environment.NewLine);

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
        public async Task<IActionResult> CreateOglas(OglasViewModel viewModel, bool? PublishNow, bool? PublishLater)
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
                    Datum = DateTime.Now,
                    UserId = user.Id,
                    User = user
                };

                if (PublishLater == true)
                {
                    
                    oglas.DatumObjave = viewModel.DatumObjave;
                    oglas.Status = OglasStatus.CekaNaObjavu;
                }
                else
                {
                    
                    oglas.DatumObjave = DateTime.Now;
                    oglas.Status = OglasStatus.Aktivan;
                }

                _context.Oglasi.Add(oglas);
                await _context.SaveChangesAsync();

                

                _logger.LogInformation("Oglas created successfully. ID: {OglasId}, Status: {Status}", oglas.OglasId, oglas.Status);

                if (oglas.Status == OglasStatus.CekaNaObjavu)
                {
                    TempData["Message"] = $"Oglas kreiran i bit će objavljen {oglas.DatumObjave.ToString("dd/MM/yyyy HH:mm")}";
                }
                else
                {
                    TempData["Message"] = "Oglas uspješno kreiran i objavljen!";
                }

                var detaljniLog = $"[DETAIL] [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] IP: {HttpContext.Connection.RemoteIpAddress} | Email: {user.Email} | Objavio oglas: \"{oglas.Naslov}\" | Opis: \"{oglas.Opis}\" | Lokacija: \"{oglas.Lokacija}\" | Rok: {oglas.Rok:yyyy-MM-dd} | Cijena: {oglas.Cijena}";
                var logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", $"activity-log-{DateTime.Now:yyyy-MM-dd}.txt");
                Directory.CreateDirectory(Path.GetDirectoryName(logPath));
                await System.IO.File.AppendAllTextAsync(logPath, detaljniLog + Environment.NewLine);

                return RedirectToAction("SviOglasi", "Oglasi");
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
        // GET: Edit 
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var oglas = await _context.Oglasi.FindAsync(id);
            if (oglas == null)
            {
                return NotFound();
            }

            if (oglas.UserId != user.Id)
            {
                return Forbid();
            }

            if (oglas.Status == OglasStatus.Obavljen || oglas.Status == OglasStatus.InProces)
            {
                TempData["Error"] = "Nije moguće uređivati poslove koji su označeni kao obavljeni ili su u procesu izvršavanja.";
                return RedirectToAction("Detalji", new { id = oglas.OglasId });
            }

            var viewModel = new OglasViewModel
            {
                Naslov = oglas.Naslov,
                Opis = oglas.Opis,
                Rok = oglas.Rok,
                Kontakt = oglas.Kontakt,
                Cijena = oglas.Cijena,
                Lokacija = oglas.Lokacija,
                DatumObjave = oglas.DatumObjave,
                Status = oglas.Status
            };

            ViewBag.OglasId = id;
            ViewBag.OglasStatus = oglas.Status;
            ViewBag.TrenutniDatumObjave = oglas.DatumObjave;
            ViewBag.IsCekaNaObjavu = oglas.Status == OglasStatus.CekaNaObjavu;

            return View(viewModel);
        }

        // POST: Edit 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OglasViewModel viewModel, bool? PublishNow, bool? PublishLater)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.OglasId = id;
                return View(viewModel);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var oglas = await _context.Oglasi.FindAsync(id);
            if (oglas == null)
            {
                return NotFound();
            }

            if (oglas.UserId != user.Id)
            {
                return Forbid();
            }

            if (oglas.Status == OglasStatus.Obavljen || oglas.Status == OglasStatus.InProces)
            {
                TempData["Error"] = "Nije moguće uređivati poslove koji su označeni kao obavljeni ili su u procesu izvršavanja.";
                return RedirectToAction("Detalji", new { id = oglas.OglasId });
            }

            oglas.Naslov = viewModel.Naslov;
            oglas.Opis = viewModel.Opis;
            oglas.Rok = viewModel.Rok;
            oglas.Kontakt = viewModel.Kontakt;
            oglas.Cijena = viewModel.Cijena;
            oglas.Lokacija = viewModel.Lokacija;

            if (PublishLater == true)
            {
                oglas.DatumObjave = viewModel.DatumObjave;
                oglas.Status = OglasStatus.CekaNaObjavu;
            }
            else if (PublishNow == true && oglas.Status == OglasStatus.CekaNaObjavu)
            {
                oglas.DatumObjave = DateTime.Now;
                oglas.Status = OglasStatus.Aktivan;
            }

            try
            {
                await _context.SaveChangesAsync();

                if (oglas.Status == OglasStatus.CekaNaObjavu)
                {
                    TempData["Message"] = $"Oglas ažuriran i bit će objavljen {oglas.DatumObjave.ToString("dd/MM/yyyy HH:mm")}";
                }
                else
                {
                    TempData["Message"] = "Oglas uspješno ažuriran!";
                }

                var detaljniLog = $"[DETAIL] [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] IP: {HttpContext.Connection.RemoteIpAddress} | Email: {user.Email} | Uredio oglas: \"{oglas.Naslov}\" | Opis: \"{oglas.Opis}\" | Lokacija: \"{oglas.Lokacija}\" | Rok: {oglas.Rok:yyyy-MM-dd} | Cijena: {oglas.Cijena}";
                var logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", $"activity-log-{DateTime.Now:yyyy-MM-dd}.txt");
                Directory.CreateDirectory(Path.GetDirectoryName(logPath));
                await System.IO.File.AppendAllTextAsync(logPath, detaljniLog + Environment.NewLine);

                return RedirectToAction("Index", "Profil");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška prilikom ažuriranja oglasa");
                ModelState.AddModelError("", "Došlo je do greške prilikom ažuriranja oglasa.");
                ViewBag.OglasId = id;
                return View(viewModel);
            }
        }

        // GET: Stranica za potvrdu brisanja
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var oglas = await _context.Oglasi
                .FirstOrDefaultAsync(o => o.OglasId == id);

            if (oglas == null)
            {
                return NotFound();
            }

            
            if (oglas.UserId != user.Id)
            {
                return Forbid();
            }
            if (oglas.Status == OglasStatus.InProces)
            {
                TempData["Error"] = "Nije moguće brisati poslove koji su u procesu izvršavanja.";
                return RedirectToAction("Detalji", new { id = oglas.OglasId });
            }
            return View(oglas);
        }

        // POST: Akcija za brisanje
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var oglas = await _context.Oglasi.FindAsync(id);
            if (oglas == null)
            {
                return NotFound();
            }

            if (oglas.UserId != user.Id)
            {
                return Forbid();
            }
            if (oglas.Status == OglasStatus.InProces)
            {
                TempData["Error"] = "Nije moguće brisati poslove koji su u procesu izvršavanja.";
                return RedirectToAction("Detalji", new { id = oglas.OglasId });
            }

            //Meko brisanje
            oglas.Status = OglasStatus.Otkazan;
            oglas.Arhiviran = true;

            await _context.SaveChangesAsync();

            var detaljniLog = $"[DETAIL] [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] IP: {HttpContext.Connection.RemoteIpAddress} | Email: {user.Email} | Izbrisao oglas: \"{oglas.Naslov}\" | Opis: \"{oglas.Opis}\" | Lokacija: \"{oglas.Lokacija}\" | Rok: {oglas.Rok:yyyy-MM-dd} | Cijena: {oglas.Cijena}";
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", $"activity-log-{DateTime.Now:yyyy-MM-dd}.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(logPath));
            await System.IO.File.AppendAllTextAsync(logPath, detaljniLog + Environment.NewLine);

            TempData["Message"] = "Oglas uspješno obrisan!";
            return RedirectToAction("Index", "Profil");
        }
        // Oglas obavljen
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OznaciKaoObavljen(int oglasId)
        {
            try
            {
                var oglas = await _context.Oglasi.FirstOrDefaultAsync(o => o.OglasId == oglasId);
                var currentUser = await _userManager.GetUserAsync(User);

                if (oglas == null || currentUser == null || oglas.UserId != currentUser.Id)
                {
                    return Forbid();
                }

                if (oglas.Status != OglasStatus.InProces)
                {
                    TempData["Error"] = "Samo poslovi koji su u procesu mogu biti označeni kao obavljeni.";
                    return RedirectToAction("Detalji", new { id = oglasId });
                }

                // Pronađi prihvaćenog radnika za ovaj oglas
                var prihvaceniRadnik = await _context.KorisnikOglasi
                    .FirstOrDefaultAsync(ko => ko.OglasId == oglasId && ko.Status == 1);

                if (prihvaceniRadnik == null)
                {
                    TempData["Error"] = "Nije pronađen prihvaćeni radnik za ovaj oglas.";
                    return RedirectToAction("Detalji", new { id = oglasId });
                }

                // Promijeni status oglasa na "Obavljen"
                oglas.Status = OglasStatus.Obavljen;
                await _context.SaveChangesAsync();

                TempData["Message"] = "Posao je uspješno označen kao obavljen.";

                // Preusmjeri poslodavca na stranicu za davanje recenzije radniku
                return RedirectToAction("Detalji", new { id = oglasId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška prilikom označavanja oglasa kao obavljenog");
                TempData["Error"] = "Došlo je do greške prilikom označavanja oglasa kao obavljenog.";
                return RedirectToAction("Detalji", new { id = oglasId });
            }
        }
        [HttpGet]
        public async Task<IActionResult> DetaljiWithRecenzije(int id)
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

                var currentUser = await _userManager.GetUserAsync(User);
                var prihvaceniRadnik = await _context.KorisnikOglasi
                    .Include(ko => ko.User)
                    .FirstOrDefaultAsync(ko => ko.OglasId == id && ko.Status == 1);

                ViewBag.PrihvaceniRadnik = prihvaceniRadnik;

                // Provjera da li je trenutni korisnik već dao recenziju
                bool daoPoslodavacRecenziju = false;
                bool daoRadnikRecenziju = false;

                if (currentUser != null)
                {
                    if (oglas.UserId == currentUser.Id && prihvaceniRadnik != null)
                    {
                        daoPoslodavacRecenziju = await _context.Recenzije
                            .AnyAsync(r => r.OglasId == id && r.AutorId == currentUser.Id && r.PrimaocId == prihvaceniRadnik.UserId);
                    }
                    else if (prihvaceniRadnik != null && prihvaceniRadnik.UserId == currentUser.Id)
                    {
                        daoRadnikRecenziju = await _context.Recenzije
                            .AnyAsync(r => r.OglasId == id && r.AutorId == currentUser.Id && r.PrimaocId == oglas.UserId);
                    }
                }

                ViewBag.DaoPoslodavacRecenziju = daoPoslodavacRecenziju;
                ViewBag.DaoRadnikRecenziju = daoRadnikRecenziju;
                ViewBag.TrenutniKorisnik = currentUser;

                return View(oglas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška prilikom dohvaćanja detalja oglasa");
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
            }
        }
    }
}
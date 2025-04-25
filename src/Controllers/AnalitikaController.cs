using Microsoft.AspNetCore.Mvc;
using hamalba.DataBase;
using Microsoft.EntityFrameworkCore;
using hamalba.Models;

public class AnalitikaController : Controller
{
    private readonly ApplicationDbContext _context;

    public AnalitikaController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var ukupnoOglasa = await _context.Oglasi.CountAsync();

        var topGradoviList = await _context.Oglasi
            .GroupBy(o => o.Lokacija)
            .Select(g => new { Lokacija = g.Key ?? "Nepoznat", Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .Take(5)
            .ToListAsync();
        var gradoviDict = topGradoviList.ToDictionary(g => g.Lokacija, g => g.Count);

        var korisniciOglasi = await _context.Oglasi
            .GroupBy(o => o.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToListAsync();
        var korisniciDict = korisniciOglasi
            .Join(_context.Users,
                ko => ko.UserId,
                u => u.Id,
                (ko, u) => new { u.Ime, ko.Count })
            .ToDictionary(x => x.Ime, x => x.Count);

        var rawMonthlyAds = await _context.Oglasi
            .GroupBy(o => new { o.Datum.Year, o.Datum.Month })
            .Select(g => new {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Count = g.Count()
            })
            .ToListAsync();
        var monthlyAdsDict = rawMonthlyAds
            .OrderBy(x => new DateTime(x.Year, x.Month, 1))
            .ToDictionary(
                x => new DateTime(x.Year, x.Month, 1).ToString("MMMM yyyy"),
                x => x.Count
            );

        var statusi = await _context.Oglasi
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

        // Za tabelu i grafikon
        var topRadniciList = await _context.Recenzije
            .Where(r => r.Tip == RecenzijaTip.ZaRadnika)
            .GroupBy(r => r.PrimaocId)
            .Select(g => new {
                KorisnikId = g.Key,
                ProsjecnaOcjena = g.Average(r => r.Ocjena),
                BrojRecenzija = g.Count()
            })
            .Where(x => x.BrojRecenzija >= 3)
            .OrderByDescending(x => x.ProsjecnaOcjena)
            .ThenByDescending(x => x.BrojRecenzija)
            .Take(10)
            .Join(_context.Users,
                rec => rec.KorisnikId,
                user => user.Id,
                (rec, user) => new {
                    ImePrezime = user.Ime + " " + user.Prezime,
                    rec.ProsjecnaOcjena,
                    rec.BrojRecenzija
                })
            .ToListAsync();

        var topRadniciDict = topRadniciList.ToDictionary(x => x.ImePrezime, x => x.ProsjecnaOcjena);

        var topPoslodavciList = await _context.Recenzije
            .Where(r => r.Tip == RecenzijaTip.ZaPoslodavca)
            .GroupBy(r => r.PrimaocId)
            .Select(g => new {
                KorisnikId = g.Key,
                ProsjecnaOcjena = g.Average(r => r.Ocjena),
                BrojRecenzija = g.Count()
            })
            .Where(x => x.BrojRecenzija >= 3)
            .OrderByDescending(x => x.ProsjecnaOcjena)
            .ThenByDescending(x => x.BrojRecenzija)
            .Take(10)
            .Join(_context.Users,
                rec => rec.KorisnikId,
                user => user.Id,
                (rec, user) => new {
                    ImePrezime = user.Ime + " " + user.Prezime,
                    rec.ProsjecnaOcjena,
                    rec.BrojRecenzija
                })
            .ToListAsync();

        var topPoslodavciDict = topPoslodavciList.ToDictionary(x => x.ImePrezime, x => x.ProsjecnaOcjena);

        ViewData["UkupnoOglasa"] = ukupnoOglasa;
        ViewData["TopGradovi"] = gradoviDict;
        ViewData["TopKorisnici"] = korisniciDict;
        ViewData["MonthlyAds"] = monthlyAdsDict;
        ViewData["StatusiOglasa"] = statusi;

        ViewData["TopRadnici"] = topRadniciList;
        ViewData["TopRadniciDict"] = topRadniciDict;

        ViewData["TopPoslodavci"] = topPoslodavciList;
        ViewData["TopPoslodavciDict"] = topPoslodavciDict;

        return View();
    }


}
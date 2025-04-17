using Microsoft.AspNetCore.Mvc;
using hamalba.DataBase;
using Microsoft.EntityFrameworkCore;

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

        var topCijeneDict = await _context.Oglasi
    .OrderByDescending(o => o.Cijena)
    .Take(10)
    .Select(o => new { o.Naslov, o.Cijena })
    .ToDictionaryAsync(x => x.Naslov, x => x.Cijena);

        ViewData["TopCijene"] = topCijeneDict;
        ViewData["UkupnoOglasa"] = ukupnoOglasa;
        ViewData["TopGradovi"] = gradoviDict;
        ViewData["TopKorisnici"] = korisniciDict;
        ViewData["MonthlyAds"] = monthlyAdsDict;

        return View();
    }

}
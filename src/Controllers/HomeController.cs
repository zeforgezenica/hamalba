using hamalba.Controllers;
using hamalba.DataBase;
using hamalba.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace hamalba.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public IActionResult Index()
        {
            // Dohvatiti sve oglase
            var oglasi = _context.Oglasi
                .Where(o => o.Status == OglasStatus.Aktivan) // Filtriramo samo aktivne oglase
                .Take(4) // Ograničavamo broj na 4
                .ToList();

            // Slanje podataka u View
            return View(oglasi);
        }
    }
}

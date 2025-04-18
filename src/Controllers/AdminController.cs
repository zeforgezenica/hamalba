using hamalba.DataBase;
using hamalba.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

            var log = $"[DETAIL] [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] IP: {HttpContext.Connection.RemoteIpAddress} | Admin toggled verifikacija for: {user.Email} | Status: {(user.Verifikovan ? "Verifikovan" : "Ne-verifikovan")}";
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", $"activity-log-{DateTime.Now:yyyy-MM-dd}.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(logPath));
            await System.IO.File.AppendAllTextAsync(logPath, log + Environment.NewLine);

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

            var log = $"[DETAIL] [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] IP: {HttpContext.Connection.RemoteIpAddress} | Admin postavio ban korisniku: {user.Email} | Do: {banTrajanje:yyyy-MM-dd}";
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", $"activity-log-{DateTime.Now:yyyy-MM-dd}.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(logPath));
            await System.IO.File.AppendAllTextAsync(logPath, log + Environment.NewLine);

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

            if (user.Arhiviran == 0)
            {
                user.Arhiviran = 1;
                await _context.SaveChangesAsync();

                var log = $"[DETAIL] [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] IP: {HttpContext.Connection.RemoteIpAddress} | Admin arhivirao korisnika: {user.Email}";
                var logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", $"activity-log-{DateTime.Now:yyyy-MM-dd}.txt");
                Directory.CreateDirectory(Path.GetDirectoryName(logPath));
                await System.IO.File.AppendAllTextAsync(logPath, log + Environment.NewLine);
            }

            return RedirectToAction("Index");
        }

        public IActionResult ActivityLog()
        {
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", $"activity-log-{DateTime.Now:yyyy-MM-dd}.txt");

            if (!System.IO.File.Exists(logPath))
                return Content("No logs yet.");

            var content = System.IO.File.ReadAllText(logPath);
            return Content(content, "text/plain");
        }
    }
}
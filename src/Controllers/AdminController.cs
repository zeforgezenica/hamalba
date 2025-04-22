using hamalba.DataBase;
using hamalba.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace hamalba.Controllers
{
    [Authorize(Roles = "Admin")] 
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Korisnik> _userManager;
        public AdminController(ApplicationDbContext context, UserManager<Korisnik> userManager)
        {
            _context = context;
            _userManager = userManager;
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
        public async Task<IActionResult> BanUser(string id, DateTime? banTrajanje, string? banRazlog)
        {
            var user = await _context.Korisnici.FindAsync(id);
            if (user == null)
                return NotFound();

            user.BanTrajanje = banTrajanje;
            user.BanRazlog = banRazlog;
            await _context.SaveChangesAsync();

            var log = $"[DETAIL] [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] IP: {HttpContext.Connection.RemoteIpAddress} | Admin postavio ban korisniku: {user.Email} | Do: {banTrajanje:yyyy-MM-dd}";
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", $"activity-log-{DateTime.Now:yyyy-MM-dd}.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(logPath));
            await System.IO.File.AppendAllTextAsync(logPath, log + Environment.NewLine);


            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> PromijeniLozinku(string id, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "Lozinke se ne podudaraju ili su prazne.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Korisnik nije pronađen.";
                return RedirectToAction("Index");
            }

            // Uklanjamo staru lozinku i postavljamo novu
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Lozinka uspješno promijenjena.";
            }
            else
            {
                TempData["ErrorMessage"] = string.Join("; ", result.Errors.Select(e => e.Description));
            }

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

            if (user.Arhiviran == 0) // samo ako već nije arhiviran
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

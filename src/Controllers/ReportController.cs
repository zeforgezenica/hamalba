// Controllers/ReportController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using hamalba.DataBase;
using hamalba.Models;
using hamalba.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace hamalba.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Korisnik> _userManager;
        private readonly ILogger<ReportController> _logger;

        public ReportController(
            ApplicationDbContext context,
            UserManager<Korisnik> userManager,
            ILogger<ReportController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> ReportUser(string userId, int? oglasId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            if (currentUser.Id == userId)
            {
                TempData["ErrorMessage"] = "Ne možete prijaviti samog sebe.";
                return RedirectToAction("Index", "Home");
            }

            var reportedUser = await _userManager.FindByIdAsync(userId);
            if (reportedUser == null)
            {
                return NotFound();
            }

            var model = new ReportViewModel
            {
                ReportedUserId = userId,
                ReportedUserName = $"{reportedUser.Ime} {reportedUser.Prezime}",
                OglasId = oglasId
            };

            if (oglasId.HasValue)
            {
                var oglas = await _context.Oglasi.FindAsync(oglasId.Value);
                if (oglas != null)
                {
                    model.OglasNaslov = oglas.Naslov;
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportUser(ReportViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var reportedUser = await _userManager.FindByIdAsync(model.ReportedUserId);
                if (reportedUser != null)
                {
                    model.ReportedUserName = $"{reportedUser.Ime} {reportedUser.Prezime}";
                }
                return View(model);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var report = new Report
            {
                ReporterUserId = currentUser.Id,
                ReportedUserId = model.ReportedUserId,
                DatumPrijave = DateTime.Now,
                Razlog = model.Razlog,
                OglasId = model.OglasId,
                Status = 0,
                Pregledano = false
            };

            try
            {
                _context.Reports.Add(report);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Prijava je uspješno poslana. Administrator će je pregledati.";

                // Log the report
                var detaljniLog = $"[REPORT] [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] IP: {HttpContext.Connection.RemoteIpAddress} | Reporter: {currentUser.Email} | Reported: {model.ReportedUserId} | Razlog: \"{model.Razlog}\"";
                var logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", $"reports-log-{DateTime.Now:yyyy-MM-dd}.txt");
                Directory.CreateDirectory(Path.GetDirectoryName(logPath));
                await System.IO.File.AppendAllTextAsync(logPath, detaljniLog + Environment.NewLine);

                // Redirect to the appropriate page
                if (model.OglasId.HasValue)
                {
                    return RedirectToAction("Detalji", "Oglasi", new { id = model.OglasId.Value });
                }
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving report");
                ModelState.AddModelError("", "Došlo je do greške prilikom slanja prijave. Pokušajte ponovo.");

                var reportedUser = await _userManager.FindByIdAsync(model.ReportedUserId);
                if (reportedUser != null)
                {
                    model.ReportedUserName = $"{reportedUser.Ime} {reportedUser.Prezime}";
                }
                return View(model);
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string searchTerm, bool showReviewed = false)
        {
            IQueryable<Report> reportsQuery = _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .Include(r => r.RelatedOglas)
                .OrderByDescending(r => r.DatumPrijave);

            if (!showReviewed)
            {
                reportsQuery = reportsQuery.Where(r => !r.Pregledano);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                reportsQuery = reportsQuery.Where(r =>
                    r.Reporter.Ime.Contains(searchTerm) ||
                    r.Reporter.Prezime.Contains(searchTerm) ||
                    r.ReportedUser.Ime.Contains(searchTerm) ||
                    r.ReportedUser.Prezime.Contains(searchTerm) ||
                    r.Razlog.Contains(searchTerm));
            }

            var model = new ReportListViewModel
            {
                Reports = await reportsQuery.ToListAsync(),
                SearchTerm = searchTerm,
                ShowReviewed = showReviewed
            };

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int id)
        {
            var report = await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .Include(r => r.RelatedOglas)
                .FirstOrDefaultAsync(r => r.ReportId == id);

            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ProcessReport(AdminReportResponseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Details", new { id = model.ReportId });
            }

            var report = await _context.Reports.FindAsync(model.ReportId);
            if (report == null)
            {
                return NotFound();
            }

            report.Status = model.Status;
            report.KomentarAdmina = model.KomentarAdmina;
            report.Pregledano = true;
            report.DatumPregleda = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Prijava je uspješno obrađena.";
            return RedirectToAction("Index");
        }

        // Add this method to get the count of unreviewed reports for ViewComponents or other controllers
        public static async Task<int> GetUnreviewedReportCount(ApplicationDbContext context)
        {
            return await context.Reports.CountAsync(r => !r.Pregledano);
        }
    }
}
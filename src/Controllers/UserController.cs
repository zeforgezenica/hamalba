using hamalba.DataBase;
using hamalba.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace hamalba.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly UserManager<Korisnik> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(UserManager<Korisnik> userManager, ApplicationDbContext context, ILogger<UserController> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult CreateOglas()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOglas(Oglas oglas)
        {
            _logger.LogInformation("POST CreateOglas called");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid:");
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    foreach (var error in state.Errors)
                    {
                        _logger.LogWarning($"Key: {key}, Error: {error.ErrorMessage}");
                    }
                }
                return View(oglas);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogError("User is not logged in.");
                    return Unauthorized();
                }

                oglas.UserId = user.Id;
                oglas.Datum = DateTime.Now;

                _context.Oglasi.Add(oglas);
                await _context.SaveChangesAsync();

                var detaljniLog = $"[DETAIL] [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] IP: {HttpContext.Connection.RemoteIpAddress} | Email: {user.Email} | Objavio oglas: \"{oglas.Naslov}\" | Opis: \"{oglas.Opis}\" | Lokacija: \"{oglas.Lokacija}\" | Rok: {oglas.Rok:yyyy-MM-dd} | Cijena: {oglas.Cijena}";
                var logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", $"activity-log-{DateTime.Now:yyyy-MM-dd}.txt");
                Directory.CreateDirectory(Path.GetDirectoryName(logPath));
                await System.IO.File.AppendAllTextAsync(logPath, detaljniLog + Environment.NewLine);

                _logger.LogInformation("Oglas saved successfully with ID: {OglasId}", oglas.OglasId);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving Oglas.");
                return View(oglas);
            }
        }
    }
}

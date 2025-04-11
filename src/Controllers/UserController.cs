using hamalba.DataBase;
using hamalba.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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

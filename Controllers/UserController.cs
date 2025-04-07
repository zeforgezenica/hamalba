using hamalba.DataBase;
using hamalba.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace hamalba.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly UserManager<Korisnik> _userManager;
        private readonly ApplicationDbContext _context;

        public UserController(UserManager<Korisnik> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
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
            if (ModelState.IsValid)
            {
                _context.Oglasi.Add(oglas);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }

            return View(oglas);
        }

    }
}

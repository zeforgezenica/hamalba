using Microsoft.AspNetCore.Mvc;
using hamalba.Models;

using System;
using hamalba.DataBase;

namespace hamalba.Controllers
{
    public class KontaktController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KontaktController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Kontakt()
        {
            return View();
        }

        //Slanje podataka za kontakt formu

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendMessage(Kontakt model)
        {
            if (ModelState.IsValid)
            {
                _context.Kontakt.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Kontakt");
            }

            
            return View("Kontakt", model);
        }



    }
}

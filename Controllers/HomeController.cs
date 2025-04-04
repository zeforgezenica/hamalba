using hamalba.Services;
using Microsoft.AspNetCore.Mvc;

namespace hamalba.Controllers
{
    public class HomeController : Controller
    {
        private readonly DatabaseService _databaseService;

        public HomeController(DatabaseService databaseService)
        {
            _databaseService = databaseService;

        }

        public IActionResult Index()
        {
            //_databaseService.InitializeDatabase();
            //return Content("Baza je inicijalizovana!");
            return View();
        }
    }
}

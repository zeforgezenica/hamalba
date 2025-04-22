using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hamalba.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("/logs")]
    public class AdminLogsController : Controller
    {
        [HttpGet("{date?}")]
        public IActionResult Index(string? date)
        {
            string logDate = string.IsNullOrEmpty(date)
                ? DateTime.Now.ToString("yyyy-MM-dd")
                : date;

            string logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", $"activity-log-{logDate}.txt");

            if (!System.IO.File.Exists(logPath))
            {
                ViewBag.Log = $"Log fajl za datum {logDate} ne postoji.";
            }
            else
            {
                ViewBag.Log = System.IO.File.ReadAllText(logPath);
            }

            ViewBag.Date = logDate;
            return View();
        }
    }
}

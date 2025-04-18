using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hamalba.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Log")]
    public class LogController : Controller
    {
        [HttpGet("")]
        public IActionResult Activity(string? date)
        {
            if (string.IsNullOrEmpty(date))
                date = DateTime.Now.ToString("yyyy-MM-dd");

            ViewBag.SelectedDate = date;

            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", $"activity-log-{date}.txt");

            if (!System.IO.File.Exists(logPath))
                return View("Activity", Array.Empty<string>());

            var lines = System.IO.File.ReadAllLines(logPath);

            return View("Activity", lines);
        }

    }
}

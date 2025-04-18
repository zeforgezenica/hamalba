using Microsoft.AspNetCore.Mvc;
using hamalba.Models;

public class ChatController : Controller
    {
        private readonly HuggingFaceService _aiService;

        public ChatController()
        {
            _aiService = new HuggingFaceService();
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new Chat());
        }

        [HttpPost]
        public async Task<IActionResult> Index(Chat model)
        {
            var aiResponse = await _aiService.AskAI(model.Question);
            model.Answer = aiResponse;
            return View(model);
        }
    }


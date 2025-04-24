using hamalba.DataBase;
using hamalba.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hamalba.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Korisnik> _userManager;

        public MessageController(ApplicationDbContext context, UserManager<Korisnik> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //  Pretraga korisnika po imenu ili emailu
        [HttpGet]
        public async Task<IActionResult> PretraziKorisnike(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Json(new List<object>());

            var korisnici = await _context.Korisnici
                .Where(k =>
                    (k.Ime + " " + k.Prezime).ToLower().Contains(query.ToLower()) ||
                    k.Email.ToLower().Contains(query.ToLower()))
                .Select(k => new
                {
                    k.Id,
                    k.Ime,
                    k.Prezime
                })
                .ToListAsync();

            return Json(korisnici);
        }

        // 💬 Dohvatanje poruka sa nekim korisnikom
        [HttpGet]
        public async Task<IActionResult> GetPoruke(string korisnikId)
        {
            var trenutniId = _userManager.GetUserId(User);

            var poruke = await _context.Poruke
                .Where(p =>
                    (p.PosiljalacId == trenutniId && p.PrimalacId == korisnikId) ||
                    (p.PosiljalacId == korisnikId && p.PrimalacId == trenutniId))
                .OrderBy(p => p.VrijemeSlanja)
                .Select(p => new
                {
                    p.Sadrzaj,
                    Vrijeme = p.VrijemeSlanja.ToString("HH:mm"),
                    JaSam = p.PosiljalacId == trenutniId
                })
                .ToListAsync();

            return Json(poruke);
        }

        // ✉️ Slanje poruke
        [HttpPost]
        public async Task<IActionResult> PosaljiPoruku(string primalacId, string sadrzaj)
        {
            var posiljalac = await _userManager.GetUserAsync(User);
            if (string.IsNullOrWhiteSpace(sadrzaj))
                return BadRequest("Poruka ne može biti prazna.");

            // 100+ zabranjenih riječi
            var zabranjeneRijeci = new[]
            {
        "jebem", "mater", "kurac", "pička", "nigger", "retard", "idiot", "govno", "fuck", "shit", "asshole",
        "cigan", "debilu", "konju", "majmune", "kreten", "smrade", "glupane", "budalo", "impotentni", "pederu",
        "glupan", "kravo", "debil", "usranče", "seljačino", "idiote", "jebo", "jebala", "drolja", "kurvo", "šupak",
        "puši", "sisač", "smeće", "otpad", "propalico", "šugavče", "luzeru", "serem", "seronjo", "pizde", "drkoš",
        "škrti", "jadniče", "glup", "smrdljivko", "bolesniku", "nakazo", "šizofreničaru", "psihu", "parazitu", "šuga",
        "četniku", "ustašo", "klošaru", "mamu", "čmar", "smetlaru", "ništa", "džukela", "ćorav", "mutavi", "gluhi",
        "ćelavi", "ružnoće", "brabonjak", "prljavi", "luđače", "kozojebu", "magarče", "krmku", "divljaku", "izrode",
        "davežu", "guzice", "čmaru", "pseto", "lezbijo", "transvestitu", "nacisto", "fuj", "zaraženi", "idiotčino",
        "otrovni", "zarazni", "degeneriku", "pokvarenjače", "toksični", "izrođeni", "šugavi", "truli", "gubitniče",
        "jalovče", "ružno", "grozno", "zlo", "katastrofa", "užas", "pakao"
    };

            int brojPrekrsaja = 0;

            foreach (var rijec in zabranjeneRijeci)
            {
                var pattern = @"\b" + System.Text.RegularExpressions.Regex.Escape(rijec) + @"\b";
                if (System.Text.RegularExpressions.Regex.IsMatch(sadrzaj, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    brojPrekrsaja++;
                    sadrzaj = System.Text.RegularExpressions.Regex.Replace(sadrzaj, pattern, "macmac", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                }
            }

            // Zabilježi prekršaje i banuj ako treba
            if (brojPrekrsaja > 0)
            {
                posiljalac.BrojPrekrsaja += brojPrekrsaja;

                if (posiljalac.BrojPrekrsaja >= 3)
                {
                    posiljalac.BanTrajanje = DateTime.UtcNow.AddDays(1);
                    posiljalac.BanRazlog = "Automatski ban zbog uvredljivih poruka";
                }

                _context.Korisnici.Update(posiljalac);
                await _context.SaveChangesAsync();
            }

            var poruka = new Poruka
            {
                PosiljalacId = posiljalac.Id,
                PrimalacId = primalacId,
                Sadrzaj = sadrzaj
            };

            _context.Poruke.Add(poruka);
            await _context.SaveChangesAsync();

            return Ok();
        }



        [HttpGet]
        public async Task<IActionResult> GetRecentConversations()
        {
            var userId = _userManager.GetUserId(User);

            var poruke = await _context.Poruke
                .Where(p => p.PosiljalacId == userId || p.PrimalacId == userId)
                .Include(p => p.Posiljalac)
                .Include(p => p.Primalac)
                .OrderByDescending(p => p.VrijemeSlanja)
                .ToListAsync();

            var razgovori = poruke
                .GroupBy(p => p.PosiljalacId == userId ? p.PrimalacId : p.PosiljalacId)
                .Select(g => g.First())
                .ToList();

            var rezultat = razgovori.Select(p => new
            {
                Id = p.PosiljalacId == userId ? p.Primalac.Id : p.Posiljalac.Id,
                Ime = p.PosiljalacId == userId ? p.Primalac.Ime : p.Posiljalac.Ime,
                Prezime = p.PosiljalacId == userId ? p.Primalac.Prezime : p.Posiljalac.Prezime,
                ImaNovu = p.PrimalacId == userId // ako je primalac trenutno ulogovani user, znači da je posiljalac taj koji mu je nešto poslao
                && p.VrijemeSlanja > DateTime.UtcNow.AddMinutes(-5) 
            });

            return Json(rezultat);
        }

        [HttpGet]
        public async Task<IActionResult> GetBrojNovihPoruka()
        {
            var userId = _userManager.GetUserId(User);

            // Dohvati sve poruke koje je ovaj korisnik primio, ali još nije pročitao (za sad po vremenu)
            var broj = await _context.Poruke
                .Where(p => p.PrimalacId == userId && p.VrijemeSlanja > DateTime.UtcNow.AddMinutes(-2)) // ili prema tvojoj logici
                .CountAsync();

            return Json(broj);
        }
    }
}

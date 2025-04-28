using hamalba.Models;
using System.Collections.Generic;

namespace hamalba.ViewModels
{
    public class ProfilViewModel
    {
        public Korisnik Korisnik { get; set; }
        public List<Oglas> MojiOglasi { get; set; }
        public List<KorisnikOglas> PrijavljeniOglasi { get; set; }
        public List<Recenzija> RecenzijePoslodavac { get; set; }
        public List<Recenzija> RecenzijeRadnik { get; set; }
        public double ProsjecnaOcjenaPoslodavac { get; set; }
        public double ProsjecnaOcjenaRadnik { get; set; }
    }
}
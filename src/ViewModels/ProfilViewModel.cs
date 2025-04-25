using System;
using System.Collections.Generic;
using hamalba.Models;

namespace hamalba.ViewModels
{
    public class ProfilViewModel
    {
        public Korisnik Korisnik { get; set; }
        public List<Oglas> MojiOglasi { get; set; }
        public List<KorisnikOglas> PrijavljeniOglasi { get; set; }
    }
}

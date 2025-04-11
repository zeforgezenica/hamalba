using Microsoft.AspNetCore.Identity;
using System;

namespace hamalba.Models
{
    public class Korisnik : IdentityUser
    {
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public DateTime DatumRegistracije { get; set; } = DateTime.UtcNow;
        public bool Verifikovan { get; set; } = false;
        public DateTime? BanTrajanje { get; set; }
}
}
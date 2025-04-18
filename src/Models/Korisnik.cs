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
        public string? BanRazlog { get; set; }
        public int Arhiviran { get; set; } = 0; // 0 - nije arhiviran, 1 - arhiviran
    }
}
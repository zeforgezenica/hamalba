using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations.Schema;

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
                                                // Nove vrijednosti za ocjene
        public virtual ICollection<Recenzija> PrimljeneRecenzije { get; set; }
        public virtual ICollection<Recenzija> DaneRecenzije { get; set; }

        [NotMapped]
        public decimal ProsjecnaOcjena
        {
            get
            {
                if (PrimljeneRecenzije == null || PrimljeneRecenzije.Count == 0)
                    return 0;

                decimal suma = 0;
                foreach (var recenzija in PrimljeneRecenzije)
                {
                    suma += recenzija.Ocjena;
                }

                return suma / PrimljeneRecenzije.Count;
            }
        }
        [NotMapped]
        public int BrojRecenzija => PrimljeneRecenzije?.Count ?? 0;
    }

}
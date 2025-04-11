// Models/OglasViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace hamalba.Models
{
    public class OglasViewModel
    {
        [Required(ErrorMessage = "Naslov je obavezan")]
        [StringLength(255, ErrorMessage = "Naslov ne smije biti duži od 255 znakova")]
        public string Naslov { get; set; }

        [Required(ErrorMessage = "Opis je obavezan")]
        [StringLength(1000, ErrorMessage = "Opis ne smije biti duži od 1000 znakova")]
        public string Opis { get; set; }

        [Required(ErrorMessage = "Rok je obavezan")]
        public DateTime Rok { get; set; }

        [Required(ErrorMessage = "Kontakt informacije su obavezne")]
        [StringLength(255, ErrorMessage = "Kontakt ne smije biti duži od 255 znakova")]
        public string Kontakt { get; set; }

        [Required(ErrorMessage = "Cijena je obavezna")]
        [Range(0, double.MaxValue, ErrorMessage = "Cijena mora biti pozitivan broj")]
        public decimal Cijena { get; set; }

        [Required(ErrorMessage = "Lokacija je obavezna")]
        [StringLength(255, ErrorMessage = "Lokacija ne smije biti duža od 255 znakova")]
        public string Lokacija { get; set; }

        public OglasStatus Status { get; set; } = OglasStatus.Aktivan;
    }
}
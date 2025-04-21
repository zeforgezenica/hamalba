using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hamalba.Models
{
    public class Poruka
    {
        public int PorukaId { get; set; }

        [Required]
        public string PosiljalacId { get; set; }
        [ForeignKey("PosiljalacId")]
        public Korisnik Posiljalac { get; set; }

        [Required]
        public string PrimalacId { get; set; }
        [ForeignKey("PrimalacId")]
        public Korisnik Primalac { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Sadrzaj { get; set; }

        public DateTime VrijemeSlanja { get; set; } = DateTime.UtcNow;
    }
}

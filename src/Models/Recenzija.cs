using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hamalba.Models
{
    public enum RecenzijaTip
    {
        ZaPoslodavca, ZaRadnika
    }
    public class Recenzija
    {
        [Key]
        public int RecenzijaId { get; set; }

        [Required]
        public int OglasId { get; set; }
        public virtual Oglas Oglas { get; set; }

        [Required]
        public string AutorId { get; set; }  // Korisnik koji daje recenziju
        public virtual Korisnik Autor { get; set; }

        [Required]
        public string PrimaocId { get; set; }  // Korisnik koji prima recenziju
        public virtual Korisnik Primaoc { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Ocjena mora biti između 1 i 5")]
        public int Ocjena { get; set; }

        [StringLength(500, ErrorMessage = "Komentar ne smije biti duži od 500 znakova")]
        public string Komentar { get; set; }

        [Required]
        public DateTime DatumKreiranja { get; set; } = DateTime.UtcNow;

        // Tip recenzije - razlikovanje između recenzije za poslodavca i radnika
        [Required]
        public RecenzijaTip Tip { get; set; }
    }

    
}
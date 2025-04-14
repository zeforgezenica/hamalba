using System;
using System.ComponentModel.DataAnnotations;

namespace hamalba.Models
{
    public class Kontakt
    {
        [Key]
        public int KontaktId { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Poruka { get; set; }

        [Required]
        [StringLength(255)]
        public string Naslov { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public DateTime DatumSlanja { get; set; } = DateTime.Now;




    }
}

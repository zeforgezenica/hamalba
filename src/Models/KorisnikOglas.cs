using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hamalba.Models
{
    public class KorisnikOglas
    {
        [Key]
        public int PrijavaId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int OglasId { get; set; }


        [ForeignKey("UserId")]
        public virtual Korisnik User { get; set; }

        [ForeignKey("OglasId")]
        public virtual Oglas Oglas { get; set; }
        
        public int Status { get; set; } = -1; // -1 = Na čekanju, 1 = Prihvaćen, 0 = Odbijen


    }
}

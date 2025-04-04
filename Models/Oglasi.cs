using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

public enum OglasStatus { Aktivan, Zavrsen, Otkazan, Proces }

namespace hamalba.Models
{
    public class Oglas
    {
        [Key]
        public int OglasId { get; set; } 

        [Required]
        [StringLength(255)]
        public string Naslov { get; set; } 

        [Required]
        [StringLength(1000)]
        public string Opis { get; set; } 

        [Required]
        public DateTime Datum { get; set; }

        [Required]
        public OglasStatus Status { get; set; }

        [Required]
        public DateTime Rok { get; set; } 


        [Required]
        public decimal Cijena { get; set; } 

        [Required]
        [StringLength(255)]
        public string Lokacija { get; set; } 

        
        public string UserId { get; set; } // Strani ključ za korisnika
        public virtual IdentityUser User { get; set; } // Navigacijsko svojstvo za korisnika
    }
}

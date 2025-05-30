using System;
using System.ComponentModel.DataAnnotations;

namespace hamalba.Models
{

    public class Report
    {
        [Key]
        public int ReportId { get; set; }

        [Required]
        public string ReporterUserId { get; set; }
        public virtual Korisnik Reporter { get; set; }

        [Required]
        public string ReportedUserId { get; set; }
        public virtual Korisnik ReportedUser { get; set; }

        [Required]
        public DateTime DatumPrijave { get; set; }

        [Required]
        [StringLength(500)]
        public string Razlog { get; set; }

        public int? OglasId { get; set; }
        public virtual Oglas RelatedOglas { get; set; }

        public bool Pregledano { get; set; } = false;

        public DateTime? DatumPregleda { get; set; }

        public string KomentarAdmina { get; set; }

        // Status: 0 - Novo, 1 - Odbijeno, 2 - Prihvaćeno
        public int Status { get; set; } = 0;
    }
}
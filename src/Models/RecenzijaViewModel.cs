using System.ComponentModel.DataAnnotations;

namespace hamalba.Models
{
    public class RecenzijaViewModel
    {
        [Required(ErrorMessage = "Ocjena je obavezna")]
        [Range(1, 5, ErrorMessage = "Ocjena mora biti između 1 i 5")]
        public int Ocjena { get; set; }

        [StringLength(500, ErrorMessage = "Komentar ne smije biti duži od 500 znakova")]
        public string Komentar { get; set; }

        [Required]
        public int OglasId { get; set; }

        [Required]
        public string PrimaocId { get; set; }

        [Required]
        public RecenzijaTip Tip { get; set; }
    }
}
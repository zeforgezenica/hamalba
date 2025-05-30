// ViewModels/ReportViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;
using hamalba.Models;

namespace hamalba.ViewModels
{
    public class ReportViewModel
    {
        public string ReportedUserId { get; set; }
        public string ReportedUserName { get; set; }

        [Required(ErrorMessage = "Razlog prijave je obavezan.")]
        [StringLength(500, ErrorMessage = "Razlog ne može biti duži od 500 karaktera.")]
        [Display(Name = "Razlog prijave")]
        public string Razlog { get; set; }

        public int? OglasId { get; set; }
        public string OglasNaslov { get; set; }
    }

    public class ReportListViewModel
    {
        public List<Report> Reports { get; set; }
        public string SearchTerm { get; set; }
        public bool ShowReviewed { get; set; } = false;
    }

    public class AdminReportResponseViewModel
    {
        public int ReportId { get; set; }

        [Required(ErrorMessage = "Komentar je obavezan.")]
        public string KomentarAdmina { get; set; }

        [Required(ErrorMessage = "Status prijave je obavezan.")]
        public int Status { get; set; }
    }
}
using Microsoft.AspNetCore.Mvc.Rendering;

namespace hamalba.ViewModels
{
    public class OglasViewModel
    {
        public string Kanton { get; set; }
        public string Grad { get; set; }

        public List<SelectListItem> Kantoni { get; set; }
        public List<string> Gradovi { get; set; }
    }
}

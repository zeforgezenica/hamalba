namespace hamalba.ViewModels
{
    public class OglasFilterViewModel
    {
        public string? Lokacija { get; set; }
        public decimal? MinimalnaCijena { get; set; }
        public decimal? MaksimalnaCijena { get; set; }
        public string? NazivPosla { get; set; } // dio ili cijeli naziv oglasa

        public List<Models.Oglas> Rezultati { get; set; } = new();
    }
}

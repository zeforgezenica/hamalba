namespace hamalba.ViewModels
{
    public enum SortOpcije
    {
        BezSortiranja,
        CijenaUzlazno,
        CijenaSilazno,
        NazivPoslaAZ,
        NazivPoslaZA
    }

    public class OglasFilterViewModel
    {
        public string? Lokacija { get; set; }
        public decimal? MinimalnaCijena { get; set; }
        public decimal? MaksimalnaCijena { get; set; }
        public string? NazivPosla { get; set; } // dio ili cijeli naziv oglasa

        public SortOpcije SortOpcija { get; set; } = SortOpcije.BezSortiranja;

        public List<Models.Oglas> Rezultati { get; set; } = new();
    }
}

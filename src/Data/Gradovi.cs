namespace hamalba.Data
{
    public static class Gradovi
    {
        public static readonly Dictionary<string, List<string>> GradoviPoKantonima = new Dictionary<string, List<string>>
        {
            { "Sarajevski", new List<string> { "Sarajevo", "Ilidža", "Vogošća", "Ilijaš", "Hadžići", "Novi Grad", "Stari Grad" } },
            { "Tuzlanski", new List<string> { "Tuzla", "Lukavac", "Živinice", "Srebrenik", "Gračanica", "Gradačac" } },
            { "Zeničko-dobojski", new List<string> { "Zenica", "Tešanj", "Visoko", "Kakanj", "Zavidovići", "Doboj Jug" } },
            { "Unsko-sanski", new List<string> { "Bihać", "Cazin", "Velika Kladuša", "Sanski Most", "Bosanska Krupa" } },
            { "Hercegovačko-neretvanski", new List<string> { "Mostar", "Čapljina", "Konjic", "Jablanica", "Stolac" } },
            { "Zapadnohercegovački", new List<string> { "Široki Brijeg", "Grude", "Posušje", "Ljubuški" } },
            { "Bosansko-podrinjski", new List<string> { "Goražde", "Pale-Prača", "Foča-Ustikolina" } },
            { "Srednjobosanski", new List<string> { "Travnik", "Bugojno", "Donji Vakuf", "Jajce", "Vitez", "Novi Travnik" } },
            { "Posavski", new List<string> { "Orašje", "Domaljevac", "Odžak" } },
            { "Kanton 10", new List<string> { "Livno", "Tomislavgrad", "Drvar", "Kupres", "Glamoč" } }
        };

        // Svi gradovi kao jedna lista za autocomplete
        public static List<string> SviGradovi => GradoviPoKantonima.SelectMany(x => x.Value).Distinct().OrderBy(g => g).ToList();
    }
}

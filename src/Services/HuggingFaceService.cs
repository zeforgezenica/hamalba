using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class HuggingFaceService
{
    private readonly HttpClient _httpClient;
    //private const string apiKey = "Tvoj AI key";

    public HuggingFaceService()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("https://api-inference.huggingface.co/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<string> AskAI(string input)
    {
        try
        {
            
            string contextFromFile = File.ReadAllText("AIContext/ai_znanje.txt");

            
            string baseContext = "Ti si AI pomoćnik koji daje odgovore na osnovu sledećeg konteksta:\n\n";
            var fullPrompt = baseContext + contextFromFile + "\n\nKorisnik pita: " + input;

            var requestData = new { inputs = fullPrompt };
            var content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api-inference.huggingface.co/models/TinyLlama/TinyLlama-1.1B-Chat",content);

            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return $"⚠️ API greška: {(int)response.StatusCode} - {response.ReasonPhrase}\nDetalji: {responseText}";
            }

            using var jsonDoc = JsonDocument.Parse(responseText);
            var answer = jsonDoc.RootElement[0].GetProperty("generated_text").GetString();

            
            var answerOnly = answer.Contains("Korisnik pita:")
                ? answer.Split("Korisnik pita: " + input)[1].Trim()
                : answer;

            return answerOnly;
        }
        catch (Exception ex)
        {
            return $"❌ Došlo je do greške prilikom komunikacije sa AI servisom:\n{ex.Message}";
        }
    }
}

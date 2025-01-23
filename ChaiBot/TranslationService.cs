using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace ChaiBot;

public class TranslationService
{
    private readonly IConfiguration _configuration;

    public TranslationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<string> TranslateTextAsync(string text, string targetLanguage)
    {
        using (var httpClient = new HttpClient())
        {
            // Odczytaj klucz API z konfiguracji
            var apiKey = _configuration["MyMemoryApi:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("Klucz API MyMemory nie został znaleziony w konfiguracji.");
            }

            // Ustaw język źródłowy na polski (pl)
            var sourceLanguage = "pl"; // Język źródłowy to zawsze polski

            // Wyślij żądanie do MyMemory API
            var url = $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(text)}&langpair={sourceLanguage}|{targetLanguage}&key={apiKey}";
            var response = await httpClient.GetStringAsync(url);

            // Parsowanie odpowiedzi JSON
            var translationResult = JsonDocument.Parse(response);
            var translatedText = translationResult.RootElement
                .GetProperty("responseData")
                .GetProperty("translatedText")
                .GetString();

            return translatedText;
        }
    }
}
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ChaiBot;


var builder = Host.CreateApplicationBuilder(args);

// Dodaj konfigurację z appsettings.json
// Zarejestruj TranslationService
builder.Services.AddSingleton<TranslationService>();

builder.Services
    .AddDiscordGateway()
    .AddApplicationCommands<ApplicationCommandInteraction, ApplicationCommandContext>();

var host = builder.Build();

// Dodaj komendy
host.AddSlashCommand("ping", "Ping!", () => "Pong!")
    .AddUserCommand("Username", (User user) => user.Username)
    .AddMessageCommand("Length", (RestMessage message) => message.Content.Length.ToString());

// Dodaj menu kontekstowe do tłumaczenia
host.AddMessageCommand("Przetłumacz", async (RestMessage message, ApplicationCommandContext context) =>
{
    await message.ModifyAsync(properties => properties.Flags = MessageFlags.Ephemeral);
    // Sprawdź, czy wiadomość nie jest od bota
    if (message.Author.IsBot)
    {
        return "Nie mogę przetłumaczyć wiadomości od bota.";
    }

    // Pobierz TranslationService z kontenera DI
    var translationService = host.Services.GetRequiredService<TranslationService>();

    // Tłumacz wiadomość za pomocą MyMemory API
    var translatedText = await translationService.TranslateTextAsync(message.Content, "en");

    // Wyślij odpowiedź efemeryczną (widoczną tylko dla użytkownika)
    return $"Przetłumaczone: {translatedText}";
});

host.AddModules(typeof(Program).Assembly);
host.UseGatewayEventHandlers();

await host.RunAsync();

using BlazorApp1.Components;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using BlazorApp1.Services;
using BlazorApp1.Classes;

var builder = WebApplication.CreateBuilder(args);

// �������� ����� ���� �� ������������
var telegramBotToken = builder.Configuration["TelegramBotToken"];

// ��������� ������ Telegram ����
builder.Services.AddSingleton<TelegramBotService>(sp => new TelegramBotService(telegramBotToken));

// ������������ FileService
builder.Services.AddHttpClient<FileService>(client =>
{
    client.BaseAddress = new Uri($"https://api.telegram.org/bot{telegramBotToken}/");
});

// ��������� Razor ����������
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

var telegramBotService = app.Services.GetRequiredService<TelegramBotService>();
await telegramBotService.StartAsync(); // ������ ������������ ������

app.Run();


    // FileService class
    public class FileService
    {
        private readonly HttpClient _httpClient;

        public FileService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

    public async Task<List<FileItem>> GetFilesAsync()
    {
        try
        {
            var response = await _httpClient.GetStringAsync("getUpdates");
            Console.WriteLine($"����� �� Telegram API: {response}");

            var updatesResponse = JsonConvert.DeserializeObject<UpdatesResponse>(response);

            if (updatesResponse?.Result == null || updatesResponse.Result.Count == 0)
            {
                Console.WriteLine("��� ���������� � �������");
                return new List<FileItem>();
            }

            var files = updatesResponse.Result
                .Where(update => update.Message?.Document != null)
                .Select(update => new FileItem
                {
                    Name = update.Message.Document?.FileName ?? "Unknown file",
                    Url = $"https://api.telegram.org/file/bot{_httpClient.BaseAddress.Segments[2]}/{update.Message.Document.FileId}"
                })
                .ToList();

            Console.WriteLine($"�����: {files.Count}");
            return files;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"������ ��� ��������� ������: {ex.Message}");
            return new List<FileItem>();
        }
    }

}


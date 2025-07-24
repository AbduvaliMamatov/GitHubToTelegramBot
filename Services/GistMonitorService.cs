using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GitHubToTelegramBot.Services;

public class GistMonitorService : BackgroundService
{
    private readonly ILogger<GistMonitorService> _logger;
    private readonly TelegramService _telegram;

    private const string GitHubUsername = "AbduvaliMamatov";
    private string? _lastGistId = null;

    public GistMonitorService(ILogger<GistMonitorService> logger, TelegramService telegram)
    {
        _logger = logger;
        _telegram = telegram;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🔁 GistMonitorService ishga tushdi.");

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("GitHubToTelegramBot/1.0"); 

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("🔄 Gist tekshirilyapti: {time}", DateTimeOffset.Now);

                var response = await httpClient.GetAsync($"https://api.github.com/users/{GitHubUsername}/gists", stoppingToken);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync(stoppingToken);

                var options = new JsonDocumentOptions
                {
                    AllowTrailingCommas = true
                };

                using var document = JsonDocument.Parse(json, options);
                var root = document.RootElement;

                if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                {
                    var firstGist = root[0];
                    var gistId = firstGist.GetProperty("id").GetString();
                    var description = firstGist.GetProperty("description").GetString() ?? "(Tavsif yo‘q)";
                    var htmlUrl = firstGist.GetProperty("html_url").GetString();

                    _logger.LogInformation("🆔 Gist ID: {gistId}", gistId);
                    _logger.LogInformation("📄 Tavsif: {desc}", description);
                    _logger.LogInformation("🔗 URL: {url}", htmlUrl);

                    if (_lastGistId != gistId)
                    {
                        _lastGistId = gistId;
                        var msg = $"📝 *Yangi Gist yaratildi!*\n\n📄 *{description}*\n🔗 [Ko‘rish]({htmlUrl})";
                        await _telegram.SendMessageAsync(msg);
                        _logger.LogInformation("📬 Yangi gist Telegram'ga yuborildi.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Gist tekshiruvida xatolik yuz berdi.");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}

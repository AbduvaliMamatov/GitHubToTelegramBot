using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class GistMonitorService : BackgroundService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GistMonitorService> _logger;
    private readonly TelegramService _telegram;
    private string? _lastGistId;

    private const string GitHubUsername = "AbduvaliMamatov";

    public GistMonitorService(ILogger<GistMonitorService> logger, TelegramService telegram)
    {
        _httpClient = new HttpClient();
        _logger = logger;
        _telegram = telegram;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var url = $"https://api.github.com/users/{GitHubUsername}/gists";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.UserAgent.ParseAdd("TelegramNotifierBot");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                var firstGist = doc.RootElement.EnumerateArray().FirstOrDefault();

                if (firstGist.ValueKind != JsonValueKind.Undefined)
                {
                    var gistId = firstGist.GetProperty("id").GetString();
                    var description = firstGist.GetProperty("description").GetString();
                    var htmlUrl = firstGist.GetProperty("html_url").GetString();

                    if (_lastGistId != gistId)
                    {
                        _lastGistId = gistId;
                        var msg = $"📝 *Yangi Gist yaratildi!*\n\n📄 *{description}*\n🔗 [Ko‘rish]({htmlUrl})";
                        await _telegram.SendMessageAsync(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gist tekshiruvida xatolik");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}

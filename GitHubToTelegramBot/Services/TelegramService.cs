using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GitHubToTelegramBot.Services;

public class TelegramService
{
    private readonly HttpClient _httpClient;
    private readonly string _botToken;
    private readonly string _channelId;

    public TelegramService(IConfiguration config)
    {
        _httpClient = new HttpClient();
        _botToken = config["TELEGRAM_BOT_TOKEN"] ?? throw new ArgumentNullException("TELEGRAM_BOT_TOKEN");
        _channelId = config["TELEGRAM_CHANNEL_ID"] ?? throw new ArgumentNullException("TELEGRAM_CHANNEL_ID");
    }

    public async Task SendMessageAsync(string text)
    {
        var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";

        var content = new StringContent(JsonSerializer.Serialize(new
        {
            chat_id = _channelId,
            text = text,
            parse_mode = "Markdown"
        }), Encoding.UTF8, "application/json");

        await _httpClient.PostAsync(url, content);
    }
}

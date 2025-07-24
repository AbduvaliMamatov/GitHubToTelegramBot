using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GitHubToTelegramBot.Services;

namespace GitHubToTelegramBot.Controllers;

[ApiController]
[Route("webhook/github")]
public class WebhookController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly TelegramService _telegram;

    public WebhookController(IConfiguration config, TelegramService telegram)
    {
        _config = config;
        _telegram = telegram;
    }

    [HttpPost]
    public async Task<IActionResult> Post()
    {
        // Imzo tekshiruvi
        if (!Request.Headers.TryGetValue("X-Hub-Signature-256", out var signatureHeader))
            return Unauthorized("❌ Signature header yo'q");

        string secret = _config["GitHubSecret"] ?? throw new Exception("GitHubSecret topilmadi");
        string computedHash;

        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
        {
            using var memoryStream = new MemoryStream();
            await Request.Body.CopyToAsync(memoryStream);
            var payloadBytes = memoryStream.ToArray();
            var hashBytes = hmac.ComputeHash(payloadBytes);
            computedHash = "sha256=" + BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

            // Imzo solishtirish
            if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(signatureHeader),
                Encoding.UTF8.GetBytes(computedHash)))
            {
                return Unauthorized("❌ Signature noto‘g‘ri");
            }

            // JSON o‘qish
            memoryStream.Position = 0;
            using var reader = new StreamReader(memoryStream);
            var body = await reader.ReadToEndAsync();
            var json = JsonDocument.Parse(body);

            var pusher = json.RootElement.GetProperty("pusher").GetProperty("name").GetString();
            var repo = json.RootElement.GetProperty("repository").GetProperty("full_name").GetString();
            var branch = json.RootElement.GetProperty("ref").GetString()?.Split("/").Last();
            var commits = json.RootElement.GetProperty("commits");

            var commitMessages = commits.EnumerateArray()
                .Select(c => $"- {c.GetProperty("message").GetString()}");

            var message = $"📦 *{pusher}* repo *{repo}* ga push qildi `{branch}` branch:\n" +
                          string.Join("\n", commitMessages);

            await _telegram.SendMessageAsync(message);
            return Ok("✅ Push qayd etildi");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using GitHubToTelegramBot.Services;

namespace GitHubToTelegramBot.Controllers;

[ApiController]
[Route("webhook/github")]
public class WebhookController : ControllerBase
{
    private readonly TelegramService _telegram;

    public WebhookController(TelegramService telegram)
    {
        _telegram = telegram;
    }

    [HttpPost]
    public async Task<IActionResult> HandleGitHubWebhook([FromBody] dynamic payload)
    {
        try
        {
            string repo = payload.repository?.full_name;
            string pusher = payload.pusher?.name;
            string branch = payload.@ref;
            string commits = "";

            foreach (var commit in payload.commits)
            {
                commits += $"- [{commit.id.ToString().Substring(0, 7)}] {commit.message} ({commit.author.name})\n";
            }

            string message = $"🚀 *{pusher}* pushed to *{repo}*\nBranch: `{branch}`\n\n*Commits:*\n{commits}";

            await _telegram.SendMessageAsync(message);
            return Ok(new { status = "Message sent to Telegram" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

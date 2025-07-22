using GitHubToTelegramBot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<TelegramService>();
    });

var app = builder.Build();

var telegram = app.Services.GetRequiredService<TelegramService>();
await telegram.SendMessageAsync("✅ Bot Railway'dan muvaffaqiyatli ishga tushdi!");

await app.RunAsync();

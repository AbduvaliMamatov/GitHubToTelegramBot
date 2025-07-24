using GitHubToTelegramBot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.ConfigureServices(services =>
        {
            services.AddControllers();
            services.AddSingleton<TelegramService>();
            services.AddHostedService<GistMonitorService>(); // 👈 BU SHART!
        });

        webBuilder.Configure(app =>
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        });
    })
    .UseConsoleLifetime();

var app = builder.Build();

var telegram = app.Services.GetRequiredService<TelegramService>();
await telegram.SendMessageAsync("✅ Bot Railway'dan muvaffaqiyatli ishga tushdi!");

await app.RunAsync();

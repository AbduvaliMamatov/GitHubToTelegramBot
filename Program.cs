using GitHubToTelegramBot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.ConfigureServices(services =>
        {
            services.AddControllers(); // Web API controllerlar uchun
            services.AddSingleton<TelegramService>();
            services.AddHostedService<GistMonitorService>(); // ✅ Gist kuzatuv servisi
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
    .UseConsoleLifetime(); // Bu serverni to'liq ishga tushiradi (ctrl+C to'xtatadi)

var app = builder.Build();

var telegram = app.Services.GetRequiredService<TelegramService>();
await telegram.SendMessageAsync("✅ Bot Railway'dan muvaffaqiyatli ishga tushdi!");

await app.RunAsync();

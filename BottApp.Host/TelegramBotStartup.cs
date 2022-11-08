using BottApp.Database;
using BottApp.Host.Configs;
using BottApp.Host.Services;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

namespace BottApp.Host;

internal class TelegramBotStartup
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BotConfig>(configuration.GetSection("BotConfiguration"));

        var botConfig = configuration.GetSection("BotConfiguration").Get<BotConfig>();

        services.AddHttpClient("telegram_bot_client")
            .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
            {
                // BotConfig? botConfig = sp.GetConfiguration<BotConfig>();
                TelegramBotClientOptions options = new(botConfig.Token);
                return new TelegramBotClient(options, httpClient);
            });


        services.AddScoped<UpdateHandler>();
        services.AddScoped<ReceiverService>();
        services.AddHostedService<PollingService>();
    }
}
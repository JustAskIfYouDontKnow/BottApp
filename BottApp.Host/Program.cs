using System.Runtime.CompilerServices;
using BottApp.Database;
using BottApp.Database.Service;
using BottApp.Host.Controllers.Client;
using BottApp.Host.Handlers;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Factory = BottApp.Host.Handlers.Factory;

namespace BottApp.Host;


public class Program
{
    public readonly IServiceCollection _ServiceCollection;
    public readonly IConfiguration _Configuration;
    public Program(IConfiguration configuration, IServiceCollection serviceCollection)
    {
        _Configuration = configuration;
        _ServiceCollection = serviceCollection;
    }
    public static void Main(string[] args)
    {
        
   
        
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services.AddDbContext<PostgreSqlContext>(
            opt => opt.UseNpgsql("Host=localhost;Port=5432;Database=bottapp_server;Username=postgres;Password=123")
        );
        
        var botConfigurationSection = builder.Configuration.GetSection(BotConfiguration.Configuration);
        builder.Services.Configure<BotConfiguration>(botConfigurationSection);
        
        var botConfiguration = botConfigurationSection.Get<BotConfiguration>();
        builder.Services.AddHttpClient("telegram_bot_client")
            .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
            {
                var botConfig = sp.GetConfiguration<BotConfiguration>();
                TelegramBotClientOptions options = new(botConfig.BotToken);
                return new TelegramBotClient(options, httpClient);
            });
       
        builder.Services.AddHostedService<ConfigureWebhook>();
        builder.Services
            .AddControllers()
            .AddNewtonsoftJson();
        
       
        builder.Services.AddScoped<UpdateHandler>();
        
        builder.Services.AddScoped<IDatabaseContainer, DatabaseContainer>();
        builder.Services.AddScoped<IHandlerContainer>(
            x => Factory.Create
            (
                x.GetRequiredService<IDatabaseContainer>(),
                x.GetRequiredService<IServiceContainer>()
            )
        );
        
        
        builder.Services.AddScoped<IServiceContainer>(
            x => Database.Service.Factory.Create(x.GetRequiredService<IDatabaseContainer>()));
        
      
        var app = builder.Build();
        
        app.MapBotWebhookRoute<UserController>(route: botConfiguration.Route);
        app.MapControllers();
        app.Run();
    }
}




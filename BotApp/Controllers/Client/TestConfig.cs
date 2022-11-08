using System;
using BottApp.Host.Exp.Configs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BottApp.Host.Exp.Controllers.Client;

public class TestConfig : AbstractClientController<TestConfig>
{
    private readonly BotConfig _botConfig;
    
    public TestConfig(ILogger<TestConfig> logger, BotConfig botConfig) : base(logger)
    {
        _botConfig = botConfig;
    }

    [HttpGet]
    public void GetBotConfig()
    {
        Console.WriteLine(_botConfig.BotToken);
    }
}
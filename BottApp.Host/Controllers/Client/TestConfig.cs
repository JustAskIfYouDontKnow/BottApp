using System;
using BottApp.Client.Bot;
using BottApp.Client.Payload.User;
using BottApp.Client.User;
using BottApp.Database;
using BottApp.Host.Configs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BottApp.Host.Controllers.Client;

public class TestConfig : AbstractClientController<TestConfig>
{
    private readonly BotConfig _botConfig;

    private readonly IDatabaseContainer _databaseContainer;
    
    public TestConfig(ILogger<TestConfig> logger, BotConfig botConfig, IDatabaseContainer databaseContainer) : base(logger)
    {
        _botConfig = botConfig;
        _databaseContainer = databaseContainer;
    }

    [HttpGet]
    public void GetBotConfig()
    {
        Console.WriteLine(_botConfig.Token);
    }


    [HttpGet]
    [ProducesResponseType(typeof(BotUpdate.Response), 200)]
    public async Task<BotUpdate.Response> CreateUser([FromBody] BotUpdate request)
    {
        var user = await _databaseContainer.User.FindOneByUid((int) request.Message.Chat.Id);
        
        
        
        return new BotUpdate.Response();
    }

    // [HttpGet]
    // [ProducesResponseType(typeof(GetOne.Response), 200)]
    // public async Task<GetOne.Response> CreateUser([FromBody] GetOne request)
    // {
        
        // var user = await _databaseContainer.User.FindOneByUid((int) request.Update.Message.Chat.Id);
        //
        // return new GetOne.Response(user);
    // }
}
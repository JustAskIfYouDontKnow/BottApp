﻿using System;
using System.Threading.Tasks;
using BottApp.Utils;
using Microsoft.Extensions.Logging;

namespace BottApp.Database.Message;

public class MessageRepository : AbstractRepository<MessageModel>, IMessageRepository
{
    public MessageRepository(PostgreSqlContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
    {
    }

    public async Task<MessageModel> CreateModel(int userId, string? description, Timestamp createdAt)
    {
        var model = MessageModel.CreateModel(userId, description, createdAt);

        var result = await CreateModelAsync(model);
        if (result == null)
        {
            throw new Exception("Message model is not created");
        }

        return result;
    }
}
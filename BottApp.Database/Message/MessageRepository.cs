﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BottApp.Database.Message;

public class MessageRepository : AbstractRepository<MessageModel>, IMessageRepository
{
    public MessageRepository(PostgresContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
    {
    }

    public async Task<MessageModel> CreateModel(int userId, string? description, string? type, DateTime createdAt)
    {
        var model = MessageModel.CreateModel(userId, description, type, createdAt);

        var result = await CreateModelAsync(model);
        if (result == null)
        {
            throw new Exception("Message model is not created");
        }

        return result;
    }
}
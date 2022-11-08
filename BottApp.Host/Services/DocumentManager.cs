﻿using BottApp.Database;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Host.Services;

public static class DocumentManager
{
    public static async Task Save(IDatabaseContainer _databaseContainer, Message message, ITelegramBotClient _botClient)
    {
        var documentType = message.Type.ToString();
        var fileInfo = await _botClient.GetFileAsync(message.Document.FileId);
        var filePath = fileInfo.FilePath;
        var extension = Path.GetExtension(filePath);
        
            
        var rootPath = Directory.GetCurrentDirectory() + "/DATA/";
        var newPath = Path.Combine(rootPath, message.Chat.FirstName + "___" + message.Chat.Id, documentType, extension);

        if (!Directory.Exists(newPath))
        {
            Directory.CreateDirectory(newPath);
        }

        string destinationFilePath = newPath + $"/{message.Chat.FirstName}__{Guid.NewGuid().ToString("N")}__{message.Chat.Id}__{extension}";
        
        ///
        var user = await _databaseContainer.User.FindOneById((int)message.Chat.Id);
        await _databaseContainer.Document.CreateModel(user.Id, documentType, extension, DateTime.Now, destinationFilePath);
        ///

        await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
        await _botClient.DownloadFileAsync(filePath, fileStream);
        fileStream.Close();
        

        await _botClient.SendTextMessageAsync(message.Chat.Id, "Спасибо! Ваш документ загружен в базу данных.");
 
    }
}
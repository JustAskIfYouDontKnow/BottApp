﻿using System;
using System.IO;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace BottApp.Host.Example
{
    public class DownloaManager
    {
        static string FileID { get; set; }
        async static public void DownloadDocument(ITelegramBotClient botClient, Message message, MessageType type)
        {


            try
            {
                if (type == MessageType.Photo)
                {
                    var _field = message.Photo[message.Photo.Length - 1].FileId;
                }
                if (type == MessageType.Document)
                {
                    var _field = message.Document.FileId;
                }
                if (type == MessageType.Voice)
                {
                    var _field = message.Voice.FileId;
                }
                if (type == MessageType.Video)
                {
                    var _field = message.Video.FileId;
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine($"{type} | Error downloading: " + ex.Message);
                return;
            }

            finally
            {

                if (type == MessageType.Photo)
                {
                    FileID = message.Photo[message.Photo.Length - 1].FileId;
                }
                if (type == MessageType.Document)
                {
                    FileID = message.Document.FileId;
                }
                if (type == MessageType.Voice)
                {
                    FileID = message.Voice.FileId;
                }
                if (type == MessageType.Video)
                {
                    FileID = message.Video.FileId;
                }
            }

            var fileInfo = await botClient.GetFileAsync(FileID);
            var filePath = fileInfo.FilePath;
            var extension = Path.GetExtension(filePath);

            
            var rootPath = Directory.GetCurrentDirectory() + "/DATA/";
            var folderName = type;
            var newPath = Path.Combine(rootPath, message.Chat.FirstName, folderName.ToString(), extension);

            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }

            string destinationFilePath = newPath + $"/{message.Chat.FirstName}__{Guid.NewGuid().ToString("N")}__{message.Chat.Id}__{extension}";

            await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
            await botClient.DownloadFileAsync(filePath, fileStream);
            fileStream.Close();

            Console.WriteLine(
                $"Документ {type} получен от пользователя {message.Chat.FirstName}" +
                $" ID {message.Chat.Id}, " +
                $" размер документа {fileInfo.FileSize} байт." +
                $" \nFULLPATH {destinationFilePath}");

            await botClient.SendTextMessageAsync(message.Chat.Id, "Спасибо! Ваш документ загружен в базу данных.");

            return;

        }

    }
}

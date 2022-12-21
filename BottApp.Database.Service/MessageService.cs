﻿using BottApp.Database.Service.Keyboards;
using BottApp.Database.User;
using BottApp.Database.UserMessage;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Database.Service;

public class MessageService : IMessageService
{
    private readonly IUserRepository _userRepository;
    private readonly IMessageRepository _messageRepository;
    private List<Message> _messageList = new();


    public MessageService(IUserRepository userRepository, IMessageRepository messageRepository)
    {
        _userRepository = userRepository;
        _messageRepository = messageRepository;
    }


    public async Task SaveMessage(Message message)
    {
        var user = await _userRepository.FindOneByUid((int)message.Chat.Id);
        string type = message.Type.ToString();
        await _messageRepository.CreateModel(user.Id, message.Text, type, DateTime.Now);
    }

    public async Task SaveInlineMessage(CallbackQuery callbackQuery)
    {
        var user = await _userRepository.FindOneByUid((int)callbackQuery.Message.Chat.Id);
        string type = callbackQuery.GetType().ToString();
        await _messageRepository.CreateModel(user.Id, callbackQuery.Data, type, DateTime.Now);
    }

    public async Task<Message> TryEditInlineMessage(
        ITelegramBotClient botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken
    )
    {
        var viewText = "Такой команды еще нет ";
        var viewExceptionText = "Все сломаделось : ";
        var editText = viewText + GetTimeEmooji();

        try
        {
            try
            {
                return await botClient.EditMessageTextAsync
                (
                    chatId: callbackQuery.Message.Chat.Id,
                    messageId: callbackQuery.Message.MessageId,
                    text: editText,
                    replyMarkup: Keyboard.MainKeyboard,
                    cancellationToken: cancellationToken
                );
            }
            catch
            {
                editText = viewText + GetTimeEmooji();

                return await botClient.EditMessageTextAsync
                (
                    chatId: callbackQuery.Message.Chat.Id,
                    messageId: callbackQuery.Message.MessageId,
                    text: editText,
                    replyMarkup: Keyboard.MainKeyboard,
                    cancellationToken: cancellationToken
                );
            }
        }
        catch (Exception e)
        {
            return await botClient.SendTextMessageAsync
            (
                chatId: callbackQuery.Message.Chat.Id,
                text: viewExceptionText + "\n" + e,
                replyMarkup: Keyboard.MainKeyboard,
                cancellationToken: cancellationToken
            );
        }
    }


    private string GetTimeEmooji()
    {
        string[] emooji = { "🕐", "🕑", "🕒", "🕓", "🕔", "🕕", "🕖", "🕗", "🕘", "🕙", "🕚", "🕛", "🕐 ", "🕑 ", };
        var rand = new Random();
        var preparedString = emooji[rand.Next(0, emooji.Length)];
        return preparedString;
    }
    
    public async Task MarkMessageToDelete(Message message)
    {
         _messageList.Add(message);
    }

    public async Task<bool> TryDeleteAfterReboot(ITelegramBotClient botClient, long UId, int messageId)
    {
        var lastIndex = messageId - 1;

        await botClient.DeleteMessageAsync(
            chatId: UId,
            messageId: lastIndex);

        return true;
    }


    public async Task DeleteMessages(ITelegramBotClient botClient, long UId, int messageId)
    {
        // try
        // {
        //     for (var i = 0; i < 15; i++)
        //     {
        //         await botClient.DeleteMessageAsync(
        //             chatId: UId,
        //             messageId: messageId -i);
        //     }
        // }
        // catch (Exception e)
        // {
        //     Console.WriteLine(e);
        // }
        //
        
        var tempMessageList = new List<Message>();
        
        foreach (var message in _messageList)
        {
            if (message.Chat.Id == UId)
            {
                tempMessageList.Add(message);
            }
        }
        
        if (tempMessageList.Count != 0)
        {
            foreach (var message in tempMessageList)
            {
                _messageList.Remove(message);
            }
            
            for (var i = tempMessageList.Count - 1; i >= 0; i--)
            {
                await botClient.DeleteMessageAsync(
                    chatId: tempMessageList[i].Chat.Id,
                    messageId: tempMessageList[i].MessageId);
                
                tempMessageList.RemoveAt(i);
            }
        }
    }
}
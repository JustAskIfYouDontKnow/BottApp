using BottApp.Database;
using BottApp.Database.User;
using BottApp.Host.Keyboards;
using BottApp.Host.Services.Handlers.MainMenu;
using BottApp.Host.SimpleStateMachine;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using OnState = BottApp.Client.Payload.User.OnState;

namespace BottApp.Host.Services.Handlers.Votes;

public class VotesHandler : IVotesHandler
{
    private readonly IUserRepository _userRepository;
    private readonly DocumentManager _documentManager;
    public VotesHandler(IUserRepository userRepository, DocumentManager documentManager)
    {
        _userRepository = userRepository;
        _documentManager = documentManager;
    }

  public string GetTimeEmooji()
    {
        string[] emooji = {"🕐", "🕑", "🕒", "🕓", "🕔", "🕕", "🕖", "🕗", "🕘", "🕙", "🕚", "🕛", "🕐 ", "🕑 ",};
        var rand = new Random();
        var preparedString = emooji[rand.Next(0, emooji.Length)];
        return preparedString;
    }
    public async Task<Message> TryEditMessage(ITelegramBotClient? botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
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
                    replyMarkup: Keyboard.MainKeyboardMarkup,
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
                    replyMarkup: Keyboard.MainKeyboardMarkup,
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
                replyMarkup: Keyboard.MainKeyboardMarkup,
                cancellationToken: cancellationToken
            );
        }
    }

    public async Task BotOnCallbackQueryReceived(ITelegramBotClient? botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        // _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);

        var action = callbackQuery.Data.Split(' ')[0] switch
        {
            "ButtonRight" => await DocumentManager.SendVotesDocument(callbackQuery, botClient, cancellationToken),
            "ButtonLeft" => await DocumentManager.SendVotesDocument (callbackQuery, botClient, cancellationToken),
            "ButtonBack" => await BackToLastInterface(botClient, callbackQuery, cancellationToken),

            _ => await TryEditMessage(botClient, callbackQuery, cancellationToken)
        };
    }
    

    public async Task<Message> BackToLastInterface(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        //ToDo: Аккуратно реализовать смену состояния Юзера используя S из SOLID
        // await _userRepository.(callbackQuery.Message.Chat.Id, OnState.Menu);
        
        return await botClient.SendTextMessageAsync
        (
            chatId: callbackQuery.Message.Chat.Id,
            text: "Переходим в главное меню!",
            replyMarkup: Keyboard.MainKeyboardMarkup,
            cancellationToken: cancellationToken
        );
    }
    
    public async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        
        if (message.Text is not { } messageText)
            return;

        var action = messageText.Split(' ')[0] switch
        {
            _ => Usage(botClient, message, cancellationToken)
        };
        
        static async Task<Message> Usage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync
            (
                chatId: message.Chat.Id,
                text: "Хорошая попытка, но тебе нужно использовать виртуальные кнопки",
                cancellationToken: cancellationToken
            );
            await Task.Delay(100);
            return await botClient.SendTextMessageAsync
            (
                chatId: message.Chat.Id,
                text: "Голосование",
                replyMarkup: Keyboard.VotesKeyboardMarkup,
                cancellationToken: cancellationToken
            );
        }
    }
    

    public Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
    {
        // _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }

    public async Task HandlePollingErrorAsync
        (ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException =>
                $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };
        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }
}
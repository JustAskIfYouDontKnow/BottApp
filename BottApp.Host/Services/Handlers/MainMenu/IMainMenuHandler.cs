using BottApp.Database.User;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Host.Services.Handlers.MainMenu;

public interface IMainMenuHandler
{
    string GetTimeEmooji();


    Task<Message> TryEditMessage(
        ITelegramBotClient? botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken
    );


    Task BotOnCallbackQueryReceived(
        ITelegramBotClient? botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken,
        UserModel user
    );


    Task BotOnMessageReceived(
        ITelegramBotClient botClient,
        Message message,
        CancellationToken cancellationToken,
        UserModel user
    );


    Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken);


    Task HandlePollingErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        CancellationToken cancellationToken
    );
}
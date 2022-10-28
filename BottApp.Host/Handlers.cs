using BottApp;
using BottApp.Database;
using BottApp.Database.User;
using BottApp.Host;
using Microsoft.EntityFrameworkCore;
using Npgsql.Replication.TestDecoding;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Examples.Polling
{
    public class Handlers
    {
        
        public readonly IDatabaseContainer _databaseContainer;

        public Handlers(IDatabaseContainer databaseContainer)
        {
            _databaseContainer = databaseContainer;
        }
        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                // UpdateType.Unknown:
                // UpdateType.ChannelPost:
                // UpdateType.EditedChannelPost:
                // UpdateType.ShippingQuery:
                // UpdateType.PreCheckoutQuery:
                // UpdateType.Poll:
                UpdateType.Message => BotOnMessageReceived(botClient, update.Message!),
                UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage!),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(botClient, update.CallbackQuery!),
                UpdateType.InlineQuery => BotOnInlineQueryReceived(botClient, update.InlineQuery!),
                UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(botClient, update.ChosenInlineResult!),
                _ => UnknownUpdateHandlerAsync(botClient, update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }


        public async Task<UserModel> Test(int uid, string firstName, string userPhone, bool isSendContact)
        {
            var user = await _databaseContainer.User.CreateUser(uid, firstName,userPhone, isSendContact);
            return user;
        }
        private static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {
             //Test(message.MessageId, message.Chat.Username,message.Contact.PhoneNumber,true);

             #region
            var preparedMessage = message.Text.ToLower();
            var action = preparedMessage.Split('@')[0] switch
            {
                "/keyboard" => SendReplyKeyboard(botClient, message),
                "/remove" => RemoveKeyboard(botClient, message),
                "/photo" => SendFile(botClient, message),
                "/request" => RequestContactAndLocation(botClient, message),
                "/hello" => Start(botClient, message),
                "/votes" => VotesInlineKeyboard(botClient, message),
                _ => Usage(botClient, message)
            };
            Message sentMessage = await action;

            Console.WriteLine($"The message was sent with id: {sentMessage.MessageId}");

            // Send inline keyboard
            // You can process responses in BotOnCallbackQueryReceived handler

            static async Task<Message> Start(ITelegramBotClient botClient, Message message)
            {
                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                           text: "Hello",
                                                           replyMarkup: Keyboards.WelcomeKeyboard);
            }

            static async Task<Message> VotesInlineKeyboard(ITelegramBotClient botClient, Message message)
            {
                await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                await Task.Delay(500);

                InlineKeyboardMarkup inlineKeyboard = new(
                    new[]
                    {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("<", "SwipeLeft"),
                        InlineKeyboardButton.WithCallbackData(">", "SwipeRight"),
                    },
                    // second row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("�����", $"/remove"),
                        InlineKeyboardButton.WithCallbackData("������!", $"������-������! {message.Chat.FirstName}"),
                    },
                    });

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: "��� ������!",
                                                            replyMarkup: inlineKeyboard);
            }

            static async Task<Message> SendReplyKeyboard(ITelegramBotClient botClient, Message message)
            {
                ReplyKeyboardMarkup replyKeyboardMarkup = new(
                    new[]
                    {
                        new KeyboardButton[] { "��� ������", "���� ������" },
                        new KeyboardButton[] { "� ����������", "�������" },
                    })
                {
                    ResizeKeyboard = true
                };

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: "Choose",
                                                            replyMarkup: replyKeyboardMarkup);
            }

            static async Task<Message> RemoveKeyboard(ITelegramBotClient botClient, Message message)
            {
                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: "Removing keyboard",
                                                            replyMarkup: new ReplyKeyboardRemove());
            }

            static async Task<Message> SendFile(ITelegramBotClient botClient, Message message)
            {
                await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

                const string filePath = @"Files/tux.png";
                using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();

                return await botClient.SendPhotoAsync(chatId: message.Chat.Id,
                                                      photo: new InputOnlineFile(fileStream, fileName),
                                                      caption: "Nice Picture");
            }

            static async Task<Message> RequestContactAndLocation(ITelegramBotClient botClient, Message message)
            {
                ReplyKeyboardMarkup RequestReplyKeyboard = new(
                    new[]
                    {
                    KeyboardButton.WithRequestLocation("Location"),
                    KeyboardButton.WithRequestContact("Contact"),
                    })
                    {ResizeKeyboard = true};

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: "��� �� � ��� ��?",
                                                            replyMarkup: RequestReplyKeyboard);
            }

            static async Task<Message> ContactHandler(ITelegramBotClient botClient, Message message)
            {
                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: "�������");
            }












            static async Task<Message> Usage(ITelegramBotClient botClient, Message message)
            {
                const string usage = "Usage:\n" +
                                     "/inline   - send inline keyboard\n" +
                                     "/keyboard - send custom keyboard\n" +
                                     "/remove   - remove custom keyboard\n" +
                                     "/photo    - send a photo\n" +
                                     "/request  - request location or contact\n" +
                                     "/votes    - �����������";

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: usage,
                                                            replyMarkup: new ReplyKeyboardRemove());
            }

         
            #endregion
        }

        // Process Inline Keyboard callback data
        private static async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {

            await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: callbackQuery.Data);

            await botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: callbackQuery.Data);
        }

        private static async Task BotOnInlineQueryReceived(ITelegramBotClient botClient, InlineQuery inlineQuery)
        {
            Console.WriteLine($"Received inline query from: {inlineQuery.From.Id}");

            InlineQueryResult[] results = {
            // displayed result
            new InlineQueryResultArticle(
                id: "3",
                title: "TgBots",
                inputMessageContent: new InputTextMessageContent(
                    "hello"
                )
            )
        };

            await botClient.AnswerInlineQueryAsync(inlineQueryId: inlineQuery.Id,
                                                   results: results,
                                                   isPersonal: true,
                                                   cacheTime: 0);
        }

        private static Task BotOnChosenInlineResultReceived(ITelegramBotClient botClient, ChosenInlineResult chosenInlineResult)
        {
            Console.WriteLine($"Received inline result: {chosenInlineResult.ResultId}");
            return Task.CompletedTask;
        }

        private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
        {
            Console.WriteLine($"Unknown update type: {update.Type}");
            return Task.CompletedTask;
        }
    }

    #region Old methods
  /*  async static Task Update(ITelegramBotClient bot, Update update, CancellationToken CLToken)
    {


        var message = update.Message;

        if (update.Type == UpdateType.Message)
        {
            try
            {
                var _text = update.Message.Text;
                var _id = update.Message.Chat.Id;
                var _username = update.Message.Chat.FirstName;
            }
            catch (Exception ex)
            {
                Console.WriteLine("�������� ����������! | " + ex);
                await bot.SendTextMessageAsync(message.Chat.Id, "��-��, ��� �����������!");
                return;
            }

            var id = update.Message.Chat.Id;
            var firstName = update.Message.Chat.FirstName;
            var userName = update.Message.Chat.Username;

            if (update.Message.Type == MessageType.Text)
            {

                var preparedMessage = message.Text.ToLower();

                if (preparedMessage.Contains("������") || preparedMessage.Contains("/start"))
                {
                    await bot.SendTextMessageAsync(id, "������! ��������� ��� ������� ��� �����", replyMarkup: Keyboards.WelcomeKeyboard);
                    return;
                }

                else
                {
                    await bot.SendTextMessageAsync(id, "�� ������ ����� ���. (�������� ������ ��� � ����������)", replyMarkup: Keyboards.WelcomeKeyboard);
                    return;
                }
            }

            if (update.Message.Type == MessageType.Contact)
            {
                Console.WriteLine("Contact is Send");

                var userPhone = message.Contact.PhoneNumber;

                ///////
                //  TODO: ����� ������ ���� ������ ������ ���������� ���� � ����
                //  Example: AddUserModelToDB(firstName, userPhone, true);
                ///////

                await bot.SendTextMessageAsync(id, $"�������, {firstName}, ���  ����� +{userPhone} �������! ");
                await Task.Delay(1000);
                await bot.SendTextMessageAsync(id, "������ �������� ����������� ����� ����, � ��������� ��� ������!");

                return;
            }
        }
    }

    async Task ErrorHandler(ITelegramBotClient client, Exception exception, CancellationToken CLToken)
    {
        Console.WriteLine("Fatal error | " + exception);
        return;
    }*/
    #endregion 
}
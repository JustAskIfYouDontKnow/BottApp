using System;
using System.Threading;
using System.Threading.Tasks;
using BottApp.Database.User;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BottApp.Host.Controllers.Client;

public class UserController : AbstractClientController<UserController>
{
    private bool IsSendContact { get; set; }
    
    public UserController(ILogger<UserController> logger) : base(logger)
    {
        IsSendContact = false;
    }
    
    public async Task Update(ITelegramBotClient bot, Update update, CancellationToken CLToken)
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
                    Console.WriteLine("Возникло исключение! | " + ex);
                    await bot.SendTextMessageAsync(message.Chat.Id, "Ой-ёй, все сломаделось!");
                    return;
                }

                var id = update.Message.Chat.Id;
                var firstName = update.Message.Chat.FirstName;
                var userName = update.Message.Chat.Username;

                if (update.Message.Type == MessageType.Text)
                {

                    var preparedMessage = message.Text.ToLower();

                    if (preparedMessage.Contains("привет") || preparedMessage.Contains("/start"))
                    {
                        if (!IsSendContact)
                        {
                            await bot.SendTextMessageAsync(id, "Привет! Отправьте ваш контакт для связи", replyMarkup: Keyboards.WelcomeKeyboard);
                            return;
                        }
                        else
                        {
                            await bot.SendTextMessageAsync(id, $"Спасибо, {firstName}, но вы уже делились контактом. Все записано, не переживайте!", replyMarkup: Keyboards.DefaultKeyboard);
                            return;
                        }

                    }


                    if (preparedMessage.Contains("контакты"))
                    {
                        await bot.SendTextMessageAsync(id, "Держи!", replyMarkup: Keyboards.inlineUrlKeyboard);
                        return;
                    }

                    if (preparedMessage.Contains("голосование"))
                    {
                        await bot.SendTextMessageAsync(id, "Выбирай", replyMarkup: Keyboards.VotesKeyboard);
                        return;
                    }

                    if (preparedMessage.Contains("главное меню"))
                    {
                        await bot.SendTextMessageAsync(id, "Выбирай", replyMarkup: Keyboards.DefaultKeyboard);
                        return;
                    }

                    if (preparedMessage.Contains("отправить документ"))
                    {
                        await bot.SendTextMessageAsync(id, "Выберите документы через скрепку и отправьте в чат. Размер одного документа должен быть до 20 Мб.", replyMarkup: Keyboards.DefaultKeyboard);
                        return;
                    }
                    if (preparedMessage.Contains("отладка"))
                    {
                        await bot.SendTextMessageAsync(id, "Раздел отладки", replyMarkup: Keyboards.debuggingKeyboard);
                        return;
                    }
                    if (preparedMessage.Contains("отправить контакт"))
                    {
                        await bot.SendTextMessageAsync(id, "Жду!", replyMarkup: Keyboards.WelcomeKeyboard);
                        return;
                    }

                    if (preparedMessage.Contains("/stats"))
                    {
                        try
                        {
                            var _text = update.Message.Text;
                            var _id = update.Message.Chat.Id;
                            var _username = update.Message.Chat.FirstName;
                           // var _phonenumber = userPhone;
                            Console.WriteLine($"{_username} | {_id} | {_text} | {"null"/*_phonenumber*/}");
                        }
                        catch
                        {
                            Console.WriteLine("Возникло исключение!");
                            return;
                        }

                        await bot.SendTextMessageAsync(id, $"Статистика: \nbool - isSendContact: {IsSendContact}, Phone {/*userPhone ?? */"null"}");
                        return;
                    }

                    else
                    {
                        await bot.SendTextMessageAsync(id, "Не совсем понял вас. (возможно раздел еще в разработке)", replyMarkup: Keyboards.DefaultKeyboard);
                        return;
                    }


                }

                if (update.Message.Type == MessageType.Photo)
                {
                    await bot.SendTextMessageAsync(id, "Гружу...");
                    DownloaManager.DownloadDocument(bot, message, update.Message.Type);
                    return;
                }

                if (update.Message.Type == MessageType.Document)
                {
                    DownloaManager.DownloadDocument(bot, message, update.Message.Type);
                    return;
                }

                if (update.Message.Type == MessageType.Voice)
                {
                    DownloaManager.DownloadDocument(bot, message, update.Message.Type);
                    await bot.SendStickerAsync(id, Stikers.stiker1.Stiker_ID);
                    await Task.Delay(500);
                    await bot.SendTextMessageAsync(id, $"{firstName}, я еще не умею обрабатывать такие команды, но уже учусь!");
                    return;
                }

                if (update.Message.Type == MessageType.Sticker)
                {
                    await bot.SendTextMessageAsync(id, $"{firstName}, лови айдишник стика!\n {update.Message.Sticker.FileId}");
                    return;
                }

                if (update.Message.Type == MessageType.Contact) //&& !isSendContact
                {
                    var userPhone = message.Contact.PhoneNumber;

                    var model = new UserModel(id, userName, userPhone, true);
                    await JsonHelper.SaveUser(model);

                    IsSendContact = true;
                    
                    await bot.SendTextMessageAsync(id, $"Спасибо, {firstName}, ваш  номер +{userPhone} записан! ");
                    await Task.Delay(1000);
                    await bot.SendTextMessageAsync(id, "Теперь выберите необходимый пункт меню, я постарюсь вам помочь!");
                    await Task.Delay(1000);
                    await bot.SendStickerAsync(id, Stikers.stiker3.Stiker_ID, replyMarkup: Keyboards.DefaultKeyboard);
                    return;
                }
                else
                {
                    await bot.SendTextMessageAsync(id, $"Спасибо, {firstName}, но вы уже делились контактом. Все записано, не переживайте!");
                    await bot.SendStickerAsync(id, Stikers.stiker2.Stiker_ID, replyMarkup: Keyboards.DefaultKeyboard);
                    return;
                }

            }
        }


}
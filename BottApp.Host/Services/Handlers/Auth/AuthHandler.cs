using BottApp.Database.User;
using BottApp.Host.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BottApp.Host.Services.Handlers.Auth
{
    public class AuthHandler : IAuthHandler
    {
        private bool _isSendPhone = false;
        private bool _isSendLastName = false;
        private bool _isSendFirstName = false;
        private bool _isAllDataGrip;
        
        private readonly IUserRepository _userRepository;


        public AuthHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }


        public async Task BotOnMessageReceivedVotes(
            ITelegramBotClient botClient,
            Message message,
            CancellationToken cancellationToken
        )
        {
            await RequestContactAndLocation(botClient, message, cancellationToken);
        }


        public async Task BotOnMessageReceived(
            ITelegramBotClient botClient,
            Message message,
            CancellationToken cancellationToken,
            UserModel user,
            long AdminChatID
        )
        {
            if (message.Text == "/start" && !_isAllDataGrip)
            {
                await RequestContactAndLocation(botClient, message, cancellationToken);
                return;
            }

            if (message.Text == "/secretRestart")
            {
                await RequestContactAndLocation(botClient, message, cancellationToken);
                _isSendLastName = false;
                _isSendFirstName = false;
                _isSendPhone = false;
                _isAllDataGrip = false;
                return;
            }

            if (_isAllDataGrip)
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id, text: "Ваши данные на проверке, не переживайте!",
                    replyMarkup: Keyboard.RequestLocationAndContactKeyboard, cancellationToken: cancellationToken
                );
            }


            if ((message.Contact != null && !_isSendPhone))
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id, text: "Cпасибо!\nТеперь отправь свое имя",
                    replyMarkup: new ReplyKeyboardRemove(), cancellationToken: cancellationToken
                );
                
                await _userRepository.UpdateUserPhone(user, message.Contact.PhoneNumber);
                
                _isSendPhone = true;
                return;
            }

            if (!_isSendFirstName && _isSendPhone)
            {
                if (message.Text != null)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id, text: "Cпасибо!\nТеперь отправь фамилию",
                        replyMarkup: new ReplyKeyboardRemove(), cancellationToken: cancellationToken
                    );
                    await _userRepository.UpdateUserFirstName(user, message.Text);
                
                    _isSendFirstName = true;
                    return;
                }

                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id, text: "Отправьте имя в виде текста",
                    replyMarkup: new ReplyKeyboardRemove(), cancellationToken: cancellationToken
                );
                return;
            }

            if (!_isSendLastName && _isSendPhone)
            {
                if (message.Text != null)
                {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Отлично!\nПередал заявку на модерацию.\nОжидай уведомление :)",
                    replyMarkup: new ReplyKeyboardRemove(), cancellationToken: cancellationToken
                );
                
                await _userRepository.UpdateUserLastName(user, message.Text);
                
                _isSendLastName = true;
                _isAllDataGrip = true;
                
                await SendUserFormToAdmin(botClient, message, user, AdminChatID); 
                return;
                }

                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id, text: "Отправьте фамилию в виде текста",
                    replyMarkup: new ReplyKeyboardRemove(), cancellationToken: cancellationToken
                );
            }
        }


        public async Task RequestContactAndLocation(
            ITelegramBotClient botClient,
            Message? message,
            CancellationToken cancellationToken
        )
        {
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Привет! Мне необходимо собрать некоторую информацию, чтобы я мог идентифицировать тебя.",
                cancellationToken: cancellationToken
            );

            await Task.Delay(750);

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Не переживай! Какие-либо данные не передаются третьим лицам и хранятся на безопасном сервере.",
                cancellationToken: cancellationToken
            );

            await Task.Delay(1500);

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id, text: "Для начала нажми на кнопку\n 'Поделиться контактом'",
                replyMarkup: Keyboard.RequestLocationAndContactKeyboard, cancellationToken: cancellationToken
            );
        }


        public async Task SendUserFormToAdmin(
            ITelegramBotClient botClient,
            Message? message,
            UserModel user,
            long AdminChatID
        )
        {
            
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id, text: $"FirstName {user.FirstName}\nLastName {user.LastName}\nPhone { user.Phone}" );

                //TODO: Фотография может быть null, тогда неоходимо вставлять фото-заглушку
           /* var getPhotoAsync = botClient.GetUserProfilePhotosAsync(message.Chat.Id);

            var photo = getPhotoAsync.Result.Photos[0];

            await botClient.SendPhotoAsync(
                AdminChatID, photo[0].FileId,
                $" Пользователь |{message.Chat.FirstName}|\n" + $" @{message.From.Username} |{message.From.Id}|\n" +
                $" Моб.тел. |{message.Contact.PhoneNumber}|\n" + $" Хочет авторизоваться в системе " +
                $"{message.Caption}", replyMarkup: Keyboard.ApproveDeclineKeyboardMarkup
            );*/
        }
    }
}
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace BlazorApp1.Services
{
    // TelegramBotService class
    public class TelegramBotService
    {
        private readonly TelegramBotClient _botClient;

        public TelegramBotService(string token)
        {
            _botClient = new TelegramBotClient(token);
        }

        public async Task StartAsync()
        {
            // Настройки получения обновлений
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // Получать все типы обновлений
            };

            _botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken: default);

            var botMe = await _botClient.GetMeAsync();
            Console.WriteLine($"Запущен бот @{botMe.Username}");
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is { Text: { } } message)
            {
                Console.WriteLine($"Получено сообщение: {message.Text}");

                var keyboard = new InlineKeyboardMarkup(new[]
                {
                InlineKeyboardButton.WithCallbackData("Start", "start"),
                InlineKeyboardButton.WithCallbackData("Добавить файл", "add_file")
            });

                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Выберите действие:",
                    replyMarkup: keyboard,
                    cancellationToken: cancellationToken
                );
            }
            else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
            {
                var callbackQuery = update.CallbackQuery;

                if (callbackQuery.Data == "start")
                {
                    await botClient.SendTextMessageAsync(
                        chatId: callbackQuery.Message.Chat.Id,
                        text: "Вы начали работу",
                        cancellationToken: cancellationToken
                    );
                }
                else if (callbackQuery.Data == "add_file")
                {
                    await botClient.SendTextMessageAsync(
                        chatId: callbackQuery.Message.Chat.Id,
                        text: "Пожалуйста, загрузите файл",
                        cancellationToken: cancellationToken
                    );
                }
            }
            else if (update.Message is { Document: { } } fileMessage)
            {
                var fileId = fileMessage.Document.FileId;
                var fileInfo = await botClient.GetFileAsync(fileId, cancellationToken);

                Console.WriteLine($"Получен файл: {fileMessage.Document.FileName}");

                await botClient.SendTextMessageAsync(
                    chatId: fileMessage.Chat.Id,
                    text: $"Файл {fileMessage.Document.FileName} успешно получен.",
                    cancellationToken: cancellationToken
                );
            }
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Ошибка Telegram API:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }
    }

}

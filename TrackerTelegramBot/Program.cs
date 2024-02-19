using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TrackerTelegramBot
{
    class Program
    {
        private static TelegramBotClient botClient;
        private static CancellationTokenSource cts;
        
        static void Main()
        {
            #region Initialize

            botClient = new TelegramBotClient("6860611262:AAEdZsjfIkgHDRlxAZCezSVnyrt9idB9RSY");

            cts = new CancellationTokenSource();

            #endregion
            
            //Start receiving updates
            botClient.StartReceiving(HandleUpdateAsync, HandlePollingErrorAsync, cancellationToken: cts.Token);
            
            // Get bot information
            var me = botClient.GetMeAsync().Result;

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            // Send cancellation request to stop bot
            cts.Cancel();
        }
        
        static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Only process Message updates: https://core.telegram.org/bots/api#message
            if (update.Message == null)
                return;

            var message = update.Message;

            // Only process text messages
            if (message.Text == null)
                return;

            var chatId = message.Chat.Id;

            Console.WriteLine($"Received a '{message.Text}' message in chat {chatId}.");

            switch (message.Text)
            {
                case "/start":
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Welcome to your monitoring bot Enjoy several functionalities to track your wallet\n Enter menu to display options",
                        parseMode: ParseMode.MarkdownV2,
                        replyToMessageId: update.Message.MessageId,
                        cancellationToken: cancellationToken);
                    break;
                case "/menu":
                    await DisplayMenu(message.Chat);
                    break;
                case "/test":
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "You said:\n" + message.Text,
                        cancellationToken: cancellationToken);
                    break;
            }
        }
        
        static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }

        private static async Task DisplayMenu(ChatId chatId)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Your wallet", "option1"),
                    InlineKeyboardButton.WithCallbackData("Recent transactions", "option2"),
                    InlineKeyboardButton.WithCallbackData("Your NFT", "option2"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Stared currencies", "option3"),
                    InlineKeyboardButton.WithCallbackData("Settings", "option4"),
                    InlineKeyboardButton.WithCallbackData("Help", "option4"),
                }
            });

            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Wallet monitoring bot menu \n\n Select an option:",
                replyMarkup: keyboard
            );
        }
    }
}
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WSBotTele.Configs;

namespace WSBotTele.Processes
{
    interface IBotProcess
    {
        Task ListenIncommingMessage();
    }
    class BotProcess : IBotProcess
    {
        private readonly BotConfig _config;
        private static TelegramBotClient botClient;
        private static CancellationTokenSource cts;

        public BotProcess(IOptions<BotConfig> config)
        {
            _config = config.Value;
        }

        public async Task ListenIncommingMessage()
        {
            //khởi tạo bot
            Init(_config.Token);

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types: khi có tin nhắn mới là 1 update
            };

            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );
        }

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            string messageText = message.Text;
            var chatId = message.Chat.Id; //lấy id của phòng chat hiện tại

            string trimedMessageText = messageText.Trim().ToUpper();
            string replyMessageText = "Không có chi nhánh này";
            //if (BRANCHS.ContainsKey(trimedMessageText))
            //{
            //    replyMessageText = BRANCHS[trimedMessageText];
            //}

            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");
            Message sentMessage = await botClient.SendTextMessageAsync(
               chatId: chatId,
               text: "[Autorep]: " + replyMessageText,
               cancellationToken: cancellationToken
               );
        }

        Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        private void Init(string botToken)
        {
            botClient = new TelegramBotClient(botToken);
            cts = new CancellationTokenSource();
        }
    }
}

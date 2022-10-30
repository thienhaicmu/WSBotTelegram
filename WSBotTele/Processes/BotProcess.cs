using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;
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
using WSBotTele.Models;
using WSBotTele.Services;
using System.Collections.Generic;
using System.Linq;

namespace WSBotTele.Processes
{
    interface IBotProcess
    {
        Task ListenIncommingMessage();
    }
    class BotProcess : IBotProcess
    {
        private readonly IRedisClient _redis;
        private readonly IBotTask _botTask;
        private readonly BotConfig _config;
        private static TelegramBotClient botClient;
        private static CancellationTokenSource cts;

        public BotProcess(IOptions<BotConfig> config, IRedisClient redis, IBotTask botTask)
        {
            _config = config.Value;
            _redis = redis;
            _botTask = botTask;
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
            var UserName = message.Chat.FirstName;
            if (string.IsNullOrEmpty(messageText))
                messageText = "";
            string trimedMessageText = messageText.Trim().ToLower();
            //string replyMessageText = "Không có chi nhánh này";

            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

            if(trimedMessageText.Contains("rp"))
            {
                await botClient.SendTextMessageAsync(
                   chatId: chatId,
                   text: "[Autorep] kq: " + _botTask.Caculate(chatId, messageText),
                   cancellationToken: cancellationToken
                   );
            }
            else
            {
                await botClient.SendTextMessageAsync(
                   chatId: chatId,
                   text: "[Autorep]: " + _botTask.SaveData(chatId, UserName, messageText),
                   cancellationToken: cancellationToken
                   );
            }
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

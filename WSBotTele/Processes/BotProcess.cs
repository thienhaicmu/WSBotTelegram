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
        private readonly BotConfig _config;
        private static TelegramBotClient botClient;
        private static CancellationTokenSource cts;

        public BotProcess(IOptions<BotConfig> config, IRedisClient redis)
        {
            _config = config.Value;
            _redis = redis;
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

            string trimedMessageText = messageText.Trim().ToLower();
            //string replyMessageText = "Không có chi nhánh này";

            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

            if(trimedMessageText.Contains("sum"))
            {
                await botClient.SendTextMessageAsync(
                   chatId: chatId,
                   text: "[Autorep] SUM: " + Caculate(chatId, ""),
                   cancellationToken: cancellationToken
                   );
            }
            else
            {
                SaveData(chatId);
                await botClient.SendTextMessageAsync(
                   chatId: chatId,
                   text: "[Autorep] Save data to redis",
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

        private bool SaveData(long chatId)
        {
            List<BaseModel> l = _redis.GetValue<List<BaseModel>>("key1");
            if(l == null)
            {
                l = new List<BaseModel>();
            }
            l.Add(new BaseModel
            {
                ChatID = chatId,
                Location = "CN1",
                KeyA = "A",
                ValueA = 10,
                KeyB = "B",
                ValueB = 20,
            });
            return _redis.SetValue("key1", l);
        }
        private int Caculate(long chatID, string Location)
        {
            List<BaseModel> resListBase = _redis.GetValue<List<BaseModel>>("key1");
            if(resListBase == null)
            {
                return 0;
            }
            int result = 0;
            resListBase.ForEach(item =>
            {
                result += item.ValueA;
            });
            return result;
        }
    }
}

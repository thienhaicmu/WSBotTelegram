using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WSBotTele.Processes;

namespace WSBotTele.Worker
{
    class BotTelegramWorker : BackgroundService
    {
        private readonly IBotProcess _botProcess;
        private readonly ILogger<BotTelegramWorker> _logger;

        public BotTelegramWorker(IBotProcess botProcess, ILogger<BotTelegramWorker> logger)
        {
            _botProcess = botProcess;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _botProcess.ListenIncommingMessage();
        }
    }
}

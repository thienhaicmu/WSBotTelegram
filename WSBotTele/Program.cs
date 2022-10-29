using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WSBotTele.Configs;
using WSBotTele.Processes;
using WSBotTele.Services;
using WSBotTele.Worker;

namespace WSBotTele
{
    public class Program
    {
        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
               .ConfigureLogging(logging =>
               {
                   logging.ClearProviders();
                   logging.AddConsole();
                   logging.AddEventLog();
               })
               // Essential to run this as a window service
               .UseWindowsService()
               .ConfigureServices(configureServices);

        private static void configureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.Configure<BotConfig>(context.Configuration.GetSection("BotTelegam"));
            services.Configure<RedisConfig>(context.Configuration.GetSection("Redis"));
            services.AddLogging();
            services.AddSingleton<IBotProcess, BotProcess>();
            services.AddSingleton<IRedisClient, RedisClient>();
            services.AddHostedService<BotTelegramWorker>();
        }
    }
}

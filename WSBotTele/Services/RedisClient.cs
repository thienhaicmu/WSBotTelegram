using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using WSBotTele.Configs;

namespace WSBotTele.Services
{
    interface IRedisClient
    {
        bool SetValue<T>(string key, T values);
        T GetValue<T>(string key);
        bool Delete(string key);
    }
    class RedisClient : IRedisClient
    {
        private static RedisConfig _redisConfig;
        private static ConnectionMultiplexer _redisServer;
        private static IDatabase _multiDB;

        public RedisClient(IOptions<RedisConfig> options)
        {
            _redisConfig = options.Value;
            _redisServer = ConnectionMultiplexer.Connect(_redisConfig.Host);
            _multiDB = _redisServer.GetDatabase();
        }

        public bool SetValue<T>(string key, T values)
        {
            return _multiDB.StringSet(key: key, value: JsonConvert.SerializeObject(values), expiry: TimeSpan.FromDays(200));
        }
        public T GetValue<T>(string key)
        {
            RedisValue result = _multiDB.StringGet(key: key);
            return JsonConvert.DeserializeObject<T>(result.ToString());
        }
        public bool Delete(string key)
        {
            return _multiDB.KeyDelete(key: key);
        }
    }
}

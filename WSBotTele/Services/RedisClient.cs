using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace WSBotTele.Services
{
    interface IRedisClient
    {
        bool SetValue(string key, dynamic values);
        object GetValue(string key);
    }
    class RedisClient : IRedisClient
    {

        private static ConnectionMultiplexer redisServer = ConnectionMultiplexer.Connect("localhost:6379");
        private static IDatabase multiDB = redisServer.GetDatabase();

        public bool SetValue(string key, dynamic values)
        {
            return multiDB.StringSet(key: key, value: JsonConvert.SerializeObject(values), expiry: TimeSpan.FromSeconds(200));
        }
        public object GetValue(string key)
        {
            return multiDB.StringGet(key: key);
        }
    }
}

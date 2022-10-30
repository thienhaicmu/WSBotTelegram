using System;
using System.Collections.Generic;
using System.Text;

namespace WSBotTele.Configs
{
    class RedisConfig
    {
        public Dictionary<string, string> dKeys = new Dictionary<string, string>
        {
            {"key1", "new1valueofkey1" }
        };
        public string Host { get; set; }
    }
}

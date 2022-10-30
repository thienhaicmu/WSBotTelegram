using System;
using System.Collections.Generic;
using System.Text;

namespace WSBotTele.Configs
{
    class RedisConfig
    {
        public Dictionary<string, string> dKeys = new Dictionary<string, string>
        {
            {"key1", "new1valueofkey1" },
            {"key2", "new2valueofkey2" },
            {"key3", "new3valueofkey3" },
            {"key4", "new4valueofkey4" },
            {"key11", "new3valueofkey11" },
            {"key12", "new4valueofkey12" },
            {"key5", "new5valueofkey5" }
        };
        public string Host { get; set; }
    }
}

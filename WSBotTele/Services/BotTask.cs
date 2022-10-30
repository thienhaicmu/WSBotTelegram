using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WSBotTele.Configs;
using WSBotTele.Models;

namespace WSBotTele.Services
{
    interface IBotTask
    {
        string SaveData(long chatId, string message);
        string Caculate(long chatID, string Location);
    }
    class BotTask : IBotTask
    {
        private static IRedisClient _redis;
        private static RedisConfig _redisConfig;
        private string _key;
        private Dictionary<string, string> _dBranchs = new Dictionary<string, string>
        {
            { "HNI", "Hà Nội"},
            { "HCM", "Hồ Chí Minh"},
            { "LCI", "Lào Cai"},
            { "SLA", "Sơn La"},
            { "LSN", "Lạng Sơn"},
            { "CBG", "Cao Bằng"},
            { "TQG", "Tuyên Quang"},
            { "YBI", "Yên Bái"},
            { "NBH", "Ninh Bình"},
            { "HPG", "Hải Phòng"},
            { "QNH", "Quảng Ninh"},
            { "TBH", "Thái Bình"},
            { "THA", "Thanh Hóa"},
            { "NAN", "Nghệ An"},
            { "HTH", "Hà Tĩnh"},
            { "QBH", "Quảng Bình"},
            { "QTI", "Quảng Trị"},
            { "HUE", "Thừa Thiên Huế"},
            { "QNI", "Quảng Ngãi"},
            { "BDH", "Bình Định"},
            { "PYN", "Phú Yên"},
            { "KHA", "Khánh Hòa"},
            { "GLI", "Gia Lai"},
            { "KTM", "Kon Tum"},
            { "DNI", "Đồng Nai"},
            { "BTN", "Bình Thuận"},
            { "LDG", "Lâm Đồng"},
            { "VTU", "Bà Rịa - Vũng Tàu"},
            { "BDG", "Bình Dương"},
            { "TNH", "Tây Ninh"},
            { "DTP", "Đồng Tháp"},
            { "NTN", "Ninh Thuận"},
            { "VLG", "Vĩnh Long"},
            { "CTO", "Cần Thơ"},
            { "LAN", "Long An"},
            { "TGG", "Tiền Giang"},
            { "TVH", "Trà Vinh"},
            { "BTE", "Bến Tre"},
            { "AGG", "An Giang"},
            { "KGG", "Kiên Giang"},
            { "STG", "Sóc Trăng"},
            { "PTO", "Phú Thọ"},
            { "VPC", "Vĩnh Phúc"},
            { "HBH", "Hòa Bình"},
            { "HAG", "Hà Giang"},
            { "DBN", "Điện Biên"},
            { "LCU", "Lai Châu"},
            { "BGG", "Bắc Giang"},
            { "BNH", "Bắc Ninh"},
            { "TNN", "Thái Nguyên"},
            { "BKN", "Bắc Kạn"},
            { "HDG", "Hải Dương"},
            { "HYN", "Hưng Yên"},
            { "NDH", "Nam Định"},
            { "HNM", "Hà Nam"},
            { "DLK", "Đắk Lắk"},
            { "DKG", "Đắk Nông"},
            { "QNM", "Quảng Nam"},
            { "DNG", "Đà Nẵng"},
            { "BPC", "Bình Phước"},
            { "HGG", "Hậu Giang"},
            { "CMU", "Cà Mau"},
            { "BLU", "Bạc Liêu"},
            { "FPY", "FoxPay"},
            { "PPH", "Phnom Penh"},
            { "KDL", "Kandal"},
            { "KPC", "Kampongcham"},
            { "BBG", "Battam Bang"},
            { "SRP", "Siem Reap"},
            { "SVE", "Sihanouk Ville"},
            { "BMY", "Banteay Meanchey"},
            { "KTM1", "Kampong Thom"},
        };
        public BotTask(IRedisClient redis, IOptions<RedisConfig> option)
        {
            _redis = redis;
            _redisConfig = option.Value;
            _key = _redisConfig.dKeys["key1"];
        }
        public string SaveData(long chatId, string message)
        {
            try
            {
                //check null on redis
                List<BaseModel> l = _redis.GetValue<List<BaseModel>>(_key);
                if (l == null)
                {
                    l = new List<BaseModel>();
                }
                
                //convert from string to BaseModel
                List<BaseModel> lNew = GetListBaseModelFromMessage(chatId, message);
                l.AddRange(lNew);
                
                //save object BaseModel to Redis
                bool isSaveSuccess = _redis.SetValue(_key, l);

                return isSaveSuccess ? "Save success" : "Save fail";
            }
            catch
            {
                return "Dữ liệu đầu vào sai định dạng";
                throw;
            }
        }
        public string Caculate(long chatID, string message)
        {
            try
            {
                //create string result for response
                string res = "";

                List<BaseModel> l = _redis.GetValue<List<BaseModel>>(_key);
                if (l == null) return "Không có data";
                //fake message: "sum:cn1"
                //get location
                var gr = l.GroupBy(c => new
                {
                    c.ChatID,
                    c.Location,
                    c.Type,
                    c.Key
                })
                    .Select(p => new BaseModel
                    {
                        ChatID = p.Key.ChatID,
                        Location = p.Key.Location,
                        Type = p.Key.Type,
                        Key = p.Key.Key,
                        Value = p.Sum(z => z.Value),
                    });

                foreach (var item in gr)
                {
                    string lineFormat = @"
                    @CN
                    @Type: @Key @Value
                ==============================
                ";
                    string tmp = lineFormat
                        .Replace("@CN", item.Location)
                        .Replace("@Type", item.Type)
                        .Replace("@Key", item.Key.ToString())
                        .Replace("@Value", item.Value.ToString());

                    res += tmp;
                }
                return res;
            }
            catch 
            {
                return "Dữ liệu đầu vào sai định dạng";
                throw;
            }
        }

        private List<BaseModel> GetListBaseModelFromMessage(long chatId, string message)
        {
            //create list result for response
            List<BaseModel> l = new List<BaseModel>();

            //check null message
            if (string.IsNullOrEmpty(message)) return null;

            //get line by line
            string[] arrSplit = message.Split("\n");
            
            //get location, if not exist return
            string location = arrSplit[0].Trim();
            if (!_dBranchs.ContainsKey(location.ToUpper())) return null;


            for (int i = 1; i < arrSplit.Length; i++)
            {
                //create result variable of function
                BaseModel result = new BaseModel();
                result.ChatID = chatId;
                result.Location = location;
                
                //get type 
                string type = arrSplit[i].Split(":")[0];
                result.Type = type.Trim();

                //get key & value
                string strKeyValue = arrSplit[i].Split(":")[1];
                int.TryParse(strKeyValue.Split("x")[0], out int key);
                int.TryParse(strKeyValue.Split("x")[1], out int value);
                result.Key = key;
                result.Value = value;

                l.Add(result);
            }

            return l;
        }
    }
}

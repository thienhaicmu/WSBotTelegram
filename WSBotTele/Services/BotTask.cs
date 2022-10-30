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
        string SaveData(long chatId, string UserName, string message);
        string Caculate(long chatID, string Location);
    }
    class BotTask : IBotTask
    {
        private static IRedisClient _redis;
        private static RedisConfig _redisConfig;
        private string _key;
        private int keyarr;
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
            _key = _redisConfig.dKeys["key11"];
        }
        public string SaveData(long chatId, string UserName, string message)
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
                List<BaseModel> lNew = GetListBaseModelFromMessage(chatId, UserName, message);
                l.AddRange(lNew);

                //save object BaseModel to Redis
                bool isSaveSuccess = _redis.SetValue(_key, l);

                return isSaveSuccess ? "OK!" : "Không lưu được!";
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
                    c.UserName,
                    c.Location,
                    c.Type,
                    c.Key
                })
                    .Select(p => new BaseModel
                    {
                        UserName = p.Key.UserName,
                        Location = p.Key.Location,
                        Type = p.Key.Type,
                        Key = p.Key.Key,
                        Value = p.Sum(z => z.Value),
                    });

                foreach (var item in gr)
                {
                    string lineFormat = @"
                    @CN
                    @UserName (UserID)
                    @Type : số: @Key - tổng tiền: @Value
                ==============================
                ";
                    string tmp = lineFormat
                        .Replace("@CN", item.Location)
                        .Replace("@UserName", item.UserName.ToString())
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

        private List<BaseModel> GetListBaseModelFromMessage(long chatId, string UserName, string message)
        {
            //create list result for response
            List<BaseModel> l = new List<BaseModel>();

            //check null message
            if (string.IsNullOrEmpty(message)) return null;
            string[] arrSplit = { };
            //get line by line
            if (message.Contains("\n"))
            {
                arrSplit = message.Split("\n");
            }
            if (message.Contains(" "))
            {
                arrSplit = message.Split(" ");
            }

            //get location, if not exist return
            string location = arrSplit[0].Trim();
            if (!_dBranchs.ContainsKey(location.ToUpper())) return null;

            string arrValue = "";

            for (int i = 1; i < arrSplit.Length; i++)
            {
                //create result variable of function
                BaseModel result = new BaseModel();
                result.ChatID = chatId;
                result.UserName = UserName;
                result.Location = location;
                string[] arrType = { };
                string type = "";
                string strKeyValue = "";
                string[] arrKeyValue = { };




                //get key & value
                if ((arrSplit[i].Contains("x") && !arrSplit[i].Contains(":")) && (arrSplit[i].Contains("x") && !arrSplit[i].Contains(".")))
                {
                    arrType = arrSplit[i].Split("x");
                    arrValue = arrType[1].ToLower();
                    type = arrType[0].ToLower();
                    result.Type = type.Trim().ToLower();
                    strKeyValue = arrType[1];
                }
                else
                {
                    if (arrSplit[i].Contains(":"))
                        arrType = arrSplit[i].Split(":");
                    else
                   if (arrSplit[i].Contains("."))
                        arrType = arrSplit[i].Split(".");
                    else
                   if (arrSplit[i].Contains(","))
                        arrType = arrSplit[i].Split(",");
                    //get type 
                    type = arrType[0].ToLower();
                    result.Type = type.Trim().ToLower();
                    strKeyValue = arrType[1];
                    arrValue = strKeyValue.Split("x")[1].ToLower();
                    if (!strKeyValue.Contains(","))
                        keyarr = Convert.ToInt32(strKeyValue.Split("x")[0]);
                }

                if (arrValue.Contains("n"))
                {
                    arrValue = arrValue.Replace("n", "");
                }
                if (arrValue.Contains("k"))
                {
                    arrValue = arrValue.Replace("k", "");
                }
                if (strKeyValue.Contains(","))
                {
                    arrKeyValue = strKeyValue.Split("x")[0].Split(",");
                    for (int j = 0; j < arrKeyValue.Length; j++)
                    {
                        keyarr = Convert.ToInt32(arrKeyValue[j]);
                        l.Add(new BaseModel() { ChatID = chatId, UserName = UserName, Location = location, Type = type.Trim(), Key = keyarr, Value = Convert.ToInt32(arrValue) });
                    }

                }
                else
                 if (strKeyValue.Contains("."))
                {
                    arrKeyValue = strKeyValue.Split("x")[0].Split(".");
                    for (int j = 0; j < arrKeyValue.Length; j++)
                    {
                        keyarr = Convert.ToInt32(arrKeyValue[j]);
                        l.Add(new BaseModel() { ChatID = chatId, UserName = UserName, Location = location, Type = type.Trim(), Key = keyarr, Value = Convert.ToInt32(arrValue) });
                    }

                }
                else
                {
                    int.TryParse(arrValue, out int value);
                    result.Key = keyarr;
                    result.Value = value;
                    l.Add(result);
                }

            }

            return l;
        }
    }
}

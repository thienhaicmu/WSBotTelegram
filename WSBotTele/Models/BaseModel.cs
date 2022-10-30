using System;
using System.Collections.Generic;
using System.Text;

namespace WSBotTele.Models
{
    class BaseModel
    {
        public long ChatID { get; set; }
        public string UserName { get; set; }
        public string Location { get; set; }
        public string Type { get; set; }
        public int Key { get; set; }
        public int Value { get; set; }
    }
}

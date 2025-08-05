using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Easy.Notifications.Providers.Telegram
{
    public class TelegramConfiguration
    {
        public string BotToken { get; set; }
        public string ChatId { get; set; };
    }
}

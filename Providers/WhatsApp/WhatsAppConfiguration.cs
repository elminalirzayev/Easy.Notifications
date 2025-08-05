using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Easy.Notifications.Providers.WhatsApp
{
    public class WhatsAppConfiguration
    {
        public string AccountSid { get; set; }
        public string AuthToken { get; set; }
        public string Sender { get; set; }
    }
}

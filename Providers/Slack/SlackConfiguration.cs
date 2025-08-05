using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Easy.Notifications.Providers.Slack
{
    public class SlackConfiguration
    {
        public string WebhookUrl { get; set; }
        public string Channel { get; set; } // Opsiyonel
    }

}

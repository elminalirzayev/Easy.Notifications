using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Easy.Notifications.Providers.Slack
{
    public class SlackBlock
    {
        public string type { get; set; }
        public SlackText text { get; set; }
        public List<SlackElement> elements { get; set; }
    }

    public class SlackText
    {
        public string type { get; set; }
        public string text { get; set; }
    }

    public class SlackElement
    {
        public string type { get; set; }
        public SlackText text { get; set; }
        public string url { get; set; }
    }
}

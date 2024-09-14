using DestinyNews.Common;
using System.Text;

namespace DestinyNewsMonitorSummerizer
{
    public abstract class AbstractMessageSender
    {
        public abstract Task SendMessage(NewsEntry newsEntry, StringBuilder summarySb);
    }
}

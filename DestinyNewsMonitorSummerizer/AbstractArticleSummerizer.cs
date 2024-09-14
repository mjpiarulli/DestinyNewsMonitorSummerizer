using System.Text;

namespace DestinyNewsMonitorSummerizer
{
    public abstract class AbstractArticleSummerizer
    {
        public abstract Task<StringBuilder> SummerizeText(string text);
    }
}

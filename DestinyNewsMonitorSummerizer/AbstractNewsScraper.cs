using DestinyNews.Common;
using System.Text;

namespace DestinyNewsMonitorSummerizer
{
    public abstract class AbstractNewsScraper
    {
        protected abstract Task<NewsEntries> GetNewsEntries(string newsEndPoint);
        protected abstract Task<string> GetNewsArticleText(string articleUrl);
        protected virtual Func<NewsEntry, bool> NewsEntriesFilter => ne => ne.DateTime > DateTime.Now.AddDays(-1);
        protected virtual Func<NewsEntry, object> NewsEntriesOrderBy => ne => ne.DateTime;
        protected abstract Task<bool> ShouldSummarizeArticle(NewsEntry entry);
        protected abstract Task<StringBuilder> SummerizeArticle(string articleText);
        protected abstract StringBuilder AppendArticleLinkToSummary(StringBuilder summarySb, NewsEntry newsEntry);
        protected abstract Task SendSummary(NewsEntry newsEntry, StringBuilder summarySb);

        public virtual async Task GetSummarizeAndSendArticleSummaries(string newsEndPoint)
        {
            var newsEntries = await GetNewsEntries(newsEndPoint);
            var recentEntries = newsEntries.Entries.Where(NewsEntriesFilter).OrderBy(NewsEntriesOrderBy).ToList();
            foreach (var newsEntry in recentEntries)
            {
                if (!await ShouldSummarizeArticle(newsEntry))
                    continue;

                var text = await GetNewsArticleText(newsEntry.Url.HostedUrl);

                var summarySb = await SummerizeArticle(text);
                summarySb = AppendArticleLinkToSummary(summarySb, newsEntry);
                
                await SendSummary(newsEntry, summarySb);                
            }
        }

        protected async virtual Task<string> GetPageHtml(string newsEndPoint)
        {            
            using (var httpClient = new HttpClient())
            {

                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(newsEndPoint),
                };

                var response = await httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                return content;
            }
        }

    }
}

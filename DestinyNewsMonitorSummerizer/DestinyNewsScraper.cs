using Data;
using DestinyNews.Common;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace DestinyNewsMonitorSummerizer
{
    public class DestinyNewsScraper : AbstractNewsScraper
    {
        private readonly AbstractArticleSummerizer m_summerizer;
        private readonly AbstractMessageSender m_messageSender;
        private readonly GenericCosmosDbRepository<DestinyNews.Data.NewsEntry> m_repository;
        private readonly string m_destinyCdnAccessToken;
        private readonly string m_destinyCdnApiKey;

        private const string DESTINY_CDN_ARTICLE_BASE = "https://cdn.contentstack.io/v3/content_types/news_article/entries/?query=%7B%22url.hosted_url%22%3A%22{0}%22%7D&locale=en-us&environment=live";
        private const string BUNGIE_ARTICLE_URL_BASE = "https://www.bungie.net/7/en/News/article{0}";

        public DestinyNewsScraper(AbstractArticleSummerizer summerizer, AbstractMessageSender messageSender, GenericCosmosDbRepository<DestinyNews.Data.NewsEntry> repository,
            string destinyCdnAccessToken, string destinyCdnApiKey) 
        {
            m_summerizer = summerizer;
            m_messageSender = messageSender;
            m_repository = repository;
            m_destinyCdnAccessToken = destinyCdnAccessToken;
            m_destinyCdnApiKey = destinyCdnApiKey;
        }
        protected override async Task<string> GetPageHtml(string newsEndPoint)
        {
            using (var httpClient = new HttpClient())
            {

                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(newsEndPoint),
                };
                request.Headers.Add("Access_token", m_destinyCdnAccessToken);
                request.Headers.Add("Api_key", m_destinyCdnApiKey);

                var response = await httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                return content;
            }
        }
        protected override async Task<NewsEntries> GetNewsEntries(string newsEndPoint)
        {            
            var content = await GetPageHtml(newsEndPoint);
            var newsEntries = JsonConvert.DeserializeObject<NewsEntries>(content);

            return newsEntries;
        }

        protected override async Task<string> GetNewsArticleText(string articleUrl)
        {
            var urlEncodedPath = HttpUtility.UrlEncode(articleUrl);
            var fullUrl = string.Format(DESTINY_CDN_ARTICLE_BASE, urlEncodedPath);
            var content = await GetPageHtml(fullUrl);            

            var articles = JsonConvert.DeserializeObject<Articles>(content);
            var article = articles.Article.FirstOrDefault();

            var doc = new HtmlDocument();
            doc.LoadHtml(article.HtmlContent);
            var text = doc.DocumentNode.InnerText;
            text = Regex.Replace(text, @"\s+", " ").Trim();
            text = text.Replace("&nbsp;", " ");

            return text;
        }

        protected override async Task<bool> ShouldSummarizeArticle(NewsEntry entry)
        {
            var dataEntry = DestinyNews.Data.NewsEntry.ToData(entry);
            var exists = await m_repository.Exists(dataEntry, new Microsoft.Azure.Cosmos.PartitionKey(dataEntry.Id));

            return !exists;
        }

        protected override async Task<StringBuilder> SummerizeArticle(string articleText)
        {
            var summerySb = await m_summerizer.SummerizeText(articleText);

            return summerySb;
        }

        protected override StringBuilder AppendArticleLinkToSummary(StringBuilder summarySb, NewsEntry newsEntry)
        {
            summarySb.AppendLine();
            var url = string.Format(BUNGIE_ARTICLE_URL_BASE, newsEntry.Url.HostedUrl);
            summarySb.AppendLine($"[Link to Article]({url})");

            return summarySb;
        }

        protected override async Task SendSummary(NewsEntry newsEntry, StringBuilder summarySb)
        {
            await m_messageSender.SendMessage(newsEntry, summarySb);
        }
    }
}

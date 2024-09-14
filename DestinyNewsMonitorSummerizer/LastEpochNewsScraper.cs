using Data;
using DestinyNews.Common;
using HtmlAgilityPack;
using System.Text;
using System.Text.RegularExpressions;

namespace DestinyNewsMonitorSummerizer
{
    public class LastEpochNewsScraper : AbstractNewsScraper
    {
        private const string DEFAULT_BANNER_IMAGE_URL = "https://forum.lastepoch.com/uploads/default/original/2X/4/485f366a543d75d290539bb44f67e2ab7453a0e6.png";
        private readonly AbstractArticleSummerizer m_summerizer;
        private readonly AbstractMessageSender m_messageSender;
        private readonly GenericCosmosDbRepository<DestinyNews.Data.NewsEntry> m_repository;

        public LastEpochNewsScraper(AbstractArticleSummerizer summerizer, AbstractMessageSender messageSender, GenericCosmosDbRepository<DestinyNews.Data.NewsEntry> repository)
        {
            m_summerizer = summerizer;
            m_messageSender = messageSender;
            m_repository = repository;
        }

        protected override async Task<NewsEntries> GetNewsEntries(string newsEndPoint)
        {
            var content = await base.GetPageHtml(newsEndPoint);
            
            var doc = new HtmlDocument();
            doc.LoadHtml(content);
            var topicNodes = doc.DocumentNode.SelectNodes("//tr[contains(@class, 'topic-list-item')]").ToList();

            var newsEntries = new NewsEntries();
            foreach(var topicNode in topicNodes )
            {
                var titleLinkNode = topicNode.Descendants().FirstOrDefault(d => d.HasClass("title"));
                var titleLinkUrl = titleLinkNode!.Attributes["href"].Value;
                if (titleLinkNode is null)
                    continue;
                var newsEntry = new DestinyNews.Common.NewsEntry
                {
                    UniqueId = titleLinkUrl.Split("/").Last(),
                    BannerImage = GetDefaultBannerImage(),
                    DateTime = ParseNewsEntryDateTime(topicNode.Descendants().Last(n => n.Name == "td").InnerText),
                    Subtitle = string.Empty,
                    Title = titleLinkNode.InnerText,
                    Url = new DestinyNews.Common.DestinyNewsUrl { HostedUrl = titleLinkUrl }
                };

                newsEntries.Entries.Add(newsEntry);
            }

            return newsEntries;
        }

        private static DateTime ParseNewsEntryDateTime(string titleDateTime)
        {
            titleDateTime = titleDateTime.Trim();
            if(DateTime.TryParse(titleDateTime, out var dateTime))
                return dateTime;

            return DateTime.MinValue;
        }

        private static DestinyNews.Common.BannerImage GetDefaultBannerImage()
        {
            return new DestinyNews.Common.BannerImage
            {
                UniqueId = string.Empty,
                Url = DEFAULT_BANNER_IMAGE_URL
            };
        }

        protected override async Task<string> GetNewsArticleText(string articleUrl)
        {
            var content = await base.GetPageHtml(articleUrl);            

            var doc = new HtmlDocument();
            doc.LoadHtml(content);
            var text = doc.DocumentNode.SelectSingleNode("//div[contains(@itemprop, 'articleBody')]").InnerText;
            text = Regex.Replace(text, @"\s+", " ").Trim();
            text = text.Replace("&nbsp;", " ");

            return text;
        }
        protected override async Task<bool> ShouldSummarizeArticle(DestinyNews.Common.NewsEntry entry)
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

        protected override StringBuilder AppendArticleLinkToSummary(StringBuilder summarySb, DestinyNews.Common.NewsEntry newsEntry)
        {
            summarySb.AppendLine();
            summarySb.AppendLine($"[Link to Article]({newsEntry.Url.HostedUrl})");

            return summarySb;
        }

        protected override async Task SendSummary(DestinyNews.Common.NewsEntry newsEntry, StringBuilder summarySb)
        {
            await m_messageSender.SendMessage(newsEntry, summarySb);
        }             
    }
}

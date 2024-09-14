using Newtonsoft.Json;

namespace DestinyNews.Data
{
    public class NewsEntry
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public BannerImage BannerImage { get; set; }
        public DateTime DateTime { get; set; }
        public string Subtitle { get; set; }
        public string Title { get; set; }
        public DestinyNewsUrl Url { get; set; }

        public static NewsEntry ToData(Common.NewsEntry ne)
        {
            return new NewsEntry
            {
                Id = ne.UniqueId,
                BannerImage = BannerImage.ToData(ne.BannerImage),
                DateTime = ne.DateTime,
                Subtitle = ne.Subtitle,
                Title = ne.Title,
                Url = DestinyNewsUrl.ToData(ne.Url)
            };
        }

    }
}

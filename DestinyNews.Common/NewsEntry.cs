using Newtonsoft.Json;

namespace DestinyNews.Common
{
    public class NewsEntry
    {
        public NewsEntry()
        {
            BannerImage = new BannerImage();
            Url = new DestinyNewsUrl();
        }

        [JsonProperty("uid")]
        public string UniqueId { get; set; }

        [JsonProperty("banner_image")]
        public BannerImage BannerImage { get; set; }

        [JsonProperty("date")]
        public DateTime DateTime { get; set; }

        [JsonProperty("subtitle")]
        public string Subtitle { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public DestinyNewsUrl Url { get; set; }
    }
}

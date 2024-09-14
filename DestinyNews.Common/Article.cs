using Newtonsoft.Json;

namespace DestinyNews.Common
{
    public class Article
    {
        [JsonProperty("uid")]
        public string UniqueId { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("html_content")]
        public string HtmlContent { get; set; }
    }
}

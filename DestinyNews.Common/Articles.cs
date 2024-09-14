using Newtonsoft.Json;

namespace DestinyNews.Common
{
    public class Articles
    {
        [JsonProperty("entries")]
        public List<Article> Article { get; set; }
    }
}

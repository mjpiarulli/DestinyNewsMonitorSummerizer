using Newtonsoft.Json;

namespace DestinyNews.Common
{
    public class BannerImage
    {
        [JsonProperty("uid")]
        public string UniqueId { get; set; }
        
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}

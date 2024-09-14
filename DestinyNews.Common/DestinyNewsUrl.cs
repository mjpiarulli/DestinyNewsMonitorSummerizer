using Newtonsoft.Json;

namespace DestinyNews.Common
{
    public class DestinyNewsUrl
    {
        [JsonProperty("hosted_url")]
        public string HostedUrl { get; set; }
    }
}

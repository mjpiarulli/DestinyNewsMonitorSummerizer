using Newtonsoft.Json;

namespace DestinyNews.Common
{
    
    public class NewsEntries
    {
        public NewsEntries() 
        { 
            Entries = new List<NewsEntry>();
        }

        [JsonProperty("entries")]
        public List<NewsEntry> Entries { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

    }
}

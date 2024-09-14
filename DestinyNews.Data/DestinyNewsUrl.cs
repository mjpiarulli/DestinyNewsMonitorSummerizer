namespace DestinyNews.Data
{
    public class DestinyNewsUrl
    {
        public string HostedUrl { get; set; }

        public static DestinyNewsUrl ToData(Common.DestinyNewsUrl dnu)
        {
            return new DestinyNewsUrl
            {
                HostedUrl = dnu.HostedUrl
            };
        }
    }
}

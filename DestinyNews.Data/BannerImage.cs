namespace DestinyNews.Data
{
    public class BannerImage
    {
        public string UniqueId { get; set; }
        public string Url { get; set; }

        public static BannerImage ToData(Common.BannerImage bi)
        {
            return new BannerImage
            {
                UniqueId = bi.UniqueId,                
                Url = bi.Url
            };
        }
    }
}

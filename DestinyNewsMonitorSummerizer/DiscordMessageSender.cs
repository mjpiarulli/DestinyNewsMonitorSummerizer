using Discord.Webhook;
using Discord;
using DestinyNews.Common;
using System.Text;

namespace DestinyNewsMonitorSummerizer
{
    public class DiscordMessageSender : AbstractMessageSender
    {
        private readonly string m_webhookUrl;

        public DiscordMessageSender(string webhookUrl)
        {
            this.m_webhookUrl = webhookUrl;
        }

        public override async Task SendMessage(NewsEntry newsEntry, StringBuilder summarySb)
        {
            using (var discordClient = new DiscordWebhookClient(m_webhookUrl))
            {
                var errorEmbed = new EmbedBuilder
                {
                    Title = newsEntry.Title,
                    ImageUrl = newsEntry.BannerImage.Url,
                    Description = summarySb.ToString()
                };

                // Webhooks are able to send multiple embeds per message
                // As such, your embeds must be passed as a collection. 
                await discordClient.SendMessageAsync(string.Empty, false, new[] { errorEmbed.Build() });
            }
        }
    }
}

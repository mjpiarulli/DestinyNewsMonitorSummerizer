using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Data;
using DestinyNews.Data;
using DestinyNewsMonitorSummerizer;
using Microsoft.Azure.Cosmos;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var keyVaultOptions = new SecretClientOptions
        {
            Retry =
            {
                Delay= TimeSpan.FromSeconds(2),
                MaxDelay = TimeSpan.FromSeconds(16),
                MaxRetries = 5,
                Mode = RetryMode.Exponential
            }
        };
        var client = new SecretClient(new Uri("https://destiny-news-monitor-kv.vault.azure.net/"), new DefaultAzureCredential(), keyVaultOptions);       

        var destinyNewsEndPoint = client.GetSecret("DestinyNewsEndPoint").Value.Value;
        var destinyCDNAccessToken = client.GetSecret("DestinyCDNAccessToken").Value.Value;
        var destinyCDNApiKey = client.GetSecret("DestinyCDNApiKey").Value.Value;
        var lastEpochAnnouncementsEndPoint = "https://forum.lastepoch.com/c/announcements/37";
        var lastEpochNewsEndPoint = "https://forum.lastepoch.com/c/news/64";
        var cosmosEndPoint = client.GetSecret("CosmosEndPoint").Value.Value;
        var cosmosKey = client.GetSecret("CosmosKey").Value.Value;
        var destinyDatabaseName = "DestinyNews";
        var destinyContainerName = "DestinyNewsEntries";
        var destinyPartitionKeyPath = "/id";
        var lastEpochDatabaseName = "LastEpochNews";
        var lastEpochContainerName = "LastEpochNewsEntries";
        var lastEpochPartitionKeyPath = "/id";
        var languageEndPoint = client.GetSecret("LanguageServiceEndPoint").Value.Value;
        var languageKey = client.GetSecret("LanguageServiceKey").Value.Value;        
        var cosmosClientOptions = new CosmosClientOptions
        {
            SerializerOptions = new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase },
        };        
        var destinyDiscordChannelWebHookUrl = client.GetSecret("DestinyDiscordChannelWebHookUrl").Value.Value;        
        var lastEpochDiscordChannelWebHookUrl = client.GetSecret("LastEpochDiscordChannelWebHookUrl").Value.Value;       

        var azureSummerizer = new AzureLanguageServiceSummerizer(languageKey, languageEndPoint);

        var leDiscordMessageSender = new DiscordMessageSender(lastEpochDiscordChannelWebHookUrl);
        var leCosmosDbRepository = new GenericCosmosDbRepository<NewsEntry>(cosmosEndPoint, cosmosKey, cosmosClientOptions,
            lastEpochDatabaseName, lastEpochContainerName, lastEpochPartitionKeyPath);
        var lastEpochNewsScraper = new LastEpochNewsScraper(azureSummerizer, leDiscordMessageSender, leCosmosDbRepository);


        var destinyDiscordMessageSender = new DiscordMessageSender(destinyDiscordChannelWebHookUrl);
        var destinyCosmosDbRepository = new GenericCosmosDbRepository<NewsEntry>(cosmosEndPoint, cosmosKey, cosmosClientOptions,
            destinyDatabaseName, destinyContainerName, destinyPartitionKeyPath);
        var destinyNewsScraper = new DestinyNewsScraper(azureSummerizer, destinyDiscordMessageSender, destinyCosmosDbRepository, destinyCDNAccessToken, destinyCDNApiKey);

        var startTimeSpan = TimeSpan.Zero;
        var periodTimeSpan = TimeSpan.FromMinutes(5);

        var timer = new Timer(async e =>
        {
            await destinyNewsScraper.GetSummarizeAndSendArticleSummaries(destinyNewsEndPoint);
            await lastEpochNewsScraper.GetSummarizeAndSendArticleSummaries(lastEpochAnnouncementsEndPoint);
            await lastEpochNewsScraper.GetSummarizeAndSendArticleSummaries(lastEpochNewsEndPoint);
        }, null, startTimeSpan, periodTimeSpan);

        Console.WriteLine("Done");
        Console.ReadKey();
    }
}
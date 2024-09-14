using Microsoft.Azure.Cosmos;

namespace Data
{
    public class GenericCosmosDbRepository<T>
    {
        private readonly string m_cosmosEndPoint;
        private readonly string m_cosmosKey;
        private readonly CosmosClientOptions m_options;
        private readonly string m_databaseName;
        private readonly string m_containerName;
        private readonly string m_partitionKeyPath;

        public GenericCosmosDbRepository(string cosmosEndPoint, string cosmosKey, CosmosClientOptions options, 
            string databaseName, string containerName, string partitionKeyPath) 
        {
            m_cosmosEndPoint = cosmosEndPoint;
            m_cosmosKey = cosmosKey;
            m_options = options;
            m_databaseName = databaseName;
            m_containerName = containerName;
            m_partitionKeyPath = partitionKeyPath;
        }
        public async Task<bool> Exists(T entity, PartitionKey partitionKey)
        {            
            using (var cosmosClient = new CosmosClient(m_cosmosEndPoint, m_cosmosKey, m_options))
            {
                var db = await cosmosClient.CreateDatabaseIfNotExistsAsync(m_databaseName);
                var container = await db.Database.CreateContainerIfNotExistsAsync(id: m_containerName, partitionKeyPath: m_partitionKeyPath);
                var upsertedItem = await container.Container.UpsertItemAsync(entity, partitionKey);

                return upsertedItem.StatusCode == System.Net.HttpStatusCode.OK;
            }
        }
    }
}

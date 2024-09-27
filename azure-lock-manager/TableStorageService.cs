public interface ITableStorageService
{
    Task AddEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity;
    Task<T?> GetEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity;
}

public class TableStorageService : ITableStorageService
{
    private readonly TableServiceClient _tableServiceClient;

    public TableStorageService(TableServiceClient tableServiceClient)
    {
        _tableServiceClient = tableServiceClient;
    }

    public async Task AddEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity
    {
        var tableClient = _tableServiceClient.GetTableClient(tableName);
        await tableClient.CreateIfNotExistsAsync();
        await tableClient.AddEntityAsync(entity);
    }

    public async Task<T?> GetEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity
    {
        var tableClient = _tableServiceClient.GetTableClient(tableName);
        try
        {
            var response = await tableClient.GetEntityAsync<T>(partitionKey, rowKey);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null; // Entity not found
        }
    }
}
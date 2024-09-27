using Azure.Data.Tables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;


var host = Host.CreateDefaultBuilder(args)
    .UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console())
    .ConfigureServices((context, services) =>
    {
        // Register the Azure Table Storage client
        string storageConnectionString = context.Configuration["AzureTableStorage:ConnectionString"];
        services.AddSingleton(new TableServiceClient(storageConnectionString));
        services.AddScoped<ITableStorageService, TableStorageService>();
    })
    .Build();

// Run the application
await host.RunAsync();

await host.StartAsync();
var logger = host.Services.GetRequiredService<ILogger<Program>>();
var tableStorageService = host.Services.GetRequiredService<ITableStorageService>();

string tableName = "SampleTable";

// Create a new entity
var newEntity = new SampleEntity
{
    PartitionKey = "samplePartition",
    RowKey = Guid.NewGuid().ToString(),
    Data = "Hello, Azure Table Storage!"
};

await tableStorageService.AddEntityAsync(tableName, newEntity);
logger.LogInformation("Added new entity to Azure Table Storage.");

// Retrieve the entity
var retrievedEntity = await tableStorageService.GetEntityAsync<SampleEntity>(tableName, newEntity.PartitionKey, newEntity.RowKey);
if (retrievedEntity != null)
{
    logger.LogInformation("Retrieved entity: {Data}", retrievedEntity.Data);
}
else
{
    logger.LogWarning("Entity not found.");
}

await host.StopAsync();

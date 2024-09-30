# Requirements

### "lookup" Table Schema

Partition Key --> Lookup Type [AppName]  
Row Key --> Name   

### "objects-to-delete-lock" Table Schema

Partition Key --> AppName - Environment [dev, stg, trn, prd]  
Row Key --> Azure type - name  


# Setup

### Step 1: Create a New Console Application

1. Open your terminal or command prompt.
2. Create a new .NET console application:

   ```bash
   dotnet new console -n AzureTableStorageApp
   cd AzureTableStorageApp
   ```

### Step 2: Add Required NuGet Packages

You will need the following packages:

```bash
dotnet add package Azure.Data.Tables
dotnet add package Serilog
dotnet add package Serilog.Extensions.Logging
dotnet add package Serilog.Sinks.Console
dotnet add package Moq
dotnet add package Microsoft.Extensions.DependencyInjection
dotnet add package Microsoft.Extensions.Hosting
dotnet add package Xunit
dotnet add package FluentAssertions
```

### Step 3: Set Up Dependency Injection and Logging

Edit the `Program.cs` file to configure dependency injection and logging.

```csharp
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
```

### Step 4: Create the Table Storage Service

Create a new class for your Azure Table Storage operations:

```csharp
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
```

### Step 5: Create Entity Class

Create an entity class that represents the data you want to store in Azure Table Storage.

```csharp
public class SampleEntity : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public string Data { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
```

### Step 6: Use the Service in Main Method

Modify the `Program.cs` to use the `ITableStorageService` for logging and interacting with Azure Table Storage:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
```

### Step 7: Configure Connection String

Add a configuration file (`appsettings.json`) for your connection string:

```json
{
  "AzureTableStorage": {
    "ConnectionString": "Your_Azure_Table_Storage_Connection_String_Here"
  }
}
```

### Step 8: Create Unit Tests

Create a new test project:

```bash
dotnet new xunit -n AzureTableStorageApp.Tests
cd AzureTableStorageApp.Tests
dotnet add reference ../AzureTableStorageApp/AzureTableStorageApp.csproj
dotnet add package Moq
dotnet add package FluentAssertions
```

Now, create a test file `TableStorageServiceTests.cs`:

```csharp
using Azure;
using Azure.Data.Tables;
using FluentAssertions;
using Moq;
using System.Threading.Tasks;
using Xunit;

public class TableStorageServiceTests
{
    private readonly Mock<TableServiceClient> _mockTableServiceClient;
    private readonly Mock<TableClient> _mockTableClient;
    private readonly TableStorageService _tableStorageService;

    public TableStorageServiceTests()
    {
        _mockTableServiceClient = new Mock<TableServiceClient>();
        _mockTableClient = new Mock<TableClient>();
        _mockTableServiceClient.Setup(c => c.GetTableClient(It.IsAny<string>()))
                               .Returns(_mockTableClient.Object);
        _tableStorageService = new TableStorageService(_mockTableServiceClient.Object);
    }

    [Fact]
    public async Task AddEntityAsync_Should_Add_Entity()
    {
        // Arrange
        var entity = new SampleEntity
        {
            PartitionKey = "partitionKey",
            RowKey = "rowKey",
            Data = "test data"
        };

        _mockTableClient.Setup(c => c.CreateIfNotExistsAsync())
                        .Returns(Task.CompletedTask);
        _mockTableClient.Setup(c => c.AddEntityAsync(entity, null, default))
                        .Returns(Task.CompletedTask);

        // Act
        await _tableStorageService.AddEntityAsync("SampleTable", entity);

        // Assert
        _mockTableClient.Verify(c => c.CreateIfNotExistsAsync(), Times.Once);
        _mockTableClient.Verify(c => c.AddEntityAsync(entity, null, default), Times.Once);
    }

    [Fact]
    public async Task GetEntityAsync_Should_Return_Entity_When_Exists()
    {
        // Arrange
        var entity = new SampleEntity
        {
            PartitionKey = "partitionKey",
            RowKey = "rowKey",
            Data = "test data"
        };

        var response = Response.FromValue(entity, new Mock<Response>().Object);

        _mockTableClient.Setup(c => c.GetEntityAsync<SampleEntity>("partitionKey", "rowKey", default))
                        .ReturnsAsync(response);

        // Act
        var result = await _tableStorageService.GetEntityAsync<SampleEntity>("SampleTable", "partitionKey", "rowKey");

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().Be("test data");
    }

    [Fact]
    public async Task GetEntityAsync_Should_Return_Null_When_Not_Found()
    {
        // Arrange
        _mockTableClient.Setup(c => c.GetEntityAsync<SampleEntity>("partitionKey", "rowKey", default))
                        .ThrowsAsync(new RequestFailedException(404, "Not Found"));

        // Act
        var result = await _tableStorageService.GetEntityAsync<SampleEntity>("SampleTable", "partitionKey", "rowKey");

        // Assert
        result.Should().BeNull();
    }
}
```

### Step 9: Run Your Application and Tests

Make sure your Azure Table Storage connection string is correct, then run your application:

```bash
dotnet run
```

To run your tests, navigate to the test project directory and run:

```bash
dotnet test
```




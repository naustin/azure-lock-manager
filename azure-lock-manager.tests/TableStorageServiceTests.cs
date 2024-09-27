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
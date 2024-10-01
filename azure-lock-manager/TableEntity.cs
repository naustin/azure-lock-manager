using Azure;
using Azure.Data.Tables;

public class SampleEntity : ITableEntity
{
    required public string PartitionKey { get; set; }
    required public string RowKey { get; set; }
    required public string Data { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
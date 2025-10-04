using System;
using Azure;
using Azure.Data.Tables;

namespace Uno.Playground.Api.Models;

public class SampleCategory : ITableEntity
{
	public SampleCategory()
	{
		PartitionKey = nameof(SampleCategory);
		ETag = ETag.All;
	}

	public SampleCategory(string id) : this()
	{
		RowKey = id;
	}

	public required string PartitionKey { get; set; }

	public required string RowKey { get; set; }

	public ETag ETag { get; set; }

	DateTimeOffset? ITableEntity.Timestamp { get; set; }

	public DateTimeOffset? Timestamp
	{
		get => ((ITableEntity)this).Timestamp;
		set => ((ITableEntity)this).Timestamp = value;
	}

	public string Id => RowKey;

	public required string Title { get; set; }

	public long ListingOrder { get; set; } = 0;

	public string? PathData { get; set; }

	public string? AccentPathData { get; set; }
}

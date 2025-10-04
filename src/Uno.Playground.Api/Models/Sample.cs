using System;
using Azure;
using Azure.Data.Tables;

namespace Uno.Playground.Api.Models;

public class Sample : ITableEntity
{
	public Sample()
	{
		PartitionKey = nameof(Sample);
		ETag = ETag.All;
	}

	public Sample(string id) : this()
	{
		RowKey = id;
	}

	public string PartitionKey { get; set; }

	public string? RowKey { get; set; }

	public ETag ETag { get; set; }

	DateTimeOffset? ITableEntity.Timestamp { get; set; }

	public DateTimeOffset? Timestamp
	{
		get => ((ITableEntity)this).Timestamp;
		set => ((ITableEntity)this).Timestamp = value;
	}

	// Convenience property to preserve previous API
	public string Id => RowKey!;

	public string? Category { get; set; }

	public string? Title { get; set; }

	public string? Description { get; set; }

	public string? Xaml { get; set; }

	public string? Data { get; set; }

	public string[] ParsedKeywords
	{
		get => Keywords?.Split(';') ?? [];
		set => Keywords = value == null ? "" : string.Join(";", value);
	}

	public string? Keywords { get; set; }

	public string? IpAddress { get; set; }

	public string? App { get; set; }

	public string? UserAgent { get; set; }

	public long ListingOrder { get; set; } = 0;

	public string? PathData { get; set; }

	public string? AccentPathData { get; set; }
}

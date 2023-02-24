using Azure;
using Azure.Data.Tables;
using System;
using System.Text.Json.Serialization;

namespace Uno.UI.Demo.Api.Models
{
	public class SampleCategory : ITableEntity
	{
		public SampleCategory()
		{
			PartitionKey = nameof(SampleCategory);
		}

		public SampleCategory(string id) : this()
		{
			RowKey = id;
			ETag = new("*");
		}
		public string PartitionKey { get; set; }
		public string RowKey { get; set; }
		public ETag ETag { get; set; }
		public DateTimeOffset? Timestamp { get; set; }

		[JsonIgnore]
		public string Id => RowKey;

		public string Title { get; set; }

		public long ListingOrder { get; set; } = 0;

		public string PathData { get; set; }

		public string AccentPathData { get; set; }
	}
}

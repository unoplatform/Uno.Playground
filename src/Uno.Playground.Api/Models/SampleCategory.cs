using System;
using System.Runtime.Serialization;
using Azure;
using Azure.Data.Tables;

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
			ETag = ETag.All;
		}

		public string PartitionKey { get; set; } = string.Empty;
		public string RowKey { get; set; } = string.Empty;
		public DateTimeOffset? Timestamp { get; set; }
		public ETag ETag { get; set; }

		[IgnoreDataMember]
		public string Id => RowKey;

		public string Title { get; set; } = string.Empty;

		public long ListingOrder { get; set; } = 0;

		public string PathData { get; set; } = string.Empty;

		public string AccentPathData { get; set; } = string.Empty;
	}
}
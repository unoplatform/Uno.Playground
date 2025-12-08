using System;
using System.Runtime.Serialization;
using Azure;
using Azure.Data.Tables;

namespace Uno.UI.Demo.Api.Models
{
	public class Sample : ITableEntity
	{
		public Sample()
		{
			PartitionKey = nameof(Sample);
		}

		public Sample(string id) : this()
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

		public string Category { get; set; } = string.Empty;

		public string Title { get; set; } = string.Empty;

		public string Description { get; set; } = string.Empty;

		public string Xaml { get; set; } = string.Empty;

		public string Data { get; set; } = string.Empty;

		[IgnoreDataMember]
		public string[] ParsedKeywords
		{
			get => Keywords?.Split(';') ?? new string[] {};
			set => Keywords = value == null ? "" : string.Join(";", value);
		}

		public string Keywords { get; set; } = string.Empty;

		public string IpAddress { get; set; } = string.Empty;

		public string App { get; set; } = string.Empty;

		public string UserAgent { get; set; } = string.Empty;

		public long ListingOrder { get; set; } = 0;

		public string PathData { get; set; } = string.Empty;

		public string AccentPathData { get; set; } = string.Empty;
	}
}

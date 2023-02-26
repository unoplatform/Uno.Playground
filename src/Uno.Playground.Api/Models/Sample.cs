using Azure;
using Azure.Data.Tables;
using System;
using System.Runtime.Serialization;

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
			ETag = new ETag("*");
		}

		public string PartitionKey { get; set; }
		public string RowKey { get; set; }
		public ETag ETag { get; set; }
		public DateTimeOffset? Timestamp { get; set; }

		[IgnoreDataMember]
		public string Id => RowKey;

		public string Category { get; set; }

		public string Title { get; set; }

		public string Description { get; set; }

		public string Xaml { get; set; }

		public string Data { get; set; }

		public string Code { get; set; }

		[IgnoreDataMember]
		public string[] ParsedKeywords
		{
			get => Keywords?.Split(';') ?? new string[] {};
			set => Keywords = value == null ? "" : string.Join(";", value);
		}

		public string Keywords { get; set; }

		public string IpAddress { get; set; }

		public string App { get; set; }

		public string UserAgent { get; set; }

		public long ListingOrder { get; set; } = 0;

		public string PathData { get; set; }

		public string AccentPathData { get; set; }
	}
}

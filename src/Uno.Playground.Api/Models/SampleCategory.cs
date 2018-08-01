using Microsoft.WindowsAzure.Storage.Table;

namespace Uno.UI.Demo.Api.Models
{
	public class SampleCategory : TableEntity
	{
		public SampleCategory()
		{
			PartitionKey = nameof(SampleCategory);
		}

		public SampleCategory(string id) : this()
		{
			RowKey = id;
			ETag = "*";
		}

		[IgnoreProperty]
		public string Id => RowKey;

		public string Title { get; set; }

		public long ListingOrder { get; set; } = 0;

		public string PathData { get; set; }

		public string AccentPathData { get; set; }
	}
}
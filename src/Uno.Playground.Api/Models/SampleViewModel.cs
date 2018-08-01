using System.Runtime.Serialization;

namespace Uno.UI.Demo.Api.Models
{
	public partial class SampleViewModel
	{
		private readonly Sample _sample;

		public SampleViewModel(Sample sample)
		{
			_sample = sample;
		}

		public string Id => _sample.Id;

		public string Title => _sample.Title ?? "(no title)";

		public string Description => _sample.Description ?? "";

		public string[] Keywords => _sample.ParsedKeywords ?? new string[] { };

		public string IconPath => _sample.PathData;

		public string IconAccentPath => _sample.AccentPathData;

		[IgnoreDataMember]
		public string Etag => _sample.ETag;
	}
}
using System.Runtime.Serialization;

namespace Uno.Playground.Api.Models;

public partial class SampleViewModel
{
	private readonly Sample _sample;

	public SampleViewModel(Sample sample)
	{
		_sample = sample;
	}

	public string Id => _sample.Id;

	public string Title => _sample.Title ?? "(no title)";

	public string Description => _sample.Description ?? string.Empty;

	public string[] Keywords => _sample.ParsedKeywords ?? [];

	public string? IconPath => _sample.PathData;

	public string? IconAccentPath => _sample.AccentPathData;

	[IgnoreDataMember]
	public string Etag => _sample.ETag.ToString() ?? string.Empty;
}

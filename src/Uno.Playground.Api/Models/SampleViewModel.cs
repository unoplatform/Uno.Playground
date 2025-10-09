using System;
using System.Runtime.Serialization;

namespace Uno.Playground.Api.Models;

public partial class SampleViewModel
{
	private readonly Sample? _sample;
	private string _id = string.Empty;
	private string _title = string.Empty;
	private string _description = string.Empty;
	private string[] _keywords = [];
	private string? _iconPath;
	private string? _iconAccentPath;
	private string _etag = string.Empty;

	public SampleViewModel()
	{
	}

	public SampleViewModel(Sample sample)
	{
		_sample = sample;
		_id = sample.Id;
		_title = sample.Title ?? "(no title)";
		_description = sample.Description ?? string.Empty;
		_keywords = sample.ParsedKeywords ?? [];
		_iconPath = sample.PathData;
		_iconAccentPath = sample.AccentPathData;
		_etag = sample.ETag.ToString() ?? string.Empty;
	}

	public string Id
	{
		get => _sample?.Id ?? _id;
		set => _id = value;
	}

	public string Title
	{
		get => _sample?.Title ?? "(no title)" ?? _title;
		set => _title = value;
	}

	public string Description
	{
		get => _sample?.Description ?? string.Empty ?? _description;
		set => _description = value;
	}

	public string[] Keywords
	{
		get => _sample?.ParsedKeywords ?? Array.Empty<string>() ?? _keywords;
		set => _keywords = value;
	}

	public string? IconPath
	{
		get => _sample?.PathData ?? _iconPath;
		set => _iconPath = value;
	}

	public string? IconAccentPath
	{
		get => _sample?.AccentPathData ?? _iconAccentPath;
		set => _iconAccentPath = value;
	}

	[IgnoreDataMember]
	public string Etag
	{
		get => _sample?.ETag.ToString() ?? string.Empty ?? _etag;
		set => _etag = value;
	}
}

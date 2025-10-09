using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uno.Playground.Api.Models;

public class SampleCategoryViewModel
{
	private readonly SampleCategory? _category;
	private string _categoryId = string.Empty;
	private string _title = string.Empty;
	private string? _defaultIconPath;
	private string? _defaultIconAccentPath;

	public SampleCategoryViewModel()
	{
	}

	public SampleCategoryViewModel(SampleCategory category, IEnumerable<Sample> samples)
	{
		_category = category;
		_categoryId = category.Id;
		_title = category.Title;
		_defaultIconPath = category.PathData;
		_defaultIconAccentPath = category.AccentPathData;
		Samples = samples
			.Where(s => string.Equals(s.Category, category.Id))
			.Select(s => new SampleViewModel(s)).ToArray();
	}

	public string CategoryId
	{
		get => _category?.Id ?? _categoryId;
		set => _categoryId = value;
	}

	public string Title
	{
		get => _category?.Title ?? _title;
		set => _title = value;
	}

	public string? DefaultIconPath
	{
		get => _category?.PathData ?? _defaultIconPath;
		set => _defaultIconPath = value;
	}

	public string? DefaultIconAccentPath
	{
		get => _category?.AccentPathData ?? _defaultIconAccentPath;
		set => _defaultIconAccentPath = value;
	}

	public SampleViewModel[] Samples { get; set; } = [];

	[IgnoreDataMember]
	public int SamplesHash
	{
		get
		{
			var sum = 0;
			foreach (var s in Samples)
			{
				unchecked
				{
					sum += GetEtagHash(s.Etag ?? string.Empty);
				}
			}

			return sum;
		}
	}

	private static int GetEtagHash(string s)
	{
		unchecked
		{
			var r = 0;

			foreach (var c in s)
			{
				r += c;
			}

			return r;

		}
	}
}
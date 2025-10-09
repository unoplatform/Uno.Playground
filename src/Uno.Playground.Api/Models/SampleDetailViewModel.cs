namespace Uno.Playground.Api.Models;

public class SampleDetailViewModel
{
	private readonly Sample? _sample;
	private string _id = string.Empty;
	private string _title = string.Empty;
	private string _description = string.Empty;
	private string? _category;
	private string _xaml = string.Empty;
	private string _data = string.Empty;

	public SampleDetailViewModel()
	{
	}

	public SampleDetailViewModel(Sample sample)
	{
		_sample = sample;
		_id = sample.Id;
		_title = sample.Title ?? "";
		_description = sample.Description ?? "";
		_category = sample.Category;
		_xaml = sample.Xaml ?? "";
		_data = sample.Data ?? "";
	}

	public string Id
	{
		get => _sample?.Id ?? _id;
		set => _id = value;
	}

	public string Title
	{
		get => _sample?.Title ?? "" ?? _title;
		set => _title = value;
	}

	public string Description
	{
		get => _sample?.Description ?? "" ?? _description;
		set => _description = value;
	}

	public string? Category
	{
		get => _sample?.Category ?? _category;
		set => _category = value;
	}

	public string Xaml
	{
		get => _sample?.Xaml ?? "" ?? _xaml;
		set => _xaml = value;
	}

	public string Data
	{
		get => _sample?.Data ?? "" ?? _data;
		set => _data = value;
	}
}

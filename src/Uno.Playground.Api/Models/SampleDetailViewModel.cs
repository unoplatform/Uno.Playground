namespace Uno.UI.Demo.Api.Models
{
	public class SampleDetailViewModel
	{
		private readonly Sample _sample;

		public SampleDetailViewModel(Sample sample)
		{
			_sample = sample;
		}

		public string Id => _sample.Id;

		public string Title => _sample.Title ?? "";

		public string Description => _sample.Description ?? "";

		public string Category => _sample.Category;

		public string Xaml => _sample.Xaml ?? "";

		public string Data => _sample.Data ?? "";
	}
}

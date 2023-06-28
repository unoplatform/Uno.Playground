namespace Uno.UI.Demo
{
#if __WASM__
	[Preserve]
#endif
	[Microsoft.UI.Xaml.Data.Bindable]
	public partial record class SampleForDisplay
	{
		public string CategoryId { get; }

		public string CategoryTitle { get; }

		public string Id { get; }

		public string Title { get; }

		public string IconPath { get; }

		public string IconAccentPath { get; }

		public SampleForDisplay(SampleCategory category, Sample sample)
		{
			CategoryId = category.CategoryId;
			CategoryTitle = category.Title;
			Id = sample.Id;
			Title = sample.Title;
			IconPath = sample.IconPath ?? category.DefaultIconPath;
			IconAccentPath = sample.IconAccentPath ?? category.DefaultIconAccentPath;
		}

		public override string ToString()
		{
			return $"{CategoryTitle} - {Title}";
		}
	}
}

namespace Uno.UI.Demo
{
#if __WASM__
	[Preserve]
#endif
	[Windows.UI.Xaml.Data.Bindable]
	[GeneratedImmutable]
	public partial class SampleForDisplay
	{
		[EqualityHash]
		public string CategoryId { get; }

		public string CategoryTitle { get; }

		[EqualityHash]
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
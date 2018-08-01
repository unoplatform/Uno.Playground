namespace Uno.UI.Demo
{
#if __WASM__
	[Preserve]
#endif
	public class SampleCategory
	{
#if __WASM__
		[Preserve]
#endif
		public SampleCategory() { }
		public string CategoryId { get; set; }
		public string Title { get; set; }
		public Sample[] Samples { get; set; }
		public string DefaultIconPath { get; set; }
		public string DefaultIconAccentPath { get; set; }
	}
}
namespace Uno.UI.Demo
{
#if __WASM__
	[Preserve]
#endif
	public class Sample
	{
		public string Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public string[] Keywords { get; set; }
		public string IconPath { get; set; }
		public string IconAccentPath { get; set; }
	}
}
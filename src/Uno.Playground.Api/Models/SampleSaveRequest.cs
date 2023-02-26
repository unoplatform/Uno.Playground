using System.Text.Json.Serialization;

namespace Uno.UI.Demo.Api.Models
{
	public class SampleSaveRequest
	{
		[JsonPropertyName("app")]
		public string App { get; set; }

		[JsonPropertyName("category")]
		public string Category { get; set; }

		[JsonPropertyName("title")]
		public string Title { get; set; }

		[JsonPropertyName("xaml")]
		public string Xaml { get; set; }

		[JsonPropertyName("code")]
		public string Code { get; set; }

		[JsonPropertyName("data")]
		public string Data { get; set; }

		[JsonPropertyName("pathdata")]
		public string PathData { get; set; }

		[JsonPropertyName("accentpathdata")]
		public string AccentPathData { get; set; }
	}
}

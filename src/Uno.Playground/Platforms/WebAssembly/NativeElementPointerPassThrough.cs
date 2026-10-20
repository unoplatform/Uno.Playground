using System.Runtime.InteropServices.JavaScript;

namespace Uno.UI.Wasm
{
	/// <summary>
	/// Lets the pointer through native elements (Monaco) to the Uno canvas, see WasmCSS/Playground.css.
	/// </summary>
	internal static class NativeElementPointerPassThrough
	{
		public static void Set(bool isEnabled)
		{
			using var document = JSHost.GlobalThis.GetPropertyAsJSObject("document")!;
			using var body = document.GetPropertyAsJSObject("body")!;
			using var dataset = body.GetPropertyAsJSObject("dataset")!;

			dataset.SetProperty("nativePointerPassThrough", isEnabled);
		}
	}
}

using System.Runtime.InteropServices.JavaScript;

namespace Uno.UI.Wasm
{
	static partial class FragmentNavigationHandler
	{
		private static partial class NativeMethods
		{
			private const string JsType = "globalThis.Uno.UI.FragmentNavigationHandler";

			[JSImport($"{JsType}.getCurrentFragment")]
			internal static partial string GetCurrentFragment();

			[JSImport($"{JsType}.setCurrentFragment")]
			internal static partial string SetCurrentFragment(string fragment);

			[JSImport($"{JsType}.subscribeToFragmentChanged")]
			internal static partial string SubscribeToFragmentChanged();
		}
	}
}

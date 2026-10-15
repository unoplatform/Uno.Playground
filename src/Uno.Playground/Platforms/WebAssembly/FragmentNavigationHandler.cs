using System.ComponentModel;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace Uno.UI.Wasm
{
	public static partial class FragmentNavigationHandler
	{
		public static string CurrentFragment
		{
			get
			{
				return RemoveLeadingHash(NativeMethods.GetCurrentFragment());
			}

			set
			{
				NativeMethods.SetCurrentFragment(value);
			}
		}

		private static event EventHandler<NewFragmentEventArgs>? _navigatedToFragment;

		public static event EventHandler<NewFragmentEventArgs> NavigatedToFragment
		{
			add
			{
				_navigatedToFragment += value;

				NativeMethods.SubscribeToFragmentChanged();
			}
			remove => _navigatedToFragment -= value;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[JSExport]
		[SupportedOSPlatform("browser")]
		public static void NotifyFragmentChanged(string newFragment)
		{
			_navigatedToFragment?.Invoke(null, new NewFragmentEventArgs(RemoveLeadingHash(newFragment)));
		}

		private static string RemoveLeadingHash(string fragmentName)
		{
			while (fragmentName.StartsWith("#"))
			{
				fragmentName = fragmentName.Substring(1);
			}

			return fragmentName;
		}
	}

	public class NewFragmentEventArgs
	{
		public string Fragment { get; }

		public NewFragmentEventArgs(string fragment)
		{
			Fragment = fragment;
		}
	}
}

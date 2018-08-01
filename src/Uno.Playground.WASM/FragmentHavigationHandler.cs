using System;
using System.ComponentModel;
using Uno.Foundation;

namespace Uno.UI.Wasm
{
	public static class FragmentHavigationHandler
	{
		public static string CurrentFragment
		{
			get
			{
				const string command = "Uno.UI.FragmentNavigationHandler.getCurrentFragment()";
				var fragment = WebAssemblyRuntime.InvokeJS(command);
				return RemoveLeadingHash(fragment);
			}

			set
			{
				var escaped = WebAssemblyRuntime.EscapeJs(value);
				var command =
					"Uno.UI.FragmentNavigationHandler.setCurrentFragment(\"" + escaped + "\")";

				WebAssemblyRuntime.InvokeJS(command);
			}
		}

		private static event EventHandler<NewFragmentEventArgs> _navigatedToFragment;

		public static event EventHandler<NewFragmentEventArgs> NavigatedToFragment
		{
			add
			{
				_navigatedToFragment += value;

				const string command = "Uno.UI.FragmentNavigationHandler.subscribeToFragmentChanged()";
				WebAssemblyRuntime.InvokeJS(command);
			}
			remove => _navigatedToFragment -= value;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Preserve]
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

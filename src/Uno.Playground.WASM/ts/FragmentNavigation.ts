namespace Uno.UI {
	export class FragmentNavigationHandler {

		private static currentFragment: string;

		public static getCurrentFragment(): string {
			return window.location.hash;
		}

		public static setCurrentFragment(fragment: string): string {
			window.location.hash = fragment;
			this.currentFragment = window.location.hash;

			return "ok";
		}

		private static subscribed: boolean = false;

		public static subscribeToFragmentChanged(): string {

			if (this.subscribed) {
				return "already subscribed";
			}

			this.subscribed = true;

			this.currentFragment = this.getCurrentFragment();

			window.addEventListener(
				"hashchange",
				_ => this.notifyFragmentChanged(),
				false);

			return "ok";
		}

		private static notifyFragmentChangedMethod: any;

		private static notifyFragmentChanged() {
			const newFragment: string = this.getCurrentFragment();
			if (newFragment === this.currentFragment) {
				return;  // nothing to do
			}

			this.currentFragment = newFragment;

			this.initializeMethods();
			const newFragmentStr = MonoRuntime.mono_string(newFragment);
			MonoRuntime.call_method(this.notifyFragmentChangedMethod, null, [newFragmentStr]);
		}

		private static initializeMethods(): void {
			if (this.notifyFragmentChangedMethod) {
				return; // already initialized.
			}

			const asm = MonoRuntime.assembly_load("Uno.Playground.WASM");
			const handlerClass = MonoRuntime.find_class(asm, "Uno.UI.Wasm", "FragmentHavigationHandler");
			this.notifyFragmentChangedMethod = MonoRuntime.find_method(handlerClass, "NotifyFragmentChanged", -1);
		}
	}

	declare var MonoRuntime: any;
}

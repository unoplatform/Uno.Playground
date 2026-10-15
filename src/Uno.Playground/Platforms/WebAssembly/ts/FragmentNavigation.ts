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

		private static notifyFragmentChangedMethod: (fragment: string) => void;

		private static async notifyFragmentChanged() {
			const newFragment: string = this.getCurrentFragment();
			if (newFragment === this.currentFragment) {
				return;  // nothing to do
			}

			this.currentFragment = newFragment;

			await this.initializeMethods();
			this.notifyFragmentChangedMethod(newFragment);
		}

		private static async initializeMethods(): Promise<void> {
			if (this.notifyFragmentChangedMethod) {
				return; // already initialized.
			}

			// [JSExport] methods of Uno.UI.Wasm.FragmentNavigationHandler
			const exports = await (<any>globalThis).Module.getAssemblyExports("Uno.Playground");
			this.notifyFragmentChangedMethod = exports.Uno.UI.Wasm.FragmentNavigationHandler.NotifyFragmentChanged;
		}
	}
}

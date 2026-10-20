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
				_ => this.notifyFragmentChanged().catch(e => console.error("Fragment change not delivered to .NET", e)),
				false);

			return "ok";
		}

		private static notifyFragmentChangedMethod: (fragment: string) => void;

		private static async notifyFragmentChanged() {
			const newFragment: string = this.getCurrentFragment();
			if (newFragment === this.currentFragment) {
				return;  // nothing to do
			}

			await this.initializeMethods();
			this.notifyFragmentChangedMethod(newFragment);

			// Only once .NET has it: if delivery fails, the fragment is retried on the next hashchange.
			this.currentFragment = newFragment;
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

var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        class FragmentNavigationHandler {
            static getCurrentFragment() {
                return window.location.hash;
            }
            static setCurrentFragment(fragment) {
                window.location.hash = fragment;
                this.currentFragment = window.location.hash;
                return "ok";
            }
            static subscribeToFragmentChanged() {
                if (this.subscribed) {
                    return "already subscribed";
                }
                this.subscribed = true;
                this.currentFragment = this.getCurrentFragment();
                window.addEventListener("hashchange", _ => this.notifyFragmentChanged().catch(e => console.error("Fragment change not delivered to .NET", e)), false);
                return "ok";
            }
            static async notifyFragmentChanged() {
                const newFragment = this.getCurrentFragment();
                if (newFragment === this.currentFragment) {
                    return; // nothing to do
                }
                await this.initializeMethods();
                this.notifyFragmentChangedMethod(newFragment);
                // Only once .NET has it: if delivery fails, the fragment is retried on the next hashchange.
                this.currentFragment = newFragment;
            }
            static async initializeMethods() {
                if (this.notifyFragmentChangedMethod) {
                    return; // already initialized.
                }
                // [JSExport] methods of Uno.UI.Wasm.FragmentNavigationHandler
                const exports = await globalThis.Module.getAssemblyExports("Uno.Playground");
                this.notifyFragmentChangedMethod = exports.Uno.UI.Wasm.FragmentNavigationHandler.NotifyFragmentChanged;
            }
        }
        FragmentNavigationHandler.subscribed = false;
        UI.FragmentNavigationHandler = FragmentNavigationHandler;
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));

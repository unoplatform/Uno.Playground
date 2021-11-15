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
                window.addEventListener("hashchange", _ => this.notifyFragmentChanged(), false);
                return "ok";
            }
            static notifyFragmentChanged() {
                const newFragment = this.getCurrentFragment();
                if (newFragment === this.currentFragment) {
                    return; // nothing to do
                }
                this.currentFragment = newFragment;
                this.initializeMethods();
                const newFragmentStr = MonoRuntime.mono_string(newFragment);
                MonoRuntime.call_method(this.notifyFragmentChangedMethod, null, [newFragmentStr]);
            }
            static initializeMethods() {
                if (this.notifyFragmentChangedMethod) {
                    return; // already initialized.
                }
                const asm = MonoRuntime.assembly_load("Uno.Playground.WASM");
                const handlerClass = MonoRuntime.find_class(asm, "Uno.UI.Wasm", "FragmentHavigationHandler");
                this.notifyFragmentChangedMethod = MonoRuntime.find_method(handlerClass, "NotifyFragmentChanged", -1);
            }
        }
        FragmentNavigationHandler.subscribed = false;
        UI.FragmentNavigationHandler = FragmentNavigationHandler;
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));

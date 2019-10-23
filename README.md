# Uno Playground

This is the code for published apps on the stores: _Uno.UI Demo_ on the [Microsoft Store](https://www.microsoft.com/store/apps/9NTT97F69ZHZ), _Uno Gallery_ for [Android](https://play.google.com/store/apps/details?id=com.nventive.uno.ui.demo) & [iOS](https://itunes.apple.com/app/uno-gallery/id1380984680)
and _Uno Playground_ for the web at <http://playground.platform.uno>.

## Building it

You'll need Visual Studio 2017 v15.5+ to compile it. You also need these workloads:

- ASP.NET and web development
- .NET Core cross-platform development
- Xamarin SDK (if you want to test iOS & Android)

## UWP

To run the UWP project, simply select the project `Uno.UI.Demo.UWP` as starting
project.

## Android & iOS

To run Xamarin versions, select the project `Uno.UI.Demo.Droid` or `Uno.UI.Demo.iOS`
as starting project. For iOS, don't forget to pick emulator or device. You may need
additional provisioning configuration to run it on a device.

## Wasm

Run the project `Uno.UI.Demo.AspnetShell` - that's the `platform.uno` web site
and go to the url `/Playground/index.html` once it's started (don't use the link
in the site, it will bring you to the public version).

# Uno Playground

This is the code for the website <http://platform.uno/>, for published apps on stores
(Windows, Android & iOS) and for the _Uno Playground_ <http://playground.platform.uno>.

## Building it

You'll need Visual Studio 2017 v15.5+ to compile it. You also need those components:

- Web Site development (aspnet core)
- NodeJS (optional, for `gulp`)
- .NET Core cross-platform projects
- Xamarin SDK (if you want to test iOS & Android)

## UWP

To run the UWP project, simply select the project `Uno.UI.Demo.UWP` as starting
project.

## Android & iOS

To run Xamarin versions, select the project `Uno.UI.Demo.Droid` or `Uno.UI.Demo.iOS`
as starting project. For iOS, don't forget to pick emulator or device. You may need
additionnal provisionning configuration to run it on a device.

## Wasm

Run the project `Uno.UI.Demo.AspnetShell` - that's the `platform.uno` web site
and go to the url `/Playground/index.html` once it's started (don't use the link
in the site, it will bring you to the public version).
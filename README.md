# Uno Playground

This is the code for published apps on stores (_Uno Gallery_) - Windows, Android & iOS
and for the _Uno Playground_ <http://playground.platform.uno>.

## Building it

You'll need Visual Studio 2017 v15.5+ to compile it. You also need those components:

- Web Site development (aspnet core)
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

## Local API (Azure Functions Core Tools)

Running the API or the AppHost locally may require the Azure Functions Core Tools (`func`).
If you see an error mentioning "Unable to launch the Azure Functions Core Tools", install the tools and verify:

Chocolatey:
```powershell
choco install azure-functions-core-tools-4
```

npm (Node.js required):
```powershell
npm i -g azure-functions-core-tools@4 --unsafe-perm true
```

Verify installation:
```powershell
func --version
```

You can also run the API via the provided Dockerfile/docker-compose if you prefer not to install `func`.

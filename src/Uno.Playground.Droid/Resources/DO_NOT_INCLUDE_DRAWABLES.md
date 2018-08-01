# DO NOT INCLUDE DRAWABLES IN THE PROJECT
1. Add drawables to `Resources/drawable-*dpi` using File Explorer.
2. Reload the project.
3. The appropriate drawables will be dynamically included:
	a. Debug: Only low-DPI resources (`nodpi` and `nodpi`) will be included.
	b. Release: All resources (including `tvdpi`, `hdpi`, `xhdpi`, `xxhdpi`, `xxxhdpi`) will be included.

This improves compilation time. See `MyUmbrellaApp.Android.csproj` for the implementation.
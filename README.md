# ArcUILibrary
The arc(or curved) UI component for WPF application.

In an interview, I was asked how to bend the UI control in WPF. Although I recalled that was developed in my past project, but didn't answer very well. So, I decide to share my thought if someone has suffering in MS empire just like me.

Before getting started, Visual Studio Blend is good tool to do the same feature and much easier.

## How it works
The idea was came out when did another 3D rendering project, is using [Viewport3D](https://docs.microsoft.com/zh-tw/dotnet/api/system.windows.controls.viewport3d) to overwrite or override WPF control's default rendering behavior, by update mesh geometry for drawing arc shape in 2D rednering.

In addition, component's DependencyProperty must be implemented for XAML styling and animation.

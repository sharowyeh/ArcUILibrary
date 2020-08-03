# ArcUILibrary
The arc(or curved) UI component for WPF application.

I was asked how to bend the WPF control in an interview. Although I recalled that was developed few years age, but didn't explain very well at that time.

Before getting start, Visual Studio Blend is good tool to do rendering stuff and much easier.

## How it works
The idea was came out when did another 3D rendering project, overrides [Viewport3D](https://docs.microsoft.com/zh-tw/dotnet/api/system.windows.controls.viewport3d) for rendering behavior to WPF control customization, by update mesh geometry for drawing arc shape in 2D rednering.

In addition, component's DependencyProperty must be implemented for XAML styling and animation.

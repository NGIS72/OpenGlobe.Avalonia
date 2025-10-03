using Avalonia;
using Avalonia.OpenGL;
using OpenGlobe;
using System;
using System.Collections.Generic;

namespace OpenGlobe.Examples
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithOpenGL()
            .WithInterFont()
            .LogToTrace();
    }
}

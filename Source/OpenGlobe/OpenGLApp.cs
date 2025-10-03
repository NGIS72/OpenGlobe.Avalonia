using Avalonia;
using Avalonia.OpenGL;

namespace OpenGlobe
{
    public static class OpenGLApp
    {
        public static readonly IList<GlVersion> Profiles = new[] { new GlVersion(GlProfileType.OpenGL, 3, 3) };

        public static AppBuilder WithOpenGL(this AppBuilder builder)
        {
            return builder
                .With(new Win32PlatformOptions
                {
                    RenderingMode = new[] { Win32RenderingMode.Wgl },
                    WglProfiles = Profiles,
                })
                .With(new X11PlatformOptions
                {
                    GlProfiles = Profiles
                });
        }
    }
}

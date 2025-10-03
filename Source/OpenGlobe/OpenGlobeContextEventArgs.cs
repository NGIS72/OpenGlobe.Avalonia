using OpenGlobe.Renderer;
using OpenGlobe.Scene;

namespace OpenGlobe
{
    public class OpenGlobeContextEventArgs : EventArgs
    {
        public OpenGlobeContextEventArgs(Context context,IScene scene)
        {
            Context = context;
            Scene = scene;
        }
        public Context Context { get; private set; }
        public IScene Scene { get; private set; }
    }
}

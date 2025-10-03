using Avalonia;
using OpenGlobe.Renderer;
using OpenGlobe.Scene.Cameras;

namespace OpenGlobe.Scene
{
    public interface IScene : IViewEvents
    {
        int Width { get; }
        int Height { get; }

        SceneState SceneState { get; set; }
        ClearState ClearState {  get; set; }
        ICameraView Camera {  get; set; }
        void Resize(int width, int height);
        void Render(Context context);
        void PreRender(Context context);
        void PostRender(Context context);
        void Load(Context context);
        void Unload(Context context);
    }

}

using Avalonia.Input;
using OpenGlobe.Renderer;
using OpenGlobe.Scene.Cameras;

namespace OpenGlobe.Scene
{
    public class SceneWrapper: IScene
    {
        private IScene m_Scene;

        public SceneWrapper(IScene scene)
        {
            m_Scene = scene;
        }

        public int Width => m_Scene?.Width??0;
        public int Height => m_Scene?.Height??0;

        public IScene Scene { get => m_Scene; set => m_Scene = value; }


        public SceneState SceneState { get => m_Scene.SceneState; set => m_Scene.SceneState=value; }
        public ClearState ClearState { get => m_Scene.ClearState; set => m_Scene.ClearState=value; }
        public ICameraView Camera { get => m_Scene.Camera; set => m_Scene.Camera=value; }

        public virtual void KeyDown(object sender, KeyEventArgs e)
        {
            m_Scene.KeyDown(sender, e);
        }

        public virtual void KeyUp(object sender, KeyEventArgs e)
        {
            m_Scene.KeyUp(sender, e);
        }

        public virtual void Load(Context context)
        {
            m_Scene.Load(context);
        }

        public virtual void PointerCaptureLost(object sender, PointerCaptureLostEventArgs e)
        {
            m_Scene.PointerCaptureLost(sender, e);
        }

        public virtual void PointerEntered(object sender, PointerEventArgs e)
        {
            m_Scene.PointerEntered(sender, e);
        }

        public virtual void PointerExited(object sender, PointerEventArgs e)
        {
            m_Scene.PointerExited(sender, e);
        }

        public virtual void PointerMoved(object sender, PointerEventArgs e)
        {
            m_Scene.PointerMoved(sender, e);
        }

        public virtual void PointerPressed(object sender, PointerEventArgs e)
        {
            m_Scene.PointerPressed(sender, e);
        }

        public virtual void PointerReleased(object sender, PointerEventArgs e)
        {
            m_Scene.PointerReleased(sender, e);
        }

        public virtual void PointerWheelChanged(object sender, PointerEventArgs e)
        {
            m_Scene.PointerWheelChanged(sender, e);
        }

        public virtual void PostRender(Context context)
        {
            m_Scene.PostRender(context);
        }

        public virtual void PreRender(Context context)
        {
            m_Scene.PreRender(context);
        }

        public virtual void Render(Context context)
        {
            m_Scene.Render(context);
        }

        public virtual void Unload(Context context)
        {
            m_Scene.Unload(context);
        }

        public virtual void Resize(int width, int height)
        {
            m_Scene.Resize(width, height);
        }
    }
}

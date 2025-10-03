using Avalonia.Input;
using OpenGlobe.Core;
using OpenGlobe.Renderer;
using OpenGlobe.Scene.Cameras;

namespace OpenGlobe.Scene
{
    public abstract class SceneBase:Disposable,IScene
    {
        private SceneState m_SceneState;
        private ICameraView m_CameraView;
        private ClearState m_ClearState;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public SceneState SceneState { get => m_SceneState;set=> m_SceneState = value; }
        public ClearState ClearState { get => m_ClearState;set => m_ClearState = value; }
        public ICameraView Camera { get => m_CameraView; set => m_CameraView = value; }

        public Context Context { get; private set; }

        public Device Device => Context?.Device;

        public virtual void KeyDown(object sender, KeyEventArgs e)
        {
            m_CameraView?.KeyDown(sender, e);
        }

        public virtual void KeyUp(object sender, KeyEventArgs e)
        {
            m_CameraView?.KeyDown(sender, e);
        }

        public virtual void Load(Context context)
        {
            Context = context;

            if (m_SceneState == null) m_SceneState = new SceneState();
            if (m_ClearState == null) m_ClearState = new ClearState();
            if (m_CameraView == null) m_CameraView = new CameraLookAtPoint(m_SceneState.Camera);

            Width = context.Viewport.Width;
            Height = context.Viewport.Height;
        }

        public void SetCameraLookAtPoint(Ellipsoid ellipsoid)
        {
            m_CameraView = new CameraLookAtPoint(m_SceneState.Camera, ellipsoid);
        }
        public void SetCameraFly()
        {
            m_CameraView = new CameraFly(m_SceneState.Camera);
        }

        public virtual void PointerCaptureLost(object sender, PointerCaptureLostEventArgs e)
        {
            m_CameraView.PointerCaptureLost(sender, e);
        }

        public virtual void PointerEntered(object sender, PointerEventArgs e)
        {
            m_CameraView.PointerEntered(sender, e);
        }

        public virtual void PointerExited(object sender, PointerEventArgs e)
        {
            m_CameraView.PointerExited(sender, e);
        }

        public virtual void PointerMoved(object sender, PointerEventArgs e)
        {
            m_CameraView.PointerMoved(sender, e);
        }

        public virtual void PointerPressed(object sender, PointerEventArgs e)
        {
            m_CameraView.PointerPressed(sender, e);
        }

        public virtual void PointerReleased(object sender, PointerEventArgs e)
        {
            m_CameraView.PointerReleased(sender, e);
        }

        public virtual void PointerWheelChanged(object sender, PointerEventArgs e)
        {
            m_CameraView.PointerWheelChanged(sender, e);
        }

        public virtual void PostRender(Context context)
        {
            
        }

        public virtual void PreRender(Context context)
        {
            m_CameraView?.PreRender();
        }

        public abstract void Render(Context context);

        public virtual void Unload(Context context)
        {
            
        }

        public virtual void Resize(int width, int height)
        {
            Width = width;
            Height = height;
        }

        protected override void Dispose(bool disposing)
        {
            (m_CameraView as IDisposable)?.Dispose();
            m_CameraView = null;
        }
    }
}

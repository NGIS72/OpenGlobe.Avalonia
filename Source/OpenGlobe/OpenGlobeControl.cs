using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Logging;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using Avalonia.VisualTree;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Threading.Tasks;

namespace OpenGlobe
{
    public class OpenGlobeControl : OpenGlControlBase
    {
        private Context m_Context;
        private InputElement m_Root;
        private IScene m_Scene, m_NextScene;
        //private SceneWrapper m_SceneWrapper;


        public event EventHandler<OpenGlobeContextEventArgs> PreRender;
        public event EventHandler<OpenGlobeContextEventArgs> PostRender;

        public bool IsReady { get; private set; }
        public Context Context => m_Context;
        public IScene Scene 
        {
            get => m_NextScene ?? m_Scene;
            set
            {
                if (Scene != value)
                {
                    m_NextScene = value;
                }
            }
        }

        private IScene GetScene()
        {
            if (IsReady)
            {
                var old = m_Scene;
                var next = m_NextScene;
                if (next != null && next!=old)
                {
                    next.Load(m_Context);
                    m_Scene = next;
                    m_NextScene = null;
                    old?.Unload(Context);
                }
                return m_Scene;
            }
            return null;
        }

        //public SceneWrapper Wrap(Func<IScene, SceneWrapper> create)
        //{
        //    var scene = GetScene();
        //    if (scene == null) return null;

        //    m_SceneWrapper = create(scene);
        //    return m_SceneWrapper;
        //}

        //public void Unwrap(SceneWrapper wrapper)
        //{
        //    var scene = GetScene();
        //    if (scene == null) return;
        //    if (wrapper == null || wrapper == m_SceneWrapper)
        //    {
        //        var old = m_SceneWrapper;
        //        m_SceneWrapper = m_SceneWrapper?.Scene as SceneWrapper;
        //        (old as IDisposable)?.Dispose();
        //    }
        //    else
        //    {
        //        var last = scene;
        //        while (last is SceneWrapper wrap)
        //        {
        //            if(wrap.Scene == wrapper)
        //            {
        //                wrap.Scene = ((SceneWrapper)wrap.Scene).Scene;
        //                (wrapper as IDisposable)?.Dispose();
        //                break;
        //            }

        //            last = wrap.Scene;
        //        }
        //    }
        //}

        private bool m_SizeChanged;
        private PointerCaptureLostEventArgs m_PointerCaptureLost;
        private PointerEventArgs m_PointerEntered, m_PointerExit, m_PointerMoved, m_PointerPressed, m_PointerReleased, m_PointerWheelChanged;
        private KeyEventArgs m_KeyDown, m_KeyUp;

        private void TryRunActions(IScene scene)
        {
            var context = m_Context;
            if (context != null && scene != null)
            {
                if (m_SizeChanged)
                {
                    m_SizeChanged = false;
                    context.Viewport = new Avalonia.PixelRect(0, 0, (int)Bounds.Width, (int)Bounds.Height);
                    scene.Resize((int)Bounds.Width, (int)Bounds.Height);
                }
                if (m_KeyDown != null)
                {
                    scene.KeyDown(this, m_KeyDown);
                    m_KeyDown = null;
                }
                if (m_KeyUp != null)
                {
                    scene.KeyUp(this, m_KeyUp);
                    m_KeyUp = null;
                }
                if (m_PointerCaptureLost != null)
                {
                    scene.PointerCaptureLost(this, m_PointerCaptureLost);
                    m_PointerCaptureLost = null;
                }
                if (m_PointerEntered != null)
                {
                    scene.PointerEntered(this, m_PointerEntered);
                    m_PointerEntered = null;
                }
                if (m_PointerMoved != null)
                {
                    scene.PointerMoved(this, m_PointerMoved);
                    m_PointerMoved = null;
                }
                if (m_PointerPressed != null)
                {
                    scene.PointerPressed(this, m_PointerPressed);
                    m_PointerPressed = null;
                }
                if (m_PointerReleased != null)
                {
                    scene.PointerReleased(this, m_PointerReleased);
                    m_PointerReleased = null;
                }
                if (m_PointerWheelChanged != null)
                {
                    scene.PointerWheelChanged(this, m_PointerWheelChanged);
                    m_PointerWheelChanged = null;
                }
                if (m_PointerExit != null)
                {
                    scene.PointerExited(this, m_PointerExit);
                    m_PointerExit = null;
                }
            }
            m_SizeChanged = false;
            m_KeyDown = null;
            m_KeyUp=null;
            m_PointerCaptureLost = null;
            m_PointerEntered = null;
            m_PointerMoved = null;
            m_PointerPressed = null;
            m_PointerReleased = null;
            m_PointerWheelChanged = null;
            m_PointerExit = null;
        }
        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);
            m_SizeChanged = true;
        }

        protected override void OnOpenGlInit(GlInterface gl)
        {
            base.OnOpenGlInit(gl);

            GL.LoadBindings(new OpenTKBindings(gl));
            
            m_Context = Context.CreateContext((int)Bounds.Width, (int)Bounds.Height);

            (m_Scene)?.Load(m_Context);
            IsReady = true;
        }

        protected override void OnOpenGlDeinit(GlInterface gl)
        {
            base.OnOpenGlDeinit(gl);

            IsReady = false;

            m_Scene?.Unload(m_Context);
        }

        protected override void OnOpenGlRender(GlInterface gl, int fb)
        {
            if (m_Context == null) return;

            //gl.Viewport(0, 0, (int)Bounds.Width, (int)Bounds.Height);
            m_Context.Viewport = new PixelRect(0, 0, (int)Bounds.Width, (int)Bounds.Height);
            var scene = GetScene();

            TryRunActions(scene);

            var camera = scene?.SceneState?.Camera;
            if (camera != null)
                camera.AspectRatio = Bounds.Width / (double)Bounds.Height;

            var args = new OpenGlobeContextEventArgs(m_Context, scene);

            PreRender?.Invoke(this, args);
            
            if (scene != null)
            {
                scene.PreRender(m_Context);
                scene.Render(m_Context);
                scene.PostRender(m_Context);
            }

            PostRender?.Invoke(this, args);

            Dispatcher.UIThread.Post(RequestNextFrameRendering, DispatcherPriority.Background);
        }


        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            RegisterPointerEvents();
        }

        protected override void OnUnloaded(RoutedEventArgs e)
        {
            UnregisterPointerEvents();
            base.OnUnloaded(e);
        }

        #region Tool

        private void RegisterPointerEvents()
        {
            m_Root = this.FindAncestorOfType<DockPanel>() as InputElement ?? this.FindAncestorOfType<Window>();
            if (m_Root != null)
            {
                var routingStrategy = RoutingStrategies.Direct | RoutingStrategies.Tunnel/* | RoutingStrategies.Bubble*/;

                m_Root.AddHandler(PointerMovedEvent, RootPointerMoved, routingStrategy);
                m_Root.AddHandler(PointerCaptureLostEvent, RootPointerCaptureLost, routingStrategy);
                m_Root.AddHandler(PointerEnteredEvent, RootPointerEntered, routingStrategy);
                m_Root.AddHandler(PointerExitedEvent, RootPointerExited, routingStrategy);
                m_Root.AddHandler(PointerPressedEvent, RootPointerPressed, routingStrategy);
                m_Root.AddHandler(PointerReleasedEvent, RootPointerReleased, routingStrategy);
                m_Root.AddHandler(PointerWheelChangedEvent, RootPointerWheelChanged, routingStrategy);

                m_Root.AddHandler(KeyUpEvent, RootKeyUp, routingStrategy);
                m_Root.AddHandler(KeyDownEvent, RootKeyDown, routingStrategy);
            }

        }
        private void UnregisterPointerEvents()
        {
            if (m_Root != null)
            {
                m_Root.RemoveHandler(PointerMovedEvent, RootPointerMoved);
                m_Root.RemoveHandler(PointerCaptureLostEvent, RootPointerCaptureLost);
                m_Root.RemoveHandler(PointerEnteredEvent, RootPointerEntered);
                m_Root.RemoveHandler(PointerExitedEvent, RootPointerExited);
                m_Root.RemoveHandler(PointerPressedEvent, RootPointerPressed);
                m_Root.RemoveHandler(PointerReleasedEvent, RootPointerReleased);
                m_Root.RemoveHandler(PointerWheelChangedEvent, RootPointerWheelChanged);

                m_Root.RemoveHandler(KeyUpEvent, RootKeyUp);
                m_Root.RemoveHandler(KeyDownEvent, RootKeyDown);
            }
            m_Root = null;
        }

        private void RootPointerCaptureLost(object sender, PointerCaptureLostEventArgs e)
        {
            m_PointerCaptureLost = e;
        }
        private void RootPointerEntered(object sender, PointerEventArgs e)
        {
            m_PointerEntered = e;
        }
        private void RootPointerExited(object sender, PointerEventArgs e)
        {
           m_PointerExit = e;
        }
        private void RootPointerMoved(object sender, PointerEventArgs e)
        {
            m_PointerMoved = e;
        }
        private void RootPointerPressed(object sender, PointerPressedEventArgs e)
        {
           m_PointerPressed = e;
        }
        private void RootPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            m_PointerReleased = e;
        }
        private void RootPointerWheelChanged(object sender, PointerWheelEventArgs e)
        {
            m_PointerWheelChanged = e;
        }

        #endregion

        #region Key args

        private void RootKeyDown(object sender, KeyEventArgs e)
        {
            m_KeyDown = e;
        }

        private void RootKeyUp(object sender, KeyEventArgs e)
        {
            m_KeyUp = e;
        }

        #endregion

        private class OpenTKBindings : IBindingsContext
        {
            private GlInterface m_Interface;
            public OpenTKBindings(GlInterface glInterface)
            {
                m_Interface = glInterface;
            }
            public IntPtr GetProcAddress(string procName) => m_Interface.GetProcAddress(procName);
        }
    }
}

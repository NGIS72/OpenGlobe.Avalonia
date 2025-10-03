#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Renderer
{
    public sealed class HighResolutionSnap : IDisposable
    {
        public HighResolutionSnap(OpenGlobeControl window, SceneState sceneState)
        {
            _window = window;
            _sceneState = sceneState;

            window.KeyDown += (s,e)=>
            {
                if (e.Key == Avalonia.Input.Key.Space)
                {
                    Enable(true);
                }
            };
        }

        private void PreRenderFrame(object sender,OpenGlobeContextEventArgs e)
        {
            var context = e.Context;

            _snapBuffer = new HighResolutionSnapFramebuffer(context, WidthInInches, DotsPerInch, _sceneState.Camera.AspectRatio);
            context.Framebuffer = _snapBuffer.Framebuffer;

            _previousViewport = context.Viewport;
            context.Viewport = new Avalonia.PixelRect(0, 0, _snapBuffer.WidthInPixels, _snapBuffer.HeightInPixels);

            _previousSnapScale = _sceneState.HighResolutionSnapScale;
            _sceneState.HighResolutionSnapScale = (double)context.Viewport.Width / (double)_previousViewport.Width;
        }

        private void PostRenderFrame(object sender, OpenGlobeContextEventArgs e)
        {
            if (ColorFilename != null)
            {
                _snapBuffer.SaveColorBuffer(ColorFilename);
            }

            if (DepthFilename != null)
            {
                _snapBuffer.SaveDepthBuffer(DepthFilename);
            }

            _window.Context.Framebuffer = null;
            _window.Context.Viewport = _previousViewport;
            _sceneState.HighResolutionSnapScale = _previousSnapScale;

            Enable(false);
            _snapBuffer.Dispose();
            _snapBuffer = null;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Enable(false);

            if (_snapBuffer != null)
            {
                _snapBuffer.Dispose();
            }
        }

        #endregion

        private void Enable(bool value)
        {
            if (_enabled != value)
            {
                if (value)
                {
                    _window.PreRender += PreRenderFrame;
                    _window.PostRender += PostRenderFrame;
                }
                else
                {
                    _window.PreRender -= PreRenderFrame;
                    _window.PostRender -= PostRenderFrame;
                }

                _enabled = value;
            }
        }

        public string ColorFilename { get; set; }
        public string DepthFilename { get; set; }
        public double WidthInInches { get; set; }
        public int DotsPerInch { get; set; }

        public HighResolutionSnapFramebuffer SnapBuffer => _snapBuffer;

        private OpenGlobeControl _window;
        private SceneState _sceneState;
        private bool _enabled;
        private HighResolutionSnapFramebuffer _snapBuffer;

        private Avalonia.PixelRect _previousViewport;
        private double _previousSnapScale;
    }
}
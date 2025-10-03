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
    public sealed class HighResolutionSnapFramebuffer : IDisposable
    {
        private Context m_Context;
        public HighResolutionSnapFramebuffer(Context context, double widthInInches, int dotsPerInch, double aspectRatio)
        {
            m_Context = context;
            _widthInInches = widthInInches;
            _dotsPerInch = dotsPerInch;
            _aspectRatio = aspectRatio;

            Texture2DDescription colorDescription = new Texture2DDescription(WidthInPixels, HeightInPixels, TextureFormat.RedGreenBlue8, false);
            _colorTexture = m_Context.Device.CreateTexture2D(colorDescription);

            Texture2DDescription depthDescription = new Texture2DDescription(WidthInPixels, HeightInPixels, TextureFormat.Depth24, false);
            _depthTexture = m_Context.Device.CreateTexture2D(depthDescription);

            _framebuffer = context.CreateFramebuffer();
            _framebuffer.ColorAttachments[0] = _colorTexture;
            _framebuffer.DepthAttachment = _depthTexture;
        }

        public double WidthInInches => _widthInInches;

        public double HeightInInches => _widthInInches * (1.0 / AspectRatio);

        public int WidthInPixels => (int)(WidthInInches * DotsPerInch);

        public int HeightInPixels => (int)(HeightInInches * DotsPerInch);

        public int DotsPerInch => _dotsPerInch;

        public double AspectRatio => _aspectRatio;

        public Framebuffer Framebuffer => _framebuffer;

        public void SaveColorBuffer(string filename)
        {
            _framebuffer.ColorAttachments[0].Save(filename);
        }

        public void SaveDepthBuffer(string filename)
        {
            _framebuffer.DepthAttachment.Save(filename);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _colorTexture.Dispose();
            _depthTexture.Dispose();
            _framebuffer.Dispose();
        }

        #endregion

        private readonly double _widthInInches;
        private readonly int _dotsPerInch;
        private readonly double _aspectRatio;

        private Texture2D _colorTexture;
        private Texture2D _depthTexture;
        private Framebuffer _framebuffer;
    }
}
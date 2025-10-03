#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Renderer
{
    public class TextureSamplers
    {
        private Context m_Context;

        internal TextureSamplers (Context context)
	    {
            m_Context = context;

            _nearestClamp = m_Context.Device.CreateTexture2DSampler(
                    TextureMinificationFilter.Nearest,
                    TextureMagnificationFilter.Nearest,
                    TextureWrap.Clamp,
                    TextureWrap.Clamp);

            _linearClamp = m_Context.Device.CreateTexture2DSampler(
                    TextureMinificationFilter.Linear,
                    TextureMagnificationFilter.Linear,
                    TextureWrap.Clamp,
                    TextureWrap.Clamp);

            _nearestRepeat = m_Context.Device.CreateTexture2DSampler(
                    TextureMinificationFilter.Nearest,
                    TextureMagnificationFilter.Nearest,
                    TextureWrap.Repeat,
                    TextureWrap.Repeat);

            _linearRepeat = m_Context.Device.CreateTexture2DSampler(
                    TextureMinificationFilter.Linear,
                    TextureMagnificationFilter.Linear,
                    TextureWrap.Repeat,
                    TextureWrap.Repeat);
	    }

        public TextureSampler NearestClamp => _nearestClamp;

        public TextureSampler LinearClamp => _linearClamp;

        public TextureSampler NearestRepeat => _nearestRepeat;

        public TextureSampler LinearRepeat => _linearRepeat;

        private readonly TextureSampler _nearestClamp;
        private readonly TextureSampler _linearClamp;
        private readonly TextureSampler _nearestRepeat;
        private readonly TextureSampler _linearRepeat;
    }
}

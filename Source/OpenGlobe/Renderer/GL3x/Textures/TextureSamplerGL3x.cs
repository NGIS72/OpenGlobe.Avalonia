#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using OpenTK.Graphics.OpenGL;

namespace OpenGlobe.Renderer.GL3x
{
    internal class TextureSamplerGL3x : TextureSampler
    {
        private Context m_Context;
        public TextureSamplerGL3x(
            Context context,
            TextureMinificationFilter minificationFilter,
            TextureMagnificationFilter magnificationFilter,
            TextureWrap wrapS,
            TextureWrap wrapT,
            float maximumAnistropy)
            : base(
                minificationFilter, 
                magnificationFilter, 
                wrapS, 
                wrapT, 
                maximumAnistropy)
        {
            m_Context = context;

            _name = new SamplerNameGL3x();

            int glMinificationFilter = (int)TypeConverterGL3x.To(minificationFilter);
            int glMagnificationFilter = (int)TypeConverterGL3x.To(magnificationFilter);
            int glWrapS = (int)TypeConverterGL3x.To(wrapS);
            int glWrapT = (int)TypeConverterGL3x.To(wrapT);

            GL.SamplerParameterI(_name.Value, SamplerParameterName.TextureMinFilter, ref glMinificationFilter);
            GL.SamplerParameterI(_name.Value, SamplerParameterName.TextureMagFilter, ref glMagnificationFilter);
            GL.SamplerParameterI(_name.Value, SamplerParameterName.TextureWrapS, ref glWrapS);
            GL.SamplerParameterI(_name.Value, SamplerParameterName.TextureWrapT, ref glWrapT);

            if (m_Context.Device.Extensions.AnisotropicFiltering)
            {
                GL.SamplerParameter(_name.Value, SamplerParameterName.TextureMaxAnisotropyExt, maximumAnistropy);
            }
            else
            {
                if (maximumAnistropy != 1)
                {
                    throw new InsufficientVideoCardException("Anisotropic filtering is not supported.  The extension GL_EXT_texture_filter_anisotropic was not found.");
                }
            }
        }

        internal void Bind(int textureUnitIndex)
        {
            GL.BindSampler(textureUnitIndex, _name.Value);
        }

        internal static void UnBind(int textureUnitIndex)
        {
            GL.BindSampler(textureUnitIndex, 0);
        }

        #region Disposable Members

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _name.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        private readonly SamplerNameGL3x _name;
    }
}
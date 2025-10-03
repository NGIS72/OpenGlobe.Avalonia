#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    internal class ViewportUniform : DrawAutomaticUniform
    {
        public ViewportUniform(Uniform uniform)
        {
            _uniform = (Uniform<Vector4F>)uniform;
        }

        #region DrawAutomaticUniform Members

        public override void Set(Context context, DrawState drawState, SceneState sceneState)
        {
            //
            // viewport.Bottom should really be used but Rectangle goes top to botom, not bottom to top.
            //
            var viewport = context.Viewport;
            _uniform.Value = new Vector4F(viewport.X, viewport.Y, viewport.Width, viewport.Height);
        }

        #endregion

        private Uniform<Vector4F> _uniform;
    }
}

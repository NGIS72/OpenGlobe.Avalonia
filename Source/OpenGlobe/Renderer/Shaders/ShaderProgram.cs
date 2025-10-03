#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    public abstract class ShaderProgram : Disposable
    {
        protected Context m_Context;

        protected ShaderProgram(Context context)
        {
            m_Context = context;
        }

        public abstract string Log { get; }
        public abstract FragmentOutputs FragmentOutputs  { get; }
        public abstract ShaderVertexAttributeCollection VertexAttributes { get; }
        public abstract UniformCollection Uniforms { get; }
        public abstract UniformBlockCollection UniformBlocks { get; }

        protected void InitializeAutomaticUniforms(UniformCollection uniforms)
        {
            foreach (Uniform uniform in uniforms)
            {
                if (m_Context.Device.LinkAutomaticUniforms.Contains(uniform.Name))
                {
                    m_Context.Device.LinkAutomaticUniforms[uniform.Name].Set(uniform);
                }
                else if (m_Context.Device.DrawAutomaticUniformFactories.Contains(uniform.Name))
                {
                    _drawAutomaticUniforms.Add(m_Context.Device.DrawAutomaticUniformFactories[uniform.Name].Create(uniform));
                }
            }
        }

        protected void SetDrawAutomaticUniforms(Context context, DrawState drawState, SceneState sceneState)
        {
            for (int i = 0; i < _drawAutomaticUniforms.Count; ++i)
            {
                _drawAutomaticUniforms[i].Set(context, drawState, sceneState);
            }
        }

        private List<DrawAutomaticUniform> _drawAutomaticUniforms = new List<DrawAutomaticUniform>();
    }
}

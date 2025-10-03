#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Drawing;
using System.Collections.Generic;
using OpenGlobe.Core;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;

namespace OpenGlobe.Examples
{
    sealed class Triangle : SceneBase, IDisposable
    {
        private Context m_Context;
        public override void Load(Context context)
        {
            base.Load(context);

            m_Context = context;

            string vs =
                @"#version 330

                  layout(location = og_positionVertexLocation) in vec4 position;
                  uniform mat4 og_modelViewPerspectiveMatrix;

                  void main()                     
                  {
                        gl_Position = og_modelViewPerspectiveMatrix * position; 
                  }";

            string fs =
                @"#version 330
                 
                  out vec3 fragmentColor;
                  uniform vec3 u_color;

                  void main()
                  {
                      fragmentColor = u_color;
                  }";
            ShaderProgram sp = m_Context.Device.CreateShaderProgram(vs, fs);
            ((Uniform<Vector3F>)sp.Uniforms["u_color"]).Value = new Vector3F(1, 0, 0);

            ///////////////////////////////////////////////////////////////////
            
            Mesh mesh = new Mesh();

            VertexAttributeFloatVector3 positionsAttribute = new VertexAttributeFloatVector3("position", 3);
            mesh.Attributes.Add(positionsAttribute);

            IndicesUnsignedShort indices = new IndicesUnsignedShort(3);
            mesh.Indices = indices;

            IList<Vector3F> positions = positionsAttribute.Values;
            positions.Add(new Vector3F(0, 0, 0));
            positions.Add(new Vector3F(1, 0, 0));
            positions.Add(new Vector3F(0, 0, 1));

            indices.AddTriangle(new TriangleIndicesUnsignedShort(0, 1, 2));

            VertexArray va = m_Context.CreateVertexArray(mesh, sp.VertexAttributes, BufferHint.StaticDraw);

            ///////////////////////////////////////////////////////////////////

            RenderState renderState = new RenderState();
            renderState.FacetCulling.Enabled = false;
            renderState.DepthTest.Enabled = false;

            _drawState = new DrawState(renderState, sp, va);

            ///////////////////////////////////////////////////////////////////
            
            SceneState.Camera.ZoomToTarget(1);
        }

        public override void Render(Context context)
        {
            
            context.Clear(ClearState);

            context.Draw(PrimitiveType.Triangles, _drawState, SceneState);
        }

        
        private DrawState _drawState;
    }
}
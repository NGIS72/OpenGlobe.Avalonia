#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Drawing;

using OpenGlobe.Core;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;

namespace OpenGlobe.Examples
{
    sealed class SubdivisionSphere1 : SceneBase, IDisposable
    {
        public override void Load(Context context)
        {
            base.Load(context);
        
            SetCameraLookAtPoint(Ellipsoid.UnitSphere);
            
            string vs =
                @"#version 330

                  layout(location = og_positionVertexLocation) in vec4 position;
                  out vec3 worldPosition;
                  out vec3 positionToLight;
                  out vec3 positionToEye;

                  uniform mat4 og_modelViewPerspectiveMatrix;
                  uniform vec3 og_cameraEye;
                  uniform vec3 og_cameraLightPosition;

                  void main()                     
                  {
                        gl_Position = og_modelViewPerspectiveMatrix * position; 

                        worldPosition = position.xyz;
                        positionToLight = og_cameraLightPosition - worldPosition;
                        positionToEye = og_cameraEye - worldPosition;
                  }";

            string fs =
                @"#version 330
                 
                  in vec3 worldPosition;
                  in vec3 positionToLight;
                  in vec3 positionToEye;
                  out vec3 fragmentColor;

                  uniform vec4 og_diffuseSpecularAmbientShininess;
                  uniform sampler2D og_texture0;

                  float LightIntensity(vec3 normal, vec3 toLight, vec3 toEye, vec4 diffuseSpecularAmbientShininess)
                  {
                      vec3 toReflectedLight = reflect(-toLight, normal);

                      float diffuse = max(dot(toLight, normal), 0.0);
                      float specular = max(dot(toReflectedLight, toEye), 0.0);
                      specular = pow(specular, diffuseSpecularAmbientShininess.w);

                      return (diffuseSpecularAmbientShininess.x * diffuse) +
                             (diffuseSpecularAmbientShininess.y * specular) +
                              diffuseSpecularAmbientShininess.z;
                  }

                  vec2 ComputeTextureCoordinates(vec3 normal)
                  {
                      return vec2(atan(normal.y, normal.x) * og_oneOverTwoPi + 0.5, asin(normal.z) * og_oneOverPi + 0.5);
                  }

                  void main()
                  {
                      vec3 normal = normalize(worldPosition);
                      float intensity = LightIntensity(normal,  normalize(positionToLight), normalize(positionToEye), og_diffuseSpecularAmbientShininess);
                      fragmentColor = intensity * texture(og_texture0, ComputeTextureCoordinates(normal)).rgb;
                  }";
            ShaderProgram sp = context.Device.CreateShaderProgram(vs, fs);

            ///////////////////////////////////////////////////////////////////

            Mesh mesh = SubdivisionSphereTessellatorSimple.Compute(5);
            VertexArray va = context.CreateVertexArray(mesh, sp.VertexAttributes, BufferHint.StaticDraw);
            _primitiveType = mesh.PrimitiveType;

            ///////////////////////////////////////////////////////////////////

            RenderState renderState = new RenderState();
            //_renderState.RasterizationMode = RasterizationMode.Line;
            renderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

            _drawState = new DrawState(renderState, sp, va);

            ///////////////////////////////////////////////////////////////////

            
            _texture = context.Device.CreateTexture2D("NE2_50M_SR_W_4096.jpg", TextureFormat.RedGreenBlue8, false);

            SceneState.Camera.ZoomToTarget(1);
        }

        public override void Render(Context context)
        {
            context.Clear(ClearState);
            context.TextureUnits[0].Texture = _texture;
            context.TextureUnits[0].TextureSampler = context.Device.TextureSamplers.LinearClamp;
            context.Draw(_primitiveType, _drawState, SceneState);
        }

        private DrawState _drawState;
        private Texture2D _texture;
        private PrimitiveType _primitiveType;
    }
}
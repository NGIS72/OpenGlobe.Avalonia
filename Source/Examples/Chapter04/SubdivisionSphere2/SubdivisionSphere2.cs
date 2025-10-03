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
using Avalonia.Media;
using OpenGlobe.Core;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;

namespace OpenGlobe.Examples
{
    sealed class SubdivisionSphere2 : SceneBase, IDisposable
    {
        public override void Load(Context context)
        {
            base.Load(context);
            ClearState.Color = Colors.Gray;
            SetCameraLookAtPoint(Ellipsoid.UnitSphere);

            string vs =
                @"#version 330

                  layout(location = og_positionVertexLocation) in vec4 position;
                  layout(location = og_normalVertexLocation) in vec3 normal;
                  layout(location = og_textureCoordinateVertexLocation) in vec2 textureCoordinate;

                  out vec3 positionToLight;
                  out vec3 positionToEye;
                  out vec3 surfaceNormal;
                  out vec2 surfaceTextureCoordinate;

                  uniform mat4 og_modelViewPerspectiveMatrix;
                  uniform vec3 og_cameraEye;
                  uniform vec3 og_cameraLightPosition;

                  void main()                     
                  {
                        gl_Position = og_modelViewPerspectiveMatrix * position; 

                        positionToLight = og_cameraLightPosition - position.xyz;
                        positionToEye = og_cameraEye - position.xyz;

                        surfaceNormal = normal;
                        surfaceTextureCoordinate = textureCoordinate;
                  }";
            string fs =
                @"#version 330
                 
                  in vec3 positionToLight;
                  in vec3 positionToEye;
                  in vec3 surfaceNormal;
                  in vec2 surfaceTextureCoordinate;

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

                  void main()
                  {
                      vec3 normal = normalize(surfaceNormal);
                      float intensity = LightIntensity(normal,  normalize(positionToLight), normalize(positionToEye), og_diffuseSpecularAmbientShininess);
                      fragmentColor = intensity * texture(og_texture0, surfaceTextureCoordinate).rgb;
                  }";
            ShaderProgram sp = context.Device.CreateShaderProgram(vs, fs);

            Mesh mesh = SubdivisionSphereTessellator.Compute(5, SubdivisionSphereVertexAttributes.All);
            VertexArray va = context.CreateVertexArray(mesh, sp.VertexAttributes, BufferHint.StaticDraw);
            _primitiveType = mesh.PrimitiveType;

            RenderState renderState = new RenderState();
            renderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

            _drawState = new DrawState(renderState, sp, va);

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
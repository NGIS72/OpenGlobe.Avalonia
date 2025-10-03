#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
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
    sealed class NightLights : SceneBase, IDisposable
    {
        
        public override void Load(Context context)
        {
            base.Load(context);
            
            Camera = new CameraLookAtPoint(SceneState.Camera, Ellipsoid.ScaledWgs84);
            
            string vs =
                @"#version 330

                  layout(location = og_positionVertexLocation) in vec4 position;
                  out vec3 worldPosition;
                  out vec3 positionToLight;
                  out vec3 positionToEye;

                  uniform mat4 og_modelViewPerspectiveMatrix;
                  uniform vec3 og_cameraEye;
                  uniform vec3 og_sunPosition;

                  void main()                     
                  {
                        gl_Position = og_modelViewPerspectiveMatrix * position; 

                        worldPosition = position.xyz;
                        positionToLight = og_sunPosition - worldPosition;
                        positionToEye = og_cameraEye - worldPosition;
                  }";

            string fs =
                @"#version 330
                 
                  in vec3 worldPosition;
                  in vec3 positionToLight;
                  in vec3 positionToEye;
                  out vec3 fragmentColor;

                  uniform vec4 og_diffuseSpecularAmbientShininess;
                  uniform sampler2D og_texture0;                    // Day
                  uniform sampler2D og_texture1;                    // Night

                  uniform float u_blendDuration;
                  uniform float u_blendDurationScale;

                  float LightIntensity(vec3 normal, vec3 toLight, vec3 toEye, float diffuseDot, vec4 diffuseSpecularAmbientShininess)
                  {
                      vec3 toReflectedLight = reflect(-toLight, normal);

                      float diffuse = max(diffuseDot, 0.0);
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

                  vec3 NightColor(vec3 normal)
                  {
                      return texture(og_texture1, ComputeTextureCoordinates(normal)).rgb;
                  }

                  vec3 DayColor(vec3 normal, vec3 toLight, vec3 toEye, float diffuseDot, vec4 diffuseSpecularAmbientShininess)
                  {
                      float intensity = LightIntensity(normal, toLight, toEye, diffuseDot, diffuseSpecularAmbientShininess);
                      return intensity * texture(og_texture0, ComputeTextureCoordinates(normal)).rgb;
                  }

                  void main()
                  {
                      vec3 normal = normalize(worldPosition);
                      vec3 toLight = normalize(positionToLight);
                      float diffuse = dot(toLight, normal);

                      if (diffuse > u_blendDuration)
                      {
                          fragmentColor = DayColor(normal, toLight, normalize(positionToEye), diffuse, og_diffuseSpecularAmbientShininess);
                      }
                      else if (diffuse < -u_blendDuration)
                      {
                          fragmentColor = NightColor(normal);
                      }
                      else
                      {
                          vec3 night = NightColor(normal);
                          vec3 day = DayColor(normal, toLight, normalize(positionToEye), diffuse, og_diffuseSpecularAmbientShininess);
                          fragmentColor = mix(night, day, (diffuse + u_blendDuration) * u_blendDurationScale);
                      }
                  }";
            ShaderProgram sp = context.Device.CreateShaderProgram(vs, fs);

            float blendDurationScale = 0.1f;
            ((Uniform<float>)sp.Uniforms["u_blendDuration"]).Value = blendDurationScale;
            ((Uniform<float>)sp.Uniforms["u_blendDurationScale"]).Value = 1 / (2 * blendDurationScale);

            Mesh mesh = SubdivisionEllipsoidTessellator.Compute(Ellipsoid.ScaledWgs84, 5, SubdivisionEllipsoidVertexAttributes.Position);
            VertexArray va = context.CreateVertexArray(mesh, sp.VertexAttributes, BufferHint.StaticDraw);
            _primitiveType = mesh.PrimitiveType;

            RenderState renderState = new RenderState();
            renderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

            _drawState = new DrawState(renderState, sp, va);

            _dayTexture = context.Device.CreateTexture2D("world.topo.200412.3x5400x2700.jpg", TextureFormat.RedGreenBlue8, false);
            _nightTexture = context.Device.CreateTexture2D("land_ocean_ice_lights_2048.jpg", TextureFormat.RedGreenBlue8, false);

            SceneState.DiffuseIntensity = 0.5f;
            SceneState.SpecularIntensity = 0.15f;
            SceneState.AmbientIntensity = 0.35f;
            SceneState.Camera.ZoomToTarget(1);
        }
        public override void Render(Context context)
        {
            context.Clear(ClearState);
            context.TextureUnits[0].Texture = _dayTexture;
            context.TextureUnits[0].TextureSampler = context.Device.TextureSamplers.LinearClamp;
            context.TextureUnits[1].Texture = _nightTexture;
            context.TextureUnits[1].TextureSampler = context.Device.TextureSamplers.LinearClamp;
            context.Draw(_primitiveType, _drawState, SceneState);
        }

        private DrawState _drawState;
        private Texture2D _dayTexture;
        private Texture2D _nightTexture;
        private PrimitiveType _primitiveType;
    }
}
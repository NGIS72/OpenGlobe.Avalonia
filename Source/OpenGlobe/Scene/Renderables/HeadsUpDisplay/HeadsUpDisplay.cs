﻿#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using OpenGlobe.Core;
using OpenGlobe.Renderer;
using Half = OpenGlobe.Core.Half;

namespace OpenGlobe.Scene
{
    public sealed class HeadsUpDisplay : IDisposable, IRenderable
    {
        private Context m_Context;
        public HeadsUpDisplay(Context context)
        {
            m_Context = context;
            RenderState renderState = new RenderState();
            renderState.FacetCulling.Enabled = false;
            renderState.DepthTest.Enabled = false;
            renderState.Blending.Enabled = true;
            renderState.Blending.SourceRGBFactor = SourceBlendingFactor.SourceAlpha;
            renderState.Blending.SourceAlphaFactor = SourceBlendingFactor.SourceAlpha;
            renderState.Blending.DestinationRGBFactor = DestinationBlendingFactor.OneMinusSourceAlpha;
            renderState.Blending.DestinationAlphaFactor = DestinationBlendingFactor.OneMinusSourceAlpha;

            ShaderProgram sp = m_Context.Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Scene.Renderables.HeadsUpDisplay.Shaders.HeadsUpDisplayVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Renderables.HeadsUpDisplay.Shaders.HeadsUpDisplayGS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Renderables.HeadsUpDisplay.Shaders.HeadsUpDisplayFS.glsl"));
            _colorUniform = (Uniform<Vector3F>)sp.Uniforms["u_color"];
            _originScaleUniform = (Uniform<Vector2F>)sp.Uniforms["u_originScale"];

            _drawState = new DrawState(renderState, sp, null);

            Color = Avalonia.Media.Colors.White;
            HorizontalOrigin = HorizontalOrigin.Left;
            VerticalOrigin = VerticalOrigin.Bottom;
            _positionDirty = true;
        }

        private void CreateVertexArray()
        {
            // TODO:  Buffer hint.
            _positionBuffer = m_Context.Device.CreateVertexBuffer(BufferHint.StaticDraw, SizeInBytes<Vector2F>.Value);

            VertexBufferAttribute positionAttribute = new VertexBufferAttribute(
                _positionBuffer, ComponentDatatype.Float, 2);

            _drawState.VertexArray = m_Context.CreateVertexArray();
            _drawState.VertexArray.Attributes[_drawState.ShaderProgram.VertexAttributes["position"].Location] = positionAttribute;
         }

        private void Update(Context context)
        {
            if (_positionDirty)
            {
                DisposeVertexArray();
                CreateVertexArray();

                Vector2F[] positions = new Vector2F[] { _position.ToVector2F() };
                _positionBuffer.CopyFromSystemMemory(positions);

                _positionDirty = false;
            }
        }

        public void Render(Context context, SceneState sceneState)
        {
            Verify.ThrowIfNull(context);
            Verify.ThrowInvalidOperationIfNull(Texture, "Texture");

            Update(context);

            if (_drawState.VertexArray != null)
            {
                context.TextureUnits[0].Texture = Texture;
                context.TextureUnits[0].TextureSampler = m_Context.Device.TextureSamplers.LinearClamp;
                context.Draw(PrimitiveType.Points, _drawState, sceneState);
            }
        }

        public Texture2D Texture { get; set; }

        public Avalonia.Media.Color Color
        {
            get { return _color; }

            set
            {
                _color = value;
                _colorUniform.Value = new Vector3F(_color.R / 255.0f, _color.G / 255.0f, _color.B / 255.0f);
            }
        }

        public HorizontalOrigin HorizontalOrigin
        {
            get { return _horizontalOrigin; }
            set
            {
                _horizontalOrigin = value;
                _originScaleUniform.Value = new Vector2F(
                    _originScale[(int)value],
                    _originScaleUniform.Value.Y);
            }
        }

        public VerticalOrigin VerticalOrigin
        {
            get { return _verticalOrigin; }
            set
            {
                _verticalOrigin = value;
                _originScaleUniform.Value = new Vector2F(
                    _originScaleUniform.Value.X,
                    _originScale[(int)value]);
            }
        }

        public Vector2D Position
        {
            get { return _position; }

            set
            {
                if (_position != value)
                {
                    _position = value;
                    _positionDirty = true;
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _drawState.ShaderProgram.Dispose();
            DisposeVertexArray();
        }

        #endregion

        private void DisposeVertexArray()
        {
            if (_positionBuffer != null)
            {
                _positionBuffer.Dispose();
                _positionBuffer = null;
            }

            if (_drawState.VertexArray != null)
            {
                _drawState.VertexArray.Dispose();
                _drawState.VertexArray = null;
            }
        }

        private readonly DrawState _drawState;
        private readonly Uniform<Vector3F> _colorUniform;
        private readonly Uniform<Vector2F> _originScaleUniform;
        private Avalonia.Media.Color _color;

        private Vector2D _position;
        private bool _positionDirty;
        private HorizontalOrigin _horizontalOrigin;
        private VerticalOrigin _verticalOrigin;

        private VertexBuffer _positionBuffer;

        private static readonly Half[] _originScale = new Half[] { new Half(0.0), new Half(1.0), new Half(-1.0) };

        
    }
}
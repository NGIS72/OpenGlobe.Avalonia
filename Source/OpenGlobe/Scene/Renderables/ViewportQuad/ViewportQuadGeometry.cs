#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using OpenGlobe.Core;
using OpenGlobe.Renderer;

namespace OpenGlobe.Scene
{
    internal sealed class ViewportQuadGeometry : IDisposable
    {
        private Context m_Context;
        public ViewportQuadGeometry(Context context)
        {
            m_Context = context;
            _positionBuffer = m_Context.Device.CreateVertexBuffer(BufferHint.StaticDraw, 4 * SizeInBytes<Vector2F>.Value);
            _textureCoordinatesBuffer = m_Context.Device.CreateVertexBuffer(BufferHint.StaticDraw, 4 * SizeInBytes<Vector2H>.Value);
        }

        internal void Update(Context context, ShaderProgram sp)
        {
            if (_va == null)
            {
                VertexBufferAttribute positionAttribute = new VertexBufferAttribute(
                    _positionBuffer, ComponentDatatype.Float, 2);
                VertexBufferAttribute textureCoordinatesAttribute = new VertexBufferAttribute(
                    _textureCoordinatesBuffer, ComponentDatatype.HalfFloat, 2);

                _va = context.CreateVertexArray();
                _va.Attributes[sp.VertexAttributes["position"].Location] = positionAttribute;
                _va.Attributes[sp.VertexAttributes["textureCoordinates"].Location] = textureCoordinatesAttribute;
            }

            if (_viewport != context.Viewport)
            {
                //
                // Bottom and top swapped:  MS -> OpenGL
                //
                float left = context.Viewport.X;
                float bottom = context.Viewport.Y;
                float right = context.Viewport.Right;
                float top = context.Viewport.Bottom;

                Vector2F[] positions = new Vector2F[] 
                { 
                    new Vector2F(left, bottom), 
                    new Vector2F(right, bottom), 
                    new Vector2F(left, top), 
                    new Vector2F(right, top)
                };
                _positionBuffer.CopyFromSystemMemory(positions);

                Vector2H[] textureCoordinates = new Vector2H[] 
                { 
                    new Vector2H(0, 0), 
                    new Vector2H(1, 0), 
                    new Vector2H(0, 1), 
                    new Vector2H(1, 1)
                };
                _textureCoordinatesBuffer.CopyFromSystemMemory(textureCoordinates);

                _viewport = context.Viewport;
            }
        }

        internal VertexArray VertexArray => _va;

        #region IDisposable Members

        public void Dispose()
        {
            _positionBuffer.Dispose();
            _textureCoordinatesBuffer.Dispose();

            if (_va != null)
            {
                _va.Dispose();
            }
        }

        #endregion

        private Avalonia.PixelRect _viewport;
        private readonly VertexBuffer _positionBuffer;
        private readonly VertexBuffer _textureCoordinatesBuffer;
        private VertexArray _va;
    }
}
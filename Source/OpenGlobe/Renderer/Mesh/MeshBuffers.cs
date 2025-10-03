#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Renderer
{
    /// <summary>
    /// Does not own vertex and index buffers.  They must be disposed.
    /// </summary>
    public class MeshBuffers
    {
        public MeshBuffers(Context context)
        {
            _attributes = new MeshVertexBufferAttributes(context);
        }
        public virtual VertexBufferAttributes Attributes => _attributes;

        public IndexBuffer IndexBuffer { get; set; }

        private MeshVertexBufferAttributes _attributes;
    }
}

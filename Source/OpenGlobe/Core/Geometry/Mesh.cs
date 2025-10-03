#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Core
{
    public enum PrimitiveType
    {
        Points,
        Lines,
        LineLoop,
        LineStrip,
        Triangles,
        TriangleStrip,
        TriangleFan,
        LinesAdjacency,
        LineStripAdjacency,
        TrianglesAdjacency,
        TriangleStripAdjacency
    }

    public enum WindingOrder
    {
        Clockwise,
        Counterclockwise
    }

    public class Mesh
    {
        public Mesh()
        {
            _attributes = new VertexAttributeCollection();
        }

        public VertexAttributeCollection Attributes => _attributes;

        public IndicesBase Indices { get; set; }

        public PrimitiveType PrimitiveType { get; set; }
        public WindingOrder FrontFaceWindingOrder { get; set; }

        private VertexAttributeCollection _attributes;
    }
}

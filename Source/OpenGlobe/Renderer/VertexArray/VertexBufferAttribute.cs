#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Renderer
{
    public class VertexBufferAttribute
    {
        public VertexBufferAttribute(
            VertexBuffer vertexBuffer,
            ComponentDatatype componentDatatype,
            int numberOfComponents)
            : this(vertexBuffer, componentDatatype, numberOfComponents, false, 0, 0)
        {
        }

        public VertexBufferAttribute(
            VertexBuffer vertexBuffer,
            ComponentDatatype componentDatatype,
            int numberOfComponents,
            bool normalize,
            int offsetInBytes,
            int strideInBytes)
        {
            if (numberOfComponents <= 0)
            {
                throw new ArgumentOutOfRangeException("numberOfComponents", "numberOfComponents must be greater than zero.");
            }

            if (offsetInBytes < 0)
            {
                throw new ArgumentOutOfRangeException("offsetInBytes", "offsetInBytes must be greater than or equal to zero.");
            }

            if (strideInBytes < 0)
            {
                throw new ArgumentOutOfRangeException("stride", "stride must be greater than or equal to zero.");
            }

            _vertexBuffer = vertexBuffer;
            _componentDatatype = componentDatatype;
            _numberOfComponents = numberOfComponents;
            _normalize = normalize;
            _offsetInBytes = offsetInBytes;

            if (strideInBytes == 0)
            {
                //
                // Tightly packed
                //
                _strideInBytes = numberOfComponents * VertexArraySizes.SizeOf(componentDatatype);
            }
            else
            {
                _strideInBytes = strideInBytes;
            }
        }

        public VertexBuffer VertexBuffer => _vertexBuffer;

        public ComponentDatatype ComponentDatatype => _componentDatatype;

        public int NumberOfComponents => _numberOfComponents;

        public bool Normalize => _normalize;

        public int OffsetInBytes => _offsetInBytes;

        public int StrideInBytes => _strideInBytes;

        private VertexBuffer _vertexBuffer;
        private ComponentDatatype _componentDatatype;
        private int _numberOfComponents;
        private bool _normalize;
        private int _offsetInBytes;
        private int _strideInBytes;
    }
}

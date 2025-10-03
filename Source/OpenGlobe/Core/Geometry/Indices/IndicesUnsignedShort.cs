#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Core
{
    public class IndicesUnsignedShort : IndicesBase
    {
        public IndicesUnsignedShort()
            : base(IndicesType.UnsignedShort)
        {
            _values = new List<ushort>();
        }

        public IndicesUnsignedShort(int capacity)
            : base(IndicesType.UnsignedShort)
        {
            _values = new List<ushort>(capacity);
        }

        public IList<ushort> Values => _values;

        public void AddTriangle(TriangleIndicesUnsignedShort triangle)
        {
            _values.Add(triangle.UI0);
            _values.Add(triangle.UI1);
            _values.Add(triangle.UI2);
        }

        private List<ushort> _values;
    }
}

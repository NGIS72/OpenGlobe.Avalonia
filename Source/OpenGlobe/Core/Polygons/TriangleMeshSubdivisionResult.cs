#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Core
{
    public class TriangleMeshSubdivisionResult
    {
        internal TriangleMeshSubdivisionResult(ICollection<Vector3D> positions, IndicesUnsignedInt indices)
        {
            _positions = positions;
            _indices = indices;
        }

        public ICollection<Vector3D> Positions => _positions;

        public IndicesUnsignedInt Indices => _indices;

        private readonly ICollection<Vector3D> _positions;
        private readonly IndicesUnsignedInt _indices;
    }
}
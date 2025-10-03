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
    public class EllipsoidTangentPlane
    {
        public EllipsoidTangentPlane(Ellipsoid ellipsoid, IEnumerable<Vector3D> positions)
        {
            if (ellipsoid == null)
            {
                throw new ArgumentNullException("ellipsoid");
            }

            if (positions == null)
            {
                throw new ArgumentNullException("positions");
            }

            if (!CollectionAlgorithms.EnumerableCountGreaterThanOrEqual(positions, 1))
            {
                throw new ArgumentOutOfRangeException("positions", "At least one position is required.");
            }

            AxisAlignedBoundingBox box = new AxisAlignedBoundingBox(positions);

            _origin = ellipsoid.ScaleToGeodeticSurface(box.Center);
            _normal = ellipsoid.GeodeticSurfaceNormal(_origin);
            _d = -_origin.Dot(_origin);
            _yAxis = _origin.Cross(_origin.MostOrthogonalAxis).Normalize();
            _xAxis = _yAxis.Cross(_origin).Normalize();
        }

        public ICollection<Vector2D> ComputePositionsOnPlane(IEnumerable<Vector3D> positions)
        {
            if (positions == null)
            {
                throw new ArgumentNullException("positions");
            }

            IList<Vector2D> positionsOnPlane = new List<Vector2D>(CollectionAlgorithms.EnumerableCount(positions));

            foreach (Vector3D position in positions)
            {
                Vector3D intersectionPoint;

                if (IntersectionTests.TryRayPlane(Vector3D.Zero, position.Normalize(), _normal, _d, out intersectionPoint))
                {
                    Vector3D v = intersectionPoint - _origin;
                    positionsOnPlane.Add(new Vector2D(_xAxis.Dot(v), _yAxis.Dot(v)));
                }
                else
                {
                    // Ray does not intersect plane
                }
            }

            return positionsOnPlane;
        }

        public Vector3D Origin => _origin;
        public Vector3D Normal => _normal;
        public double D => _d;
        public Vector3D XAxis => _xAxis;
        public Vector3D YAxis => _yAxis;

        private Vector3D _origin;
        private Vector3D _normal;
        private double _d;
        private Vector3D _xAxis;
        private Vector3D _yAxis;
    }
}

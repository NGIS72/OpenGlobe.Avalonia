#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using OpenGlobe.Core;

namespace OpenGlobe.Scene
{
    public class RasterLevel
    {
        public RasterLevel(RasterSource source, int level, GeodeticExtent extent, int longitudePosts, int latitudePosts, int longitudePostsPerTile, int latitudePostsPerTile)
        {
            _source = source;
            _level = level;
            _extent = extent;
            _longitudePosts = longitudePosts;
            _latitudePosts = latitudePosts;
            _longitudePostsPerTile = longitudePostsPerTile;
            _latitudePostsPerTile = latitudePostsPerTile;
            _postDeltaLongitude = (_extent.East - _extent.West) / (longitudePosts - 1);
            _postDeltaLatitude = (_extent.North - _extent.South) / (latitudePosts - 1);
        }

        public RasterSource Source => _source;

        public int Level => _level;

        public GeodeticExtent Extent => _extent;

        public int LongitudePosts => _longitudePosts;

        public int LatitudePosts => _latitudePosts;

        public int LongitudePostsPerTile => _longitudePostsPerTile;

        public int LatitudePostsPerTile => _latitudePostsPerTile;

        public double PostDeltaLongitude => _postDeltaLongitude;

        public double PostDeltaLatitude => _postDeltaLatitude;

        public double LongitudeToIndex(double longitude)
        {
            return (longitude - _extent.West) / _postDeltaLongitude;
        }

        public double LatitudeToIndex(double latitude)
        {
            return (latitude - _extent.South) / _postDeltaLatitude;
        }

        public double IndexToLongitude(int longitudeIndex)
        {
            return _extent.West + longitudeIndex * _postDeltaLongitude;
        }

        public double IndexToLatitude(int latitudeIndex)
        {
            return _extent.South + latitudeIndex * _postDeltaLatitude;
        }

        public RasterTileRegion[] GetTilesInExtent(int west, int south, int east, int north)
        {
            int tileXStart = west / LongitudePostsPerTile;
            int tileXStop = east / LongitudePostsPerTile;

            if (west < 0)
            {
                --tileXStart;
            }
            if (east < 0)
            {
                --tileXStop;
            }

            int tileYStart = south / LatitudePostsPerTile;
            int tileYStop = north / LatitudePostsPerTile;

            if (south < 0)
            {
                --tileYStart;
            }
            if (north < 0)
            {
                --tileYStop;
            }

            int tileWidth = tileXStop - tileXStart + 1;
            int tileHeight = tileYStop - tileYStart + 1;

            RasterTileRegion[] result = new RasterTileRegion[tileWidth * tileHeight];
            int resultIndex = 0;

            for (int tileY = tileYStart; tileY <= tileYStop; ++tileY)
            {
                int tileYOrigin = tileY * LatitudePostsPerTile;

                int currentSouth = south - tileYOrigin;
                if (currentSouth < 0)
                    currentSouth = 0;

                int currentNorth = north - tileYOrigin;
                if (currentNorth >= LatitudePostsPerTile)
                    currentNorth = LatitudePostsPerTile - 1;

                for (int tileX = tileXStart; tileX <= tileXStop; ++tileX)
                {
                    int tileXOrigin = tileX * LongitudePostsPerTile;

                    int currentWest = west - tileXOrigin;
                    if (currentWest < 0)
                        currentWest = 0;

                    int currentEast = east - tileXOrigin;
                    if (currentEast >= LongitudePostsPerTile)
                        currentEast = LongitudePostsPerTile - 1;

                    RasterTileIdentifier tileID = new RasterTileIdentifier(_level, tileX, tileY);
                    RasterTile tile = Source.GetTile(tileID);
                    result[resultIndex] = new RasterTileRegion(tile, currentWest, currentSouth, currentEast, currentNorth);
                    ++resultIndex;
                }
            }

            return result;
        }

        private readonly RasterSource _source;
        private readonly int _level;
        private readonly GeodeticExtent _extent;
        private readonly int _longitudePosts;
        private readonly int _latitudePosts;
        private readonly int _longitudePostsPerTile;
        private readonly int _latitudePostsPerTile;
        private readonly double _postDeltaLongitude;
        private readonly double _postDeltaLatitude;
    }
}

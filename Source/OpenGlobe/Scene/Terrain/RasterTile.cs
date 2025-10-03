#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using OpenGlobe.Renderer;

namespace OpenGlobe.Scene
{
    public class RasterTile
    {
        public RasterTile(RasterSource source, RasterTileIdentifier identifier)
        {
            _level = source.Levels[identifier.Level];
            _identifier = identifier;
            _west = identifier.X * Level.LongitudePostsPerTile;
            _east = Math.Min(_west + Level.LongitudePostsPerTile, Level.LongitudePosts) - 1;
            _south = identifier.Y * Level.LatitudePostsPerTile;
            _north = Math.Min(_south + Level.LatitudePostsPerTile, Level.LatitudePosts) - 1;
        }

        public RasterSource Source => _level.Source;

        public RasterLevel Level => _level;

        public RasterTileIdentifier Identifier => _identifier;

        public RasterTile SouthwestChild
        {
            get
            {
                RasterTileIdentifier current = Identifier;
                RasterTileIdentifier child = new RasterTileIdentifier(current.Level + 1, current.X * 2, current.Y * 2);
                return Source.GetTile(child);
            }
        }

        public RasterTile SoutheastChild
        {
            get
            {
                RasterTileIdentifier current = Identifier;
                RasterTileIdentifier child = new RasterTileIdentifier(current.Level + 1, current.X * 2 + 1, current.Y * 2);
                return Source.GetTile(child);
            }
        }

        public RasterTile NorthwestChild
        {
            get
            {
                RasterTileIdentifier current = Identifier;
                RasterTileIdentifier child = new RasterTileIdentifier(current.Level + 1, current.X * 2, current.Y * 2 + 1);
                return Source.GetTile(child);
            }
        }

        public RasterTile NortheastChild
        {
            get
            {
                RasterTileIdentifier current = Identifier;
                RasterTileIdentifier child = new RasterTileIdentifier(current.Level + 1, current.X * 2 + 1, current.Y * 2 + 1);
                return Source.GetTile(child);
            }
        }

        /// <summary>
        /// Gets the index in the overall level of the westernmost post in this tile.
        /// </summary>
        public int West => _west;

        /// <summary>
        /// Gets the index in the overall level of the southernmost post in this tile.
        /// </summary>
        public int South => _south;

        /// <summary>
        /// Gets the index in the overall level of the easternmost post in this tile.
        /// </summary>
        public int East => _east;

        /// <summary>
        /// Gets the index in the overall level of the northernmost post in this tile.
        /// </summary>
        public int North => _north;

        /// <summary>
        /// Gets the width (east/west) of the tile, in posts.
        /// </summary>
        public int Width => East - West + 1;

        /// <summary>
        /// Gets the height (north/south) of the tile, in posts.
        /// </summary>
        public int Height => North - South + 1;

        public LazyTexture2D LoadTexture()
        {
            return Source.LoadTileTexture(_identifier);
        }

        private RasterTileIdentifier _identifier;
        private RasterLevel _level;
        private int _west;
        private int _south;
        private int _east;
        private int _north;
    }
}

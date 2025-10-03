#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Scene
{
    internal class ClipmapUpdate
    {
        public ClipmapUpdate(ClipmapLevel level, int west, int south, int east, int north)
        {
            _level = level;
            _west = west;
            _south = south;
            _east = east;
            _north = north;
        }

        public ClipmapLevel Level => _level;

        public int West => _west;

        public int South => _south;

        public int East => _east;

        public int North => _north;

        public int Width => East - West + 1;

        public int Height => North - South + 1;

        /// <summary>
        /// Creates a new region which is equivalent to this one but with a one post buffer added
        /// around the perimeter.  The buffer will not cause the update region to exceed the bounds
        /// of the level's <see cref="ClipmapLevel.NextExtent"/>.
        /// </summary>
        /// <returns>The new region.</returns>
        public ClipmapUpdate AddBufferWithinLevelNextExtent()
        {
            return new ClipmapUpdate(
                _level,
                Math.Max(_west - 1, Level.NextExtent.West),
                Math.Max(_south - 1, Level.NextExtent.South),
                Math.Min(_east + 1, Level.NextExtent.East),
                Math.Min(_north + 1, Level.NextExtent.North));
        }

        private ClipmapLevel _level;
        private int _west;
        private int _south;
        private int _east;
        private int _north;
    }
}

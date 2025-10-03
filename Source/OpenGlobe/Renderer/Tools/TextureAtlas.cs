#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    public sealed class TextureAtlas : IDisposable
    {
        public TextureAtlas(IEnumerable<Bitmap> bitmaps)
            : this(bitmaps, 1)
        {
        }

        public TextureAtlas(IEnumerable<Bitmap> bitmaps, int borderWidthInPixels)
        {
            if (bitmaps == null)
            {
                throw new ArgumentNullException("bitmaps");
            }

            int numberOfBitmaps = CollectionAlgorithms.EnumerableCount(bitmaps);

            if (numberOfBitmaps == 0)
            {
                throw new ArgumentException("bitmaps does not contain any items.", "bitmaps");
            }

            List<AnnotatedBitmap> annotatedBitmaps = new List<AnnotatedBitmap>(numberOfBitmaps);

            var pixelFormat = SKColorType.Rgba8888;//SKColorType.Unknown;

            int j = 0;
            foreach (Bitmap b in bitmaps)
            {
                if (b == null)
                {
                    throw new ArgumentNullException("bitmaps", "An item in bitmaps is null.");
                }

                //if (pixelFormat == SKColorType.Unknown)
                //{
                //    pixelFormat = b.ColorType;
                //}
                //else if (b.ColorType != pixelFormat)
                //{
                //    throw new ArgumentException("All bitmaps must have the same PixelFormat.", "bitmaps");
                //}

                annotatedBitmaps.Add(new AnnotatedBitmap(b, j++));
            }

            if (pixelFormat == SKColorType.Unknown)
            {
                throw new ArgumentException("All bitmaps have PixelFormat.Undefined.", "bitmaps");
            }

            if (borderWidthInPixels < 0)
            {
                throw new ArgumentOutOfRangeException("borderWidthInPixels");
            }

            ///////////////////////////////////////////////////////////////////

            IList<Avalonia.Point> offsets = new List<Avalonia.Point>(numberOfBitmaps);
            int width = ComputeAtlasWidth(bitmaps, borderWidthInPixels);
            int xOffset = 0;
            int yOffset = 0;
            int rowHeight = 0;

            annotatedBitmaps.Sort(new BitmapMaximumToMinimumHeight());

            //
            // This could be packed more tightly using the algorithm in
            //
            //     http://www-ui.is.s.u-tokyo.ac.jp/~takeo/papers/i3dg2001.pdf
            //
            for (int i = 0; i < numberOfBitmaps; ++i)
            {
                Bitmap b = annotatedBitmaps[i].Bitmap;

                int widthIncrement = b.Width + borderWidthInPixels;

                if (xOffset + widthIncrement > width)
                {
                    xOffset = 0;
                    yOffset += rowHeight + borderWidthInPixels;
                }

                if (xOffset == 0)
                {
                    //
                    // The first bitmap of the row determines the row height.
                    // This is worst case since bitmaps are sorted by height.
                    //
                    rowHeight = b.Height;
                }

                offsets.Add(new Avalonia.Point(xOffset, yOffset));
                xOffset += widthIncrement;
            }
            int height = yOffset + rowHeight;

            ///////////////////////////////////////////////////////////////////

            RectangleH[] textureCoordinates = new RectangleH[annotatedBitmaps.Count];
            Bitmap bitmap = new Bitmap(width, height, pixelFormat, SKAlphaType.Premul);
            using (var canvas = new SKCanvas(bitmap))
            {
                double widthD = width;
                double heightD = height;

                for (int i = 0; i < numberOfBitmaps; ++i)
                {
                    var upperLeft = offsets[i];
                    AnnotatedBitmap b = annotatedBitmaps[i];

                    textureCoordinates[b.Index] = new RectangleH(
                        new Vector2H(                                                       // Lower Left
                            (double)upperLeft.X / widthD,
                            (heightD - (double)(upperLeft.Y + b.Bitmap.Height)) / heightD),
                        new Vector2H(                                                       // Upper Right
                            (double)(upperLeft.X + b.Bitmap.Width) / widthD,
                            (heightD - (double)upperLeft.Y) / heightD));
                    using (var image = SKImage.FromBitmap(b.Bitmap))
                    {
                        canvas.DrawImage(image, (int)upperLeft.X, (int)upperLeft.Y);
                    }
                }
            }
            
            _bitmap = bitmap;
            _textureCoordinates = new TextureCoordinateCollection(textureCoordinates);
            _borderWidth = borderWidthInPixels;
        }

        public Bitmap Bitmap => _bitmap;

        public TextureCoordinateCollection TextureCoordinates => _textureCoordinates;

        public int BorderWidth => _borderWidth;

        private static int ComputeAtlasWidth(IEnumerable<Bitmap> bitmaps, int borderWidthInPixels)
        {
            int maxWidth = 0;
            int area = 0;
            foreach (Bitmap b in bitmaps)
            {
                area += (b.Width + borderWidthInPixels) * (b.Height + borderWidthInPixels);
                maxWidth = Math.Max(maxWidth, b.Width);
            }

            return Math.Max((int)Math.Sqrt((double)area), maxWidth + borderWidthInPixels);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _bitmap.Dispose();
        }

        #endregion

        private readonly Bitmap _bitmap;
        private readonly TextureCoordinateCollection _textureCoordinates;
        private readonly int _borderWidth;

        private class AnnotatedBitmap
        {
            public AnnotatedBitmap(Bitmap bitmap, int index)
            {
                _bitmap = bitmap;
                _index = index;
            }

            public Bitmap Bitmap => _bitmap;
            public int Index => _index;

            private Bitmap _bitmap;
            private int _index;
        }

        private class BitmapMaximumToMinimumHeight : IComparer<AnnotatedBitmap>
        {
            public int Compare(AnnotatedBitmap left, AnnotatedBitmap right)
            {
                return right.Bitmap.Height - left.Bitmap.Height;
            }
        }
    }
}

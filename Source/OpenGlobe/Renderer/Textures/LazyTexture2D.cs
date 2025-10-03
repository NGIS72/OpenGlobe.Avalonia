#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using Avalonia.Media.Imaging;

namespace OpenGlobe.Renderer
{
    public class LazyTexture2D
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public TextureFormat Format { get; set; }

        public float[] Data { get; set; }

        public Bitmap Bitmap { get; set; }


        private Texture2D m_Texture;
        public Texture2D Get(Context context)
        {
            if (m_Texture != null) return m_Texture;
            if (Bitmap != null)
                return m_Texture = context.Device.CreateTexture2DRectangle(Bitmap, TextureFormat.RedGreenBlue8);

            if (Data != null)
            {
                var description = new Texture2DDescription(Width, Height, TextureFormat.Red32f, false);
                Texture2D texture = context.Device.CreateTexture2DRectangle(description);

                using (WritePixelBuffer wpb = context.Device.CreateWritePixelBuffer(PixelBufferHint.Stream, Width * Height * sizeof(float)))
                {
                    wpb.CopyFromSystemMemory(Data);
                    texture.CopyFromBuffer(wpb, ImageFormat.Red, ImageDatatype.Float);
                }

                return m_Texture= texture;
            }

            throw new NullReferenceException();
        }
    }
}

#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System.Text;
using OpenGlobe.Core;
using System.Net;
using OpenGlobe.Renderer;

namespace OpenGlobe.Scene
{
    public class WorldWindTerrainSource : RasterSource
    {
        private static HttpClient s_HttpClient = new HttpClient(
    new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (a, b, c, d) => true
    });
        //https://worldwind26.arc.nasa.gov/elev?request=getcapabilities&service=wms
        //LIST: GEBCO, NASA_SRTM30_900m_Tiled, 
        //aster_layer0, aster_layer1, aster_layer2, aster_layer3, aster_layer4, aster_layer5, aster_layer6, aster_layer7, aster_layer8
        //USGS-NED, NASA_SRTM30_900m_Tiled, 
        //private const string LAYER = "NASA_SRTM30_900m_Tiled";
        private const string LAYER = "NASA_SRTM30_900m_Tiled";
        private Context m_Context;
        
        public WorldWindTerrainSource(Context context) :
            this(context, new Uri($"https://worldwind26.arc.nasa.gov/elev?request=GetMap&service=wms&version=1.3&crs=EPSG:4326&layers={LAYER}&styles=&format=application/bil16&bgColor=-9999.0&width=150&height=150"))
        {
        }

        public WorldWindTerrainSource(Context context, Uri baseUri)
        {
            m_Context = context;

            _baseUri = baseUri;

            _levels = new RasterLevel[NumberOfLevels];
            _levelsCollection = new RasterLevelCollection(_levels);

            double deltaLongitude = LevelZeroDeltaLongitudeDegrees;
            double deltaLatitude = LevelZeroDeltaLatitudeDegrees;

            for (int i = 0; i < _levels.Length; ++i)
            {
                int longitudePosts = (int)Math.Round(360.0 / deltaLongitude) * TileLongitudePosts + 1;
                int latitudePosts = (int)Math.Round(180.0 / deltaLatitude) * TileLatitudePosts + 1;
                _levels[i] = new RasterLevel(this, i, _extent, longitudePosts, latitudePosts, TileLongitudePosts, TileLatitudePosts);
                deltaLongitude /= 2.0;
                deltaLatitude /= 2.0;
            }
        }

        public override GeodeticExtent Extent => _extent;

        public override RasterLevelCollection Levels => _levelsCollection;

        public int TileLongitudePosts => TileWidth;

        public int TileLatitudePosts => TileHeight;

        public override LazyTexture2D LoadTileTexture(RasterTileIdentifier identifier)
        {
            string cachePath = "worldwind26_elev\\" + identifier.Level.ToString();
            cachePath = Path.Combine(cachePath, identifier.X.ToString());
            string cacheFilename = Path.Combine(cachePath, identifier.Y.ToString() + ".bil");

            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }

            int heightsToRead = TileWidth * TileHeight;
            float[] result = new float[heightsToRead];
            byte[] data = null;

            if (File.Exists(cacheFilename))
            {
                data = File.ReadAllBytes(cacheFilename);
            }

            if (data == null || data.Length != heightsToRead * sizeof(short))
            {
                double divisor = Math.Pow(2.0, identifier.Level);
                double longitudeResolution = LevelZeroDeltaLongitudeDegrees / divisor;
                double latitudeResolution = LevelZeroDeltaLatitudeDegrees / divisor;

                double west = -180.0 + longitudeResolution * identifier.X;
                double east = -180.0 + longitudeResolution * (identifier.X + 1);
                double south = -90.0 + latitudeResolution * identifier.Y;
                double north = -90.0 + latitudeResolution * (identifier.Y + 1);

                StringBuilder query = new StringBuilder(_baseUri.AbsoluteUri);
                query.Append("&bbox=");
                query.Append(west.ToString("0.###########"));
                query.Append(',');
                query.Append(south.ToString("0.###########"));
                query.Append(',');
                query.Append(east.ToString("0.###########"));
                query.Append(',');
                query.Append(north.ToString("0.###########"));
                query.Append('&');

                string queryString = query.ToString();
                ++_tilesLoaded;
                Console.WriteLine("(" + _tilesLoaded + ") Downloading " + queryString);

                int bytesToRead = heightsToRead * 2;

                var response = s_HttpClient.Send(new HttpRequestMessage(HttpMethod.Get, queryString));
                var stream = response.Content.ReadAsStream();

                data = new byte[bytesToRead];

                using (FileStream file = new FileStream(cacheFilename, FileMode.Create, FileAccess.Write))
                {
                    int bytesRead = 0;
                    while (bytesRead < bytesToRead)
                    {
                        int bytesReadThisTime = stream.Read(data, bytesRead, bytesToRead - bytesRead);
                        if (bytesReadThisTime == 0)
                            break;
                        file.Write(data, bytesRead, bytesReadThisTime);
                        bytesRead += bytesReadThisTime;
                    }
                }
            }

            // Make the southwest corner the origin instead of the northwest.
            int index = 0;
            for (int row = TileHeight - 1; row >= 0; --row)
            {
                int rowIndex = row * TileWidth;
                for (int col = 0; col < TileWidth; ++col)
                {
                    result[index++] = BitConverter.ToInt16(data, 2 * (rowIndex + col));
                }
            }

            return PostsToLazyTexture(result);
        }

        private Texture2D PostsToTexture(float[] posts)
        {
            Texture2DDescription description = new Texture2DDescription(TileWidth, TileHeight, TextureFormat.Red32f, false);
            Texture2D texture = m_Context.Device.CreateTexture2DRectangle(description);

            using (WritePixelBuffer wpb = m_Context.Device.CreateWritePixelBuffer(PixelBufferHint.Stream, TileWidth * TileHeight * sizeof(float)))
            {
                wpb.CopyFromSystemMemory(posts);
                texture.CopyFromBuffer(wpb, ImageFormat.Red, ImageDatatype.Float);
            }

            return texture;
        }
        private LazyTexture2D PostsToLazyTexture(float[] posts)
        {
            return new LazyTexture2D
            {
                Data = posts,
                Width = TileWidth,
                Height = TileHeight,
                Format = TextureFormat.Red32f
            };
        }

        private Uri _baseUri;
        private RasterLevel[] _levels;
        private RasterLevelCollection _levelsCollection;
        private GeodeticExtent _extent = new GeodeticExtent(-180.0, -90.0, 180.0, 90.0);
        private int _tilesLoaded;

        private const int NumberOfLevels = 12;
        private const int TileWidth = 150;
        private const int TileHeight = 150;
        private const double LevelZeroDeltaLongitudeDegrees = 20.0;
        private const double LevelZeroDeltaLatitudeDegrees = 20.0;
    }
}

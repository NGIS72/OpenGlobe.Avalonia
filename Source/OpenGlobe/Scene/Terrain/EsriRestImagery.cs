using OpenGlobe.Core;
using OpenGlobe.Renderer;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OpenGlobe.Scene
{
    public class EsriRestImagery : RasterSource
    {
        private static HttpClient s_HttpClient = new HttpClient(
            new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (a, b, c, d) => true
            });

        public const string IMAGERY = "https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer";
        public const string TERRAIN_BASE = "https://server.arcgisonline.com/arcgis/rest/services/World_Terrain_Base/MapServer";
        public const string HILLSHADE = "https://server.arcgisonline.com/arcgis/rest/services/Elevation/World_Hillshade/MapServer";

        private Context m_Context;
        private string m_CacheRoot;
        public EsriRestImagery(Context context) :
            this(context, IMAGERY)
        {

        }
        
        public EsriRestImagery(Context context, string serviceUrl)
        {
            var i1 = serviceUrl.IndexOf("/MapServer", StringComparison.OrdinalIgnoreCase);
            if (i1 == -1) throw new NotSupportedException("serviceUrl is not esri service url format");
            var baseUrl = serviceUrl.Substring(0, i1);
            var i2 = baseUrl.LastIndexOf('/');
            m_CacheRoot = baseUrl.Substring(i2 + 1);
            m_Context = context;
            _baseUri = new Uri(baseUrl + "/MapServer/tile/");

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

        public bool HeightsMode { get; set; }

        public override GeodeticExtent Extent => _extent;

        public int TileLongitudePosts => 512;

        public int TileLatitudePosts => 512;

        public override RasterLevelCollection Levels => _levelsCollection;


        public override LazyTexture2D LoadTileTexture(RasterTileIdentifier identifier)
        {
            int level = identifier.Level;
            int longitudeIndex = identifier.X;
            int latitudeIndex = identifier.Y;

            string cachePath = m_CacheRoot;
            cachePath = Path.Combine(cachePath, level.ToString());
            cachePath = Path.Combine(cachePath, latitudeIndex.ToString());
            //string cacheFilename = Path.Combine(cachePath, longitudeIndex.ToString() + ".jpg");
            string cacheFilename = Path.Combine(cachePath, longitudeIndex.ToString() + ".png");

            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }


            try
            {
                if (!File.Exists(cacheFilename))
                {
                    // Esri tiles are numbered from the northwest instead of from the southwest.

                    StringBuilder query = new StringBuilder(_baseUri.AbsoluteUri);
                    //query.Append(level);
                    //query.Append('/');
                    //query.Append((1 << level) - latitudeIndex - 1);
                    //query.Append('/');
                    //query.Append(longitudeIndex);

                    //OSM
                    query.Append(level);
                    query.Append('/');
                    query.Append(longitudeIndex);
                    query.Append('/');
                    query.Append((1 << level) - latitudeIndex - 1);
                    query.Append(".png");


                    string queryString = query.ToString();
                    ++_tilesLoaded;
                    Console.WriteLine("(" + _tilesLoaded + ") Downloading " + queryString);

                    var response = s_HttpClient.Send(new HttpRequestMessage(HttpMethod.Get, queryString));
                    var stream = response.Content.ReadAsStream();
                    using (FileStream file = new FileStream(cacheFilename, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        const int bufferSize = 4096;
                        byte[] buffer = new byte[bufferSize];

                        int bytesRead = stream.Read(buffer, 0, bufferSize);
                        while (bytesRead > 0)
                        {
                            file.Write(buffer, 0, bytesRead);
                            bytesRead = stream.Read(buffer, 0, bufferSize);
                        }
                    }
                }
            }
            catch
            {

            }
            if (!File.Exists(cacheFilename)) throw new Exception("Texture file not found");


            var bitmap = Bitmap.Decode(cacheFilename);
            if (!TextureUtility.Supported(bitmap.ColorType))
            {
                bitmap = bitmap.Copy(SKColorType.Rgba8888);
            }

            //return m_Context.Device.CreateTexture2DRectangle(bitmap, TextureFormat.RedGreenBlue8);
            return new LazyTexture2D
            {
                Bitmap = bitmap,
                Width = bitmap.Width,
                Height = bitmap.Height,
                Format = TextureFormat.RedGreenBlue8
            };
        }

        private Uri _baseUri;
        private GeodeticExtent _extent = new GeodeticExtent(-180, -90, 180, 90);
        private int _tilesLoaded;
        private RasterLevel[] _levels;
        private RasterLevelCollection _levelsCollection;

        private const int NumberOfLevels = 16;
        private const double LevelZeroDeltaLongitudeDegrees = 180.0;
        private const double LevelZeroDeltaLatitudeDegrees = 180.0;
    }
}

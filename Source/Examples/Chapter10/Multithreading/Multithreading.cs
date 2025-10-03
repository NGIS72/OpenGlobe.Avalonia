#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion


using System;
using System.Threading;
using System.Collections.Generic;

using OpenGlobe.Core;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;

namespace OpenGlobe.Examples
{
    internal class ShapefileRequest
    {
        public ShapefileRequest(string filename, ShapefileAppearance appearance)
        {
            _filename = filename;
            _appearance = appearance;
        }

        public string Filename { get { return _filename; } }
        public ShapefileAppearance Appearance { get { return _appearance; } }

        private string _filename;
        private ShapefileAppearance _appearance;
    }

    internal class ShapefileWorker
    {
        public ShapefileWorker(Context context, Ellipsoid globeShape, MessageQueue doneQueue)
        {
            _context = context;
            _globeShape = globeShape;
            _doneQueue = doneQueue;
        }

        public void Process(object sender, MessageQueueEventArgs e)
        {
#if !SINGLE_THREADED
            _context.MakeCurrent();
#endif

            ShapefileRequest request = (ShapefileRequest)e.Message;
            ShapefileRenderer shapefile = new ShapefileRenderer(
                request.Filename, _context, _globeShape, request.Appearance);

#if !SINGLE_THREADED
            Fence fence = _context.Device.CreateFence();
            while (fence.ClientWait(0) == ClientWaitResult.TimeoutExpired)
            {
                Thread.Sleep(10);   // Other work, etc.
            }
#endif

            _doneQueue.Post(shapefile);
        }

        private readonly Context _context;
        private readonly Ellipsoid _globeShape;
        private readonly MessageQueue _doneQueue;
    }
    
    sealed class Multithreading : SceneBase, IDisposable
    {
        public override void Load(Context context)
        {
            base.Load(context);
        
            Ellipsoid globeShape = Ellipsoid.ScaledWgs84;

            SetCameraLookAtPoint(globeShape);
            
            _texture = context.Device.CreateTexture2D("NE2_50M_SR_W_4096.jpg", TextureFormat.RedGreenBlue8, false);

            _globe = new RayCastedGlobe(context);
            _globe.Shape = globeShape;
            _globe.Texture = _texture;
            _globe.UseAverageDepth = true;

            ///////////////////////////////////////////////////////////////////

            _doneQueue.MessageReceived += ProcessNewShapefile;

            _requestQueue.MessageReceived += new ShapefileWorker(Context, globeShape, _doneQueue).Process;

            // 2ND_EDITION:  Draw order
            _requestQueue.Post(new ShapefileRequest("110m_admin_0_countries.shp", 
                new ShapefileAppearance()));
            _requestQueue.Post(new ShapefileRequest("110m_admin_1_states_provinces_lines_shp.shp", 
                new ShapefileAppearance()));
            _requestQueue.Post(new ShapefileRequest("airprtx020.shp", 
                new ShapefileAppearance() { Bitmap = SKBitmap.Decode("paper-plane--arrow.png") }));
            _requestQueue.Post(new ShapefileRequest("amtrakx020.shp", 
                new ShapefileAppearance() { Bitmap = SKBitmap.Decode("car-red.png") }));
            _requestQueue.Post(new ShapefileRequest("110m_populated_places_simple.shp", 
                new ShapefileAppearance() { Bitmap = SKBitmap.Decode("032.png") }));

#if SINGLE_THREADED
            _requestQueue.ProcessQueue();
#else
            _requestQueue.StartInAnotherThread();
#endif

            ///////////////////////////////////////////////////////////////////

            SceneState.Camera.ZoomToTarget(globeShape.MaximumRadius);
        }


        public override void Render(Context context)
        {
            _doneQueue.ProcessQueue();

            context.Clear(ClearState);
            _globe.Render(context, SceneState);

            foreach (IRenderable shapefile in _shapefiles)
            {
                shapefile.Render(context, SceneState);
            }
        }

        public void ProcessNewShapefile(object sender, MessageQueueEventArgs e)
        {
            _shapefiles.Add((IRenderable)e.Message);
        }

        private RayCastedGlobe _globe;
        private Texture2D _texture;

        private IList<IRenderable> _shapefiles = new List<IRenderable>();
        private MessageQueue _requestQueue = new MessageQueue();
        private MessageQueue _doneQueue = new MessageQueue();
        
    }
}
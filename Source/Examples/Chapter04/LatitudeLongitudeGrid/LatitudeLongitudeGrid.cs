#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Drawing;
using System.Collections.Generic;

using OpenGlobe.Core;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;
using SkiaSharp;

namespace OpenGlobe.Examples
{
    sealed class LatitudeLongitudeGrid : SceneBase, IDisposable
    {
        private Context m_Context;
        public override void Load(Context context)
        {
            base.Load(context);
        
            m_Context = context;

            Ellipsoid globeShape = Ellipsoid.Wgs84;
            Camera = new CameraLookAtPoint(SceneState.Camera, globeShape);
            

            SceneState.Camera.PerspectiveNearPlaneDistance = 0.01 * globeShape.MaximumRadius;
            SceneState.Camera.PerspectiveFarPlaneDistance = 10.0 * globeShape.MaximumRadius;
            SceneState.Camera.ZoomToTarget(globeShape.MaximumRadius);

            ///////////////////////////////////////////////////////////////////

            IList<GridResolution> gridResolutions = new List<GridResolution>();
            gridResolutions.Add(new GridResolution(
                new Interval(0, 1000000, IntervalEndpoint.Closed, IntervalEndpoint.Open),
                new Vector2D(0.005, 0.005)));
            gridResolutions.Add(new GridResolution(
                new Interval(1000000, 2000000, IntervalEndpoint.Closed, IntervalEndpoint.Open),
                new Vector2D(0.01, 0.01)));
            gridResolutions.Add(new GridResolution(
                new Interval(2000000, 20000000, IntervalEndpoint.Closed, IntervalEndpoint.Open),
                new Vector2D(0.05, 0.05)));
            gridResolutions.Add(new GridResolution(
                new Interval(20000000, double.MaxValue, IntervalEndpoint.Closed, IntervalEndpoint.Open),
                new Vector2D(0.1, 0.1)));

            _globe = new LatitudeLongitudeGridGlobe(context);
            _globe.Texture = context.Device.CreateTexture2D("NE2_50M_SR_W_4096.jpg", TextureFormat.RedGreenBlue8, false);
            _globe.Shape = globeShape;
            _globe.GridResolutions = new GridResolutionCollection(gridResolutions);

            ///////////////////////////////////////////////////////////////////
            
            Vector3D tyumenLocation = globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(65.5272), Trig.ToRadians(57.1522), 0));

            TextureAtlas atlas = new TextureAtlas(new SKBitmap[]
            {
                SKBitmap.Decode("building.png"),
                context.Device.CreateBitmapFromText("Tyumen", context.Device.CreateDefaultFont(24))
            });

            m_TyumenLabel = new BillboardCollection(context);
            m_TyumenLabel.Texture = context.Device.CreateTexture2D(atlas.Bitmap, TextureFormat.RedGreenBlueAlpha8, false);
            m_TyumenLabel.DepthTestEnabled = false;
            m_TyumenLabel.Add(new Billboard()
            {
                Position = tyumenLocation,
                TextureCoordinates = atlas.TextureCoordinates[0]
            });
            m_TyumenLabel.Add(new Billboard()
            {
                Position = tyumenLocation,
                TextureCoordinates = atlas.TextureCoordinates[1],
                HorizontalOrigin = HorizontalOrigin.Left
            });

            atlas.Dispose();
        }

        public override void Render(Context context)
        {
            
            context.Clear(ClearState);

            _globe.Render(context, SceneState);
            m_TyumenLabel.Render(context, SceneState);
        }
        private LatitudeLongitudeGridGlobe _globe;
        private BillboardCollection m_TyumenLabel;
    }
}